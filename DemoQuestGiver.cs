using Sansar.Script;
using Sansar.Simulation;
using Sansar.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DemoQuest
{
    public class DemoQuestGiver : SceneObjectScript
    {

        [DisplayName("Character ID")]
        [EditorVisible]
        public string CharacterId = null;

        [DisplayName("Default Greeting Text")]
        [EditorVisible]
        public string GreetingText = "Default Greeting Text";

        [DisplayName("Complete Objective Handle")]
        [EditorVisible]
        public string CompleteObjectiveHandle = "finish-quest";

        [DisplayName("Grid")]
        [EditorVisible]
        public string Grid = "staging";

        [DefaultValue("")]
        public Interaction GiveQuest;

        public string BaseUrl;

        public override void Init()
        {
            if (Grid.Equals("production"))
            {
                Grid = "";
            }
            else
            {
                Grid = $".{Grid}";
            }
            BaseUrl = $"https://profiles-api{Grid}.sansar.com";
            GiveQuest.Subscribe((InteractionData idata) =>
            {
                GiveQuest.SetPrompt($"Character: {CharacterId}");
                AgentPrivate Quester = ScenePrivate.FindAgent(idata.AgentId);

                List<string> CompletedQuestIds = CompleteAnyQuests(Quester);
                if (CompletedQuestIds.Count > 0)
                {
                    return;
                }

                List<string> AvailableQuestIds = GetAvailableQuests(Quester);
                if (AvailableQuestIds.Count > 0)
                {
                    string firstQuestId = AvailableQuestIds[0];
                    OfferQuest(Quester, firstQuestId);

                    return;
                }

                Quester.SendChat(GreetingText);
            });
        }

        public List<string> CompleteAnyQuests(AgentPrivate Quester)
        {
            List<string> QuestIds = new List<string>();

            HttpRequestOptions options = new HttpRequestOptions();
            options.Method = HttpRequestMethod.PATCH;
            options.Headers = new Dictionary<string, string>()
            {
                {"content-type", "application/json" }
            };
            options.Body = $"{{\"data\": {{\"state\":\"COMPLETED\"}} }}";

            Guid PersonaId = Quester.AgentInfo.AvatarUuid;
            string completeQuestsUrl = $"{BaseUrl}/players/{PersonaId}/characters/{CharacterId}/objectives/{CompleteObjectiveHandle}";
            Quester.SendChat(completeQuestsUrl);

            var result = WaitFor(ScenePrivate.HttpClient.Request, completeQuestsUrl, options) as HttpClient.RequestData;
            if (!result.Success || result.Response.Status != 200)
            {
                return QuestIds;
            }

            string jsonResponse = result.Response.Body;
            Quester.SendChat($"{jsonResponse}");
            StorylineResponse parsed = ((JsonSerializationData<StorylineResponse>)(WaitFor(JsonSerializer.Deserialize<StorylineResponse>, jsonResponse))).Object;
            Quester.SendChat(parsed.ToString());

            foreach(QuestData d in parsed.data)
            {
                QuestIds.Add(d.id);

            }
            return QuestIds;
        }

        public List<string> GetAvailableQuests(AgentPrivate Quester)
        {
            List<string> QuestIds = new List<string>();

            HttpRequestOptions options = new HttpRequestOptions();
            options.Method = HttpRequestMethod.GET;

            Guid PersonaId = Quester.AgentInfo.AvatarUuid;
            string availableQuestsUrl = $"{BaseUrl}/players/{PersonaId}/characters/{CharacterId}/quest-definitions";
            Quester.SendChat($"{availableQuestsUrl}");
            var result = WaitFor(ScenePrivate.HttpClient.Request, availableQuestsUrl, options) as HttpClient.RequestData;
            if (!result.Success || result.Response.Status != 200)
            {
                return QuestIds;
            }

            string jsonResponse = result.Response.Body;
            Quester.SendChat($"{jsonResponse}");
            StorylineResponse parsed = ((JsonSerializationData<StorylineResponse>)(WaitFor(JsonSerializer.Deserialize<StorylineResponse>, jsonResponse))).Object;
            Quester.SendChat(parsed.ToString());
            foreach(QuestData d in parsed.data)
            {
                QuestIds.Add(d.id);

            }
            return QuestIds;
        }

        public void OfferQuest(AgentPrivate Quester, string QuestId)
        {
            HttpRequestOptions options = new HttpRequestOptions();
            options.Method = HttpRequestMethod.POST;
            options.Headers = new Dictionary<string, string>()
            {
                {"content-type", "application/json" }
            };
            Guid PersonaId = Quester.AgentInfo.AvatarUuid;
            options.Body = $"{{\"data\": {{\"questDefinitionId\":\"{QuestId}\"}} }}";
            var result = WaitFor(ScenePrivate.HttpClient.Request, $"{BaseUrl}/players/{PersonaId}/quests", options) as HttpClient.RequestData;
            if (result.Success)
            {
                Quester.SendChat($"{result.Response.Body}");
            }
        }
    }

    public class QuestData
    {
        public string id;
        public string title;
        public List<ObjectiveData> objectiveDefinitions;
    }

    public class ObjectiveData
    {
        public string handle;
        public string state;
    }

    public class StorylineResponse
    {
        public List<QuestData> data;
    }

    public class QuestResponseData
    {
        public string id;
        public string title;
        public List<ObjectiveResponseData> objectives;
    }

    public class ObjectiveResponseData
    {
        public string handle;
        public string state;
    }

    public class QuestResponse
    {
        public QuestResponseData data;
    }
}
