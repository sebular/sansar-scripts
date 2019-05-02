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
            ScenePrivate.CreateClusterData SpawnCube(WebVector location);
            ScenePrivate.CreateClusterData SpawnRamp(ShapePosition position);
        }

        [DisplayName("Level URL")]
        [EditorVisible]
        public string LevelUrl = "";

        public Interaction FetchAction;

        private ISpawner Spawner;
        private LinkedList<WebVector> CubeLocations = new LinkedList<WebVector>();
        private LinkedList<WebVector> RampLocations = new LinkedList<WebVector>();
        private LinkedList<ScenePrivate.CreateClusterData> Cubes = new LinkedList<ScenePrivate.CreateClusterData>();
        private LinkedList<ScenePrivate.CreateClusterData> Ramps = new LinkedList<ScenePrivate.CreateClusterData>();

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

            foreach (WebVector location in parsed.data.cubes)
            {
                var match = CubeLocations.Find(location);
                if (match == null)
                {
                    ScenePrivate.CreateClusterData data = Spawner.SpawnCube(location);
                    Cubes.AddLast(data);
                }
            }
        }

        public void FetchLevelChat(string levelName = "default")
        {
            HttpRequestOptions options = new HttpRequestOptions();
            options.Method = HttpRequestMethod.GET;

            var result = WaitFor(ScenePrivate.HttpClient.Request, $"{LevelUrl}/{levelName}", options) as HttpClient.RequestData;
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

            foreach (WebVector location in parsed.data.cubes)
            {
                var match = CubeLocations.Find(location);
                if (match == null)
                {
                    ScenePrivate.CreateClusterData data = Spawner.SpawnCube(location);
                    Cubes.AddLast(data);
                    CubeLocations.AddLast(location);
                }
            }

            foreach (ShapePosition position in parsed.data.ramps)
            {
                var match = RampLocations.Find(position.location);
                if (match == null)
                {
                    ScenePrivate.CreateClusterData data = Spawner.SpawnRamp(position);
                    Ramps.AddLast(data);
                    RampLocations.AddLast(position.location);
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

                foreach(ScenePrivate.CreateClusterData data in Ramps)
                {
                    if (data != null && data.ClusterReference != null)
                    {
                        data.ClusterReference.Destroy();
                    }
                }
                Ramps.Clear();
                RampLocations.Clear();
            }
            else if (cmds[0] == "/load")
            {
                if (cmds.Length == 1)
                {
                    FetchLevelChat();
                }
                else if (cmds.Length == 2)
                {
                    FetchLevelChat(cmds[1]);
                }
            }

        }
    }

    public class WebVector
    {
        public float x;
        public float y;
        public float z;

        public bool Equals(WebVector other)
        {
            return this.x == other.x && this.y == other.y && this.z == other.z;
        }

    }

    public class ShapePosition
    {
        public WebVector location;
        public WebVector rotation;
    }

    public class LevelData
    {
        public List<WebVector> cubes;
        public List<ShapePosition> ramps;
    }

    public class LevelResponse
    {
        public LevelData data;
    }
}
