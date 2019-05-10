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
            ScenePrivate.CreateClusterData SpawnCube(ShapeDefinition definition);
            ScenePrivate.CreateClusterData SpawnRamp(ShapeDefinition definition);
            ScenePrivate.CreateClusterData SpawnGlobalObjective(ObjectiveEntity entity);
        }

        [DisplayName("Level URL")]
        [EditorVisible]
        public string LevelUrl = "";

        [DisplayName("Bit URL")]
        [EditorVisible]
        public string BitUrl = "";

        private ISpawner Spawner;
        private LinkedList<WebVector> CubePositions = new LinkedList<WebVector>();
        private LinkedList<WebVector> RampPositions = new LinkedList<WebVector>();
        private LinkedList<WebVector> ObjectivePositions = new LinkedList<WebVector>();
        private LinkedList<ScenePrivate.CreateClusterData> Cubes = new LinkedList<ScenePrivate.CreateClusterData>();
        private LinkedList<ScenePrivate.CreateClusterData> Ramps = new LinkedList<ScenePrivate.CreateClusterData>();
        private LinkedList<ScenePrivate.CreateClusterData> Objectives = new LinkedList<ScenePrivate.CreateClusterData>();

        public override void Init()
        {
            Spawner = ScenePrivate.FindReflective<ISpawner>("Builder.Spawner").FirstOrDefault();
            if (Spawner == null)
            {
                Log.Write(LogLevel.Error, "Fetcher failed to find the Spawner :(");
            }

            ScenePrivate.Chat.Subscribe(Chat.DefaultChannel, OnChat, true);
        }

        public void FetchBit(string bitName = "default")
        {
            HttpRequestOptions options = new HttpRequestOptions();
            options.Method = HttpRequestMethod.GET;

            var result = WaitFor(ScenePrivate.HttpClient.Request, $"{BitUrl}/{bitName}", options) as HttpClient.RequestData;
            if (!result.Success || result.Response.Status != 200)
            {
                Log.Write(LogLevel.Error, $"Bad request fetching bit, {result.Response.Status}");
                return;
            }

            string jsonResponse = result.Response.Body;
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions
            {
                SerializeReferences = false
            };
            BitResponse parsed = ((JsonSerializationData<BitResponse>)(WaitFor(JsonSerializer.Deserialize<BitResponse>, jsonResponse, jsonOptions))).Object;

            foreach (ShapeDefinition definition in parsed.data.cubes)
            {
                var match = CubePositions.Find(definition.p);
                if (match == null)
                {
                    ScenePrivate.CreateClusterData data = Spawner.SpawnCube(definition);
                    Cubes.AddLast(data);
                    CubePositions.AddLast(definition.p);
                }
            }

            foreach (ShapeDefinition definition in parsed.data.ramps)
            {
                var match = RampPositions.Find(definition.p);
                if (match == null)
                {
                    ScenePrivate.CreateClusterData data = Spawner.SpawnRamp(definition);
                    Ramps.AddLast(data);
                    RampPositions.AddLast(definition.p);
                }
            }
            
        }

        public void FetchLevel(string levelName = "default")
        {
            HttpRequestOptions options = new HttpRequestOptions();
            options.Method = HttpRequestMethod.GET;

            var result = WaitFor(ScenePrivate.HttpClient.Request, $"{LevelUrl}/{levelName}", options) as HttpClient.RequestData;
            if (!result.Success || result.Response.Status != 200)
            {
                Log.Write(LogLevel.Error, $"Bad request fetching level, {result.Response.Status}");
                return;
            }

            string jsonResponse = result.Response.Body;
            LevelResponse parsed = ((JsonSerializationData<LevelResponse>)(WaitFor(JsonSerializer.Deserialize<LevelResponse>, jsonResponse))).Object;
            foreach (BitDefinition bitDefinition in parsed.data.bits)
            {
                FetchBit(bitDefinition.name);
            }

            foreach (ObjectiveEntity entity in parsed.data.objectives)
            {
                var match = ObjectivePositions.Find(entity.definition.p);
                if (match == null)
                {
                    ScenePrivate.CreateClusterData data = Spawner.SpawnGlobalObjective(entity);
                    Objectives.AddLast(data);
                    ObjectivePositions.AddLast(entity.definition.p);
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
                CubePositions.Clear();

                foreach(ScenePrivate.CreateClusterData data in Ramps)
                {
                    if (data != null && data.ClusterReference != null)
                    {
                        data.ClusterReference.Destroy();
                    }
                }
                Ramps.Clear();
                RampPositions.Clear();

                foreach(ScenePrivate.CreateClusterData data in Objectives)
                {
                    if (data != null && data.ClusterReference != null)
                    {
                        data.ClusterReference.Destroy();
                    }
                }
                Objectives.Clear();
                ObjectivePositions.Clear();
            }
            else if (cmds[0] == "/level")
            {
                if (cmds.Length == 1)
                {
                    FetchLevel();
                }
                else if (cmds.Length == 2)
                {
                    FetchLevel(cmds[1]);
                }
            }
            else if (cmds[0] == "/bit")
            {
                if (cmds.Length == 1)
                {
                    FetchBit();
                }
                else if (cmds.Length == 2)
                {
                    FetchBit(cmds[1]);
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

    public class ShapeDefinition
    {
        public WebVector p;
        public WebVector r;
        public string s;
        public string c;
    }

    public class ObjectiveEntity
    {
        public ShapeDefinition definition;
        public string handle;
        public string storylineId;
        public string prompt;
    }

    public class BitData 
    {
        public List<ShapeDefinition> cubes;
        public List<ShapeDefinition> ramps;
    }

    public class BitResponse 
    {
        public BitData data;
    }

    public class BitDefinition
    {
        public string name;
    }
    public class LevelData
    {
        public List<BitDefinition> bits;
        public List<ObjectiveEntity> objectives;
    }
    public class LevelResponse
    {
        public LevelData data;
    }
}
