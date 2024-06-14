namespace snorri
{
    using UnityEngine;
    
    public class AbilityFollowSibling : Ability
    {
        Vec offset;

        float movementSpeed;
        float rotationSpeed;

        Point target;

        Vec position;
        Vec quaternion;

        protected override void Setup()
        {
            base.Setup();

            movementSpeed = Vars.Get<float>("movement_speed", 8f);
            rotationSpeed = Vars.Get<float>("rotation_speed", 4f);
            offset = new Vec(Vars.Get<Bag<float>>("offset", new Bag<float>(0f, 0f, 0f)).All());
        }
        protected override void Launch()
        {
            base.Launch();

            string nodeTarget = Node.Parent + "." + Vars.Get<string>("sibling", "");
            target = NODE.Tree.Get<Node>(nodeTarget).Point;
        }

        public override bool CanRun()
        {
            if (target == null)
            {
                string nodeTarget = Node.Parent + "." + Vars.Get<string>("sibling", "");
                LOG.Console("ability follow sibling looking for node: " + nodeTarget);
                target = NODE.Tree.Get<Node>(nodeTarget).Point;
                return false;
            }
            return true;
        }

        public override void Begin() { 
            base.Begin(); 
            position = Node.Point.Position;
            quaternion = Node.Point.Rotation;
        }
        public override void End() { base.End(); }
        public override void TickPhysicsAbility() { 
            base.TickPhysicsAbility(); 
            position = position.Lerp(target.Position.Add(offset), TIME.DeltaPhysics*movementSpeed);
            Node.Point.Position = position;

            quaternion = quaternion.Lerp(target.Rotation, TIME.DeltaPhysics*rotationSpeed, is_quaternion:true);
            Node.Point.Rotation = quaternion;
        }
        public override void TickAbility() { 
            base.TickAbility();
        }
    }
}