/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Collections.Generic;

namespace Builder
{
    [RegisterReflective]
    public class Spawner : SceneObjectScript
    {
        [DisplayName("Cube White Full")]
        [EditorVisible]
        public ClusterResource cubeWhiteFull = null;

        [DisplayName("Cube White Half")]
        [EditorVisible]
        public ClusterResource cubeWhiteHalf = null;

        [DisplayName("Cube White Quarter")]
        [EditorVisible]
        public ClusterResource cubeWhiteQuarter = null;

        [DisplayName("Cube White Eighth")]
        [EditorVisible]
        public ClusterResource cubeWhiteEighth = null;

        [DisplayName("Cube White Sixteenth")]
        [EditorVisible]
        public ClusterResource cubeWhiteSixteenth = null;

        [DisplayName("Cube Black Full")]
        [EditorVisible]
        public ClusterResource cubeBlackFull = null;

        [DisplayName("Cube Black Half")]
        [EditorVisible]
        public ClusterResource cubeBlackHalf = null;

        [DisplayName("Cube Black Quarter")]
        [EditorVisible]
        public ClusterResource cubeBlackQuarter = null;

        [DisplayName("Cube Black Eighth")]
        [EditorVisible]
        public ClusterResource cubeBlackEighth = null;

        [DisplayName("Cube Black Sixteenth")]
        [EditorVisible]
        public ClusterResource cubeBlackSixteenth = null;

        [DisplayName("Ramp White Full")]
        [EditorVisible]
        public ClusterResource rampWhiteFull = null;

        [DisplayName("Ramp White Half")]
        [EditorVisible]
        public ClusterResource rampWhiteHalf = null;

        [DisplayName("Ramp White Quarter")]
        [EditorVisible]
        public ClusterResource rampWhiteQuarter = null;

        [DisplayName("Ramp White Eighth")]
        [EditorVisible]
        public ClusterResource rampWhiteEighth = null;

        [DisplayName("Ramp White Sixteenth")]
        [EditorVisible]
        public ClusterResource rampWhiteSixteenth = null;

        [DisplayName("Ramp Black Full")]
        [EditorVisible]
        public ClusterResource rampBlackFull = null;

        [DisplayName("Ramp Black Half")]
        [EditorVisible]
        public ClusterResource rampBlackHalf = null;

        [DisplayName("Ramp Black Quarter")]
        [EditorVisible]
        public ClusterResource rampBlackQuarter = null;

        [DisplayName("Ramp Black Eighth")]
        [EditorVisible]
        public ClusterResource rampBlackEighth = null;

        [DisplayName("Ramp Black Sixteenth")]
        [EditorVisible]
        public ClusterResource rampBlackSixteenth = null;

        [DisplayName("Objective")]
        [EditorVisible]
        public ClusterResource objectiveCluster = null;

        [DisplayName("Grid")]
        [EditorVisible]
        public string Grid = "staging";

        string BaseUrl;
        Dictionary<string, Dictionary<string, ClusterResource>> cubes = new Dictionary<string, Dictionary<string, ClusterResource>>();
        Dictionary<string, Dictionary<string, ClusterResource>> ramps = new Dictionary<string, Dictionary<string, ClusterResource>>();
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

            Dictionary<string, ClusterResource> whiteCubeSizes = new Dictionary<string, ClusterResource>()
            {
                { "full",  cubeWhiteFull },
                { "half", cubeWhiteHalf },
                { "quarter", cubeWhiteQuarter },
                { "eighth", cubeWhiteEighth },
                { "sixteenth", cubeWhiteSixteenth }
            };

            Dictionary<string, ClusterResource> blackCubeSizes = new Dictionary<string, ClusterResource>()
            {
                { "full",  cubeBlackFull },
                { "half", cubeBlackHalf },
                { "quarter", cubeBlackQuarter },
                { "eighth", cubeBlackEighth },
                { "sixteenth", cubeBlackSixteenth }
            };

            cubes.Add("white", whiteCubeSizes);
            cubes.Add("black", blackCubeSizes);

            Dictionary<string, ClusterResource> whiteRampSizes = new Dictionary<string, ClusterResource>()
            {
                { "full",  rampWhiteFull },
                { "half", rampWhiteHalf },
                { "quarter", rampWhiteQuarter },
                { "eighth", rampWhiteEighth },
                { "sixteenth", rampWhiteSixteenth }
            };

            Dictionary<string, ClusterResource> blackRampSizes = new Dictionary<string, ClusterResource>()
            {
                { "full",  rampBlackFull },
                { "half", rampBlackHalf },
                { "quarter", rampBlackQuarter },
                { "eighth", rampBlackEighth },
                { "sixteenth", rampBlackSixteenth }
            };

            ramps.Add("white", whiteRampSizes);
            ramps.Add("black", blackRampSizes);
        }

        public ScenePrivate.CreateClusterData SpawnCube(ShapeDefinition definition)
        {
            Vector position = new Vector(definition.p.x, definition.p.y, definition.p.z);
            ScenePrivate.CreateClusterData createData = null;

            ClusterResource cube = cubes[definition.c][definition.s];
            createData = (ScenePrivate.CreateClusterData)WaitFor(ScenePrivate.CreateCluster, cube, position, Quaternion.Identity, Vector.Zero);
            return createData;
        }

        public ScenePrivate.CreateClusterData SpawnRamp(ShapeDefinition definition)
        {
            Vector location = new Vector(definition.p.x, definition.p.y, definition.p.z);
            Vector eulerRotation = new Vector(definition.r.x, definition.r.y, definition.r.z);

            Quaternion rotation = Quaternion.FromEulerAngles(eulerRotation);
            ScenePrivate.CreateClusterData createData = null;

            ClusterResource ramp = ramps[definition.c][definition.s];
            createData = (ScenePrivate.CreateClusterData)WaitFor(ScenePrivate.CreateCluster, ramp, location, rotation, Vector.Zero);
            return createData;
        }

        public ScenePrivate.CreateClusterData SpawnGlobalObjective(ObjectiveEntity entity)
        {
            Vector location = new Vector(entity.definition.p.x, entity.definition.p.y, entity.definition.p.z);
            Vector eulerRotation = new Vector(entity.definition.r.x, entity.definition.r.y, entity.definition.r.z);

            Quaternion rotation = Quaternion.FromEulerAngles(eulerRotation);
            ScenePrivate.CreateClusterData createData = null;

            createData = (ScenePrivate.CreateClusterData)WaitFor(ScenePrivate.CreateCluster, objectiveCluster, location, rotation, Vector.Zero);
            ObjectPrivate.AddInteractionData addData = (ObjectPrivate.AddInteractionData)WaitFor(createData.ClusterReference.GetObjectPrivate(0).AddInteraction, entity.prompt, true);
            addData.Interaction.Subscribe((InteractionData data) =>
            {
                AgentPrivate Quester = ScenePrivate.FindAgent(data.AgentId);
                if (Quester == null || !Quester.IsValid)
                {
                    return;
                }
                HttpRequestOptions options = new HttpRequestOptions();
                options.Method = HttpRequestMethod.PATCH;
                options.Headers = new Dictionary<string, string>()
                {
                    { "content-type", "application/json" }
                };
                options.Body = $"{{\"data\": {{\"state\":\"COMPLETED\"}} }}";
                Guid PersonaId = Quester.AgentInfo.AvatarUuid;
                string completeObjectiveUrl = $"{BaseUrl}/players/{PersonaId}/storylines/{entity.storylineId}/objectives/{entity.handle}";
                Quester.SendChat(completeObjectiveUrl);

                var result = WaitFor(ScenePrivate.HttpClient.Request, completeObjectiveUrl, options) as HttpClient.RequestData;
            });

            return createData;
        }
    }
}
