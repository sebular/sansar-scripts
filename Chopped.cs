using Sansar.Script;
using Sansar.Simulation;
using System;

namespace Kitchen
{
    [RegisterReflective]
    public class Chopped : SceneObjectScript
    {
        [DisplayName("Name")]
        [EditorVisible]
        public string Name = "chopped";

        [EditorVisible]
        private RigidBodyComponent RigidBody = null;

        public Chopped()
        {

        }

        public override void Init()
        {

            if (!ObjectPrivate.TryGetFirstComponent(out RigidBody))
            {
                Log.Write(LogLevel.Error, "Chopped couldn't find a RigidBody component.  That component is needed in order to detect when an avatar walks into them");
                return;
            }
        }

        public string GetName()
        {
            return Name;
        }

        public ObjectId ObjectIdReflected()
        {
            return ObjectPrivate.ObjectId;
        }

        public void SetPosition(Sansar.Vector position)
        {
            WaitFor(RigidBody.SetPosition, position);
            Log.Write(LogLevel.Info, $"Position of chopped bit was set to: ({position.X}, {position.Y}, {position.Z})");
        }
    }
}
