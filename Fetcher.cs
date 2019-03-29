using Sansar.Script;
using Sansar.Simulation;
using Sansar.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Builder
{
    public class Fetcher : SceneObjectScript
    {

        public interface ISpawner
        {
            ScenePrivate.CreateClusterData SpawnCube(int x, int y, int z);
        }

        [DisplayName("Level URL")]
        [EditorVisible]
        public string LevelUrl = "";

        public Interaction FetchAction;

        private ISpawner Spawner;
        private LinkedList<CubeLocation> CubeLocations = new LinkedList<CubeLocation>();
        private LinkedList<ScenePrivate.CreateClusterData> Cubes = new LinkedList<ScenePrivate.CreateClusterData>();

        public override void Init()
        {
            Spawner = ScenePrivate.FindReflective<ISpawner>("Builder.Spawner").FirstOrDefault();
            if (Spawner == null)
            {
                Log.Write(LogLevel.Error, "Fetcher failed to find the Spawner :(");
            }

            FetchAction.Subscribe((InteractionData idata) =>
            {
                AgentPrivate Quester = ScenePrivate.FindAgent(idata.AgentId);
                FetchLevel(Quester);
            });

            ScenePrivate.Chat.Subscribe(Chat.DefaultChannel, OnChat, true);
        }

        public void FetchLevel(AgentPrivate Quester)
        {
            HttpRequestOptions options = new HttpRequestOptions();
            options.Method = HttpRequestMethod.GET;
            Guid PersonaId = Quester.AgentInfo.AvatarUuid;
            Quester.SendChat($"Fetching {LevelUrl}");

            var result = WaitFor(ScenePrivate.HttpClient.Request, LevelUrl, options) as HttpClient.RequestData;
            if (!result.Success || result.Response.Status != 200)
            {
                Log.Write(LogLevel.Error, $"Bad request, {result.Response.Status}");
                return;
            }

            string jsonResponse = result.Response.Body;
            Quester.SendChat($"{jsonResponse}");
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions
            {
                SerializeReferences = false
            };
            LevelResponse parsed = ((JsonSerializationData<LevelResponse>)(WaitFor(JsonSerializer.Deserialize<LevelResponse>, jsonResponse, jsonOptions))).Object;
            Quester.SendChat(parsed.ToString());

            foreach (CubeLocation location in parsed.data.cubes)
            {
                var match = CubeLocations.Find(location);
                if (match == null)
                {
                    ScenePrivate.CreateClusterData data = Spawner.SpawnCube(location.x, location.y, location.z);
                    Cubes.AddLast(data);
                }
            }
        }

        public void FetchLevelChat()
        {
            HttpRequestOptions options = new HttpRequestOptions();
            options.Method = HttpRequestMethod.GET;

            var result = WaitFor(ScenePrivate.HttpClient.Request, LevelUrl, options) as HttpClient.RequestData;
            if (!result.Success || result.Response.Status != 200)
            {
                Log.Write(LogLevel.Error, $"Bad request, {result.Response.Status}");
                return;
            }

            string jsonResponse = result.Response.Body;
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions
            {
                SerializeReferences = false
            };
            LevelResponse parsed = ((JsonSerializationData<LevelResponse>)(WaitFor(JsonSerializer.Deserialize<LevelResponse>, jsonResponse, jsonOptions))).Object;

            foreach (CubeLocation location in parsed.data.cubes)
            {
                var match = CubeLocations.Find(location);
                if (match == null)
                {
                    ScenePrivate.CreateClusterData data = Spawner.SpawnCube(location.x, location.y, location.z);
                    Cubes.AddLast(data);
                }
            }
        }

        public void OnChat(ChatData chatData)
        {
            var cmds = chatData.Message.Split(new Char[] { ' ' });
            if (cmds[0] == "/clear")
            {
                foreach(ScenePrivate.CreateClusterData data in Cubes)
                {
                    if (data != null && data.ClusterReference != null)
                    {
                        data.ClusterReference.Destroy();
                    }
                }
                Cubes.Clear();
                CubeLocations.Clear();
            }
            else if (cmds[0] == "/load")
            {
                FetchLevelChat();
            }

        }
    }

    public class CubeLocation
    {
        public int x;
        public int y;
        public int z;

        public bool Equals(CubeLocation other)
        {
            return this.x == other.x && this.y == other.y && this.z == other.z;
        }

    }
    public class LevelData
    {
        public List<CubeLocation> cubes;
    }
    public class LevelResponse
    {
        public LevelData data;
    }
}
