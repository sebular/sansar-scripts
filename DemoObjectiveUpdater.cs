using Sansar.Script;
using Sansar.Simulation;
using Sansar.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DemoQuest
{
    public class DemoObjectiveUpdater : SceneObjectScript
    {

        [DisplayName("Storyline ID")]
        [EditorVisible]
        public string StorylineId = null;

        [DisplayName("Grid")]
        [EditorVisible]
        public string Grid = "staging";

        [DisplayName("Update to State")]
        [EditorVisible]
        public string State = "COMPLETED";

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
            if (parsed.data.Count == 0)
            {
                return;
            }
            Quester.SendChat(parsed.ToString());
            Quester.SendChat(parsed.data.ToString());
            Quester.SendChat(parsed.data[0].ToString());
            Quester.SendChat(parsed.data[0].id.ToString());

            string QuestId = parsed.data[0].id;
            string QuestTitle = parsed.data[0].title;
            Quester.SendChat($"questID: {QuestId}");
            Quester.SendChat($"questTitle: {QuestTitle}");
            Quester.SendChat($"questObjectives: {parsed.data[0].objectiveDefinitions.ToString()}");

            GetQuest(Quester, QuestId);
        }
        public void GetQuest(AgentPrivate Quester, string QuestId)
        {
            HttpRequestOptions options = new HttpRequestOptions();
            options.Method = HttpRequestMethod.GET;
            Guid PersonaId = Quester.AgentInfo.AvatarUuid;
            var result = WaitFor(ScenePrivate.HttpClient.Request, $"{BaseUrl}/players/{PersonaId}/quests/{QuestId}", options) as HttpClient.RequestData;
            if (!result.Success || result.Response.Status != 200)
            {
                return;
            }

            string jsonResponse = result.Response.Body;
            Quester.SendChat($"Quest response: {jsonResponse}");
            QuestResponse parsed = ((JsonSerializationData<QuestResponse>)(WaitFor(JsonSerializer.Deserialize<QuestResponse>, jsonResponse))).Object;
            ObjectiveResponseData objectiveData = null;
            if (State == "ACTIVE")
            {
                for(int i = parsed.data.objectives.Count - 1; i >=0; i--)
                {
                    ObjectiveResponseData d = parsed.data.objectives[i];
                    if (d.state == "COMPLETED")
                    {
                        objectiveData = d;
                        break;
                    }
                }
            }
            else if (State == "COMPLETED")
            {
                foreach (ObjectiveResponseData d in parsed.data.objectives)
                {
                    if (d.state == "ACTIVE")
                    {
                        objectiveData = d;
                        break;
                    }
                }
            }
            Quester.SendChat($"objectiveData: {objectiveData}");
            UpdateObjectiveStatus(Quester, QuestId, objectiveData);
        }

        public void UpdateObjectiveStatus(AgentPrivate Quester, string QuestId, ObjectiveResponseData objective)
        {
            if (objective == null)
            {
                return;
            }
            HttpRequestOptions options = new HttpRequestOptions();
            options.Method = HttpRequestMethod.PATCH;
            options.Headers = new Dictionary<string, string>()
            {
                {"content-type", "application/json" }
            };
            Guid PersonaId = Quester.AgentInfo.AvatarUuid;
            options.Body = $"{{\"data\": {{\"state\":\"{State}\"}} }}";
            var result = WaitFor(ScenePrivate.HttpClient.Request, $"{BaseUrl}/players/{PersonaId}/quests/{QuestId}/objectives/{objective.handle}", options) as HttpClient.RequestData;
            if (result.Success)
            {
                Quester.SendChat($"{result.Response.Body}");
            }
        }

    }
}
