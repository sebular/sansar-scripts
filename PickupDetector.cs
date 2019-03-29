using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Linq;

namespace ItemCollector
{
    public class PickupDetector : SceneObjectScript
    {
        public interface IPickupTracker
        {
            void RecordPickup(AgentPrivate collectorAgent, Guid collectedItemId);
            void RecordPickupTimed(AgentPrivate collectorAgent, Guid collectedItemId);
            void RegisterPresence(Guid collectibleItemId);
        }

        private IPickupTracker PickupTracker;
        private Guid itemId;

        public override void Init()
        {
            itemId = Guid.NewGuid();
            PickupTracker = ScenePrivate.FindReflective<IPickupTracker>("ItemCollector.PickupTracker").FirstOrDefault();
            if (PickupTracker == null)
            {
                Log.Write(LogLevel.Error, "Pickup Detector couldn't find the master Pickup Tracker.  Make sure it's attached to something in the scene, like the floor.");
                return;
            }

            PickupTracker.RegisterPresence(itemId);

            RigidBodyComponent RigidBody;
            if (ObjectPrivate.TryGetFirstComponent(out RigidBody))
            {
                RigidBody.Subscribe(CollisionEventType.CharacterContact, OnPickup);
            }
            else
            {
                Log.Write(LogLevel.Error, "Pickup Detector couldn't find a RigidBody component.  That component is needed in order to detect when an avatar walks into them");
                return;
            }

        }

        void OnPickup(CollisionData data)
        {
            AgentPrivate agent = ScenePrivate.FindAgent(data.HitComponentId.ObjectId);
            if (agent != null)
            {
                PickupTracker.RecordPickupTimed(agent, itemId);
            }
        }
    }
}