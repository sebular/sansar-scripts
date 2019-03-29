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

        [DisplayName("Storyline ID")]
        [EditorVisible]
        public string StorylineId = null;

        [DisplayName("Grid")]
        [EditorVisible]
        public string Grid = "staging";

        [DefaultValue("Get this sweet quest")]
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
                GiveQuest.SetPrompt($"Storyline: {StorylineId}");
                AgentPrivate Quester = ScenePrivate.FindAgent(idata.AgentId);
                GetAvailableQuests(Quester);
            });
        }

        public void GetAvailableQuests(AgentPrivate Quester)
        {
            HttpRequestOptions options = new HttpRequestOptions();
            options.Method = HttpRequestMethod.GET;

            Guid PersonaId = Quester.AgentInfo.AvatarUuid;
            string availableQuestsUrl = $"{BaseUrl}/players/{PersonaId}/storylines/{StorylineId}/quests";
            Quester.SendChat($"{availableQuestsUrl}");
            var result = WaitFor(ScenePrivate.HttpClient.Request, availableQuestsUrl, options) as HttpClient.RequestData;
            if (!result.Success || result.Response.Status != 200)
            {
                return;
            }

            string jsonResponse = result.Response.Body;
            Quester.SendChat($"{jsonResponse}");
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions
            {
                SerializeReferences = false
            };
            StorylineResponse parsed = ((JsonSerializationData<StorylineResponse>)(WaitFor(JsonSerializer.Deserialize<StorylineResponse>, jsonResponse, jsonOptions))).Object;
            Quester.SendChat(parsed.ToString());
            Quester.SendChat(parsed.data.ToString());
            Quester.SendChat(parsed.data[0].ToString());
            Quester.SendChat(parsed.data[0].id.ToString());

            string QuestId = parsed.data[0].id;
            string QuestTitle = parsed.data[0].title;
            Quester.SendChat($"questID: {QuestId}");
            Quester.SendChat($"questTitle: {QuestTitle}");
            OfferQuest(Quester, QuestId);
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
