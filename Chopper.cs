using Sansar.Script;
using Sansar.Simulation;
using System;

namespace Kitchen
{
    [RegisterReflective]

    public class Chopper : SceneObjectScript
    {
        [DisplayName("Name")]
        [EditorVisible]
        public string Name = "axe";

        [EditorVisible]
        private RigidBodyComponent RigidBody = null;

        public Chopper()
        {

        }

        public override void Init()
        {
            if (ObjectPrivate.TryGetFirstComponent(out RigidBody))
            {
                RigidBody.Subscribe(CollisionEventType.RigidBodyContact, OnChop);
            }
            else
            {
                Log.Write(LogLevel.Error, "Choppable couldn't find a RigidBody component.  That component is needed in order to detect when an avatar walks into them");
                return;
            }

        }

        void OnChop(CollisionData data)
        {
            Log.Write(LogLevel.Info, $"Chopper chopped: {data.HitObject.ObjectId}");
        }
        public string GetName()
        {
            return Name;
        }

        public ObjectId ObjectIdReflected()
        {
            return ObjectPrivate.ObjectId;
        }
    }
}
