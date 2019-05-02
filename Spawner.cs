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

namespace Builder
{
    [RegisterReflective]
    public class Spawner : SceneObjectScript
    {
        [DisplayName("Cube")]
        [EditorVisible]
        public ClusterResource cube = null;
        public ClusterResource ramp = null;

        public override void Init()
        {

        }

        public ScenePrivate.CreateClusterData SpawnCube(WebVector location)
        {
            Vector position = new Vector(location.x, location.y, location.z);
            ScenePrivate.CreateClusterData createData = null;
            createData = (ScenePrivate.CreateClusterData)WaitFor(ScenePrivate.CreateCluster, cube, position, Quaternion.Identity, Vector.Zero);
            return createData;
        }

        public ScenePrivate.CreateClusterData SpawnRamp(ShapePosition position)
        {
            Vector location = new Vector(position.location.x, position.location.y, position.location.z);
            Vector eulerRotation = new Vector(position.rotation.x, position.rotation.y, position.rotation.z);

            Quaternion rotation = Quaternion.FromEulerAngles(eulerRotation);
            ScenePrivate.CreateClusterData createData = null;

            createData = (ScenePrivate.CreateClusterData)WaitFor(ScenePrivate.CreateCluster, ramp, location, rotation, Vector.Zero);
            return createData;
        }
    }
}
