using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kitchen
{
    public class Choppable : SceneObjectScript
    {
        [EditorVisible]
        private RigidBodyComponent RigidBody = null;

        public interface INamedRigidThing
        {
            string GetName();
            ObjectId ObjectIdReflected();
            void SetPosition(Sansar.Vector position);
        }

        public interface IChopper
        {
            string GetName();
            ObjectId ObjectIdReflected();
        }

        private List<INamedRigidThing> Pieces = new List<INamedRigidThing>();
        private IChopper Axe;

        public Choppable()
        {

        }

        public override void Init()
        {

            if (ObjectPrivate.TryGetFirstComponent(out RigidBody))
            {
                RigidBody.Subscribe(CollisionEventType.RigidBodyContact, OnCollision);
                RigidBody.SetMotionType(RigidBodyMotionType.MotionTypeDynamic);
            }
            else
            {
                Log.Write(LogLevel.Error, "Choppable couldn't find a RigidBody component.  That component is needed in order to detect when an avatar walks into them");
                return;
            }
            Pieces = ScenePrivate.FindReflective<INamedRigidThing>("Kitchen.Chopped").ToList<INamedRigidThing>();
            Log.Write(LogLevel.Info, $"Found {Pieces.Count()} chopped bits");
            Axe = ScenePrivate.FindReflective<IChopper>("Kitchen.Chopper").FirstOrDefault();
            Log.Write(LogLevel.Info, $"Found Axe: {Axe.GetName()}");
        }

        void OnCollision(CollisionData data)
        {
            Log.Write(LogLevel.Info, $"Choppable Struck by: {data.HitObject.ObjectId}");
            if (data.HitObject.ObjectId != Axe.ObjectIdReflected())
            {
                return;
            }
            Sansar.Vector pos = RigidBody.GetPosition();
            WaitFor(RigidBody.SetPosition, new Sansar.Vector(0, 2, 0));
            foreach (INamedRigidThing p in Pieces)
            {
                p.SetPosition(pos);
            }
            Log.Write(LogLevel.Info, $"Done setting stuff to ({pos.X}, {pos.Y}, {pos.Z})");
        }
    }
}
