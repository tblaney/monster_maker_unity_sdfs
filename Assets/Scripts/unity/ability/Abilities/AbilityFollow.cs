namespace snorri
{
    using UnityEngine;
    
    public class AbilityFollow : Ability
    {
        Vec offset;

        float movementSpeed;

        Point target;

        Vec position;

        protected override void Setup()
        {
            base.Setup();

            movementSpeed = Vars.Get<float>("movement_speed", 8f);
            offset = new Vec(Vars.Get<Bag<float>>("offset", new Bag<float>(0f, 0f, 0f)).All());
        }
        protected override void Launch()
        {
            base.Launch();

            string nodeTarget = Vars.Get<string>("target", "stage_overworld.player");
            target = NODE.Tree.Get<Node>(nodeTarget).Point;
        }

        public override bool CanRun()
        {
            if (target == null)
            {
                string nodeTarget = Vars.Get<string>("target", "stage_overworld.player");
                target = NODE.Tree.Get<Node>(nodeTarget).Point;
                return false;
            }
            return true;
        }

        public override void Begin() { 
            base.Begin(); 
            position = Node.Point.Position;
        }
        public override void End() { base.End(); }
        public override void TickPhysicsAbility() { 
            base.TickPhysicsAbility(); 
            Node.Point.Position = position.Lerp(target.Position.Add(offset), TIME.DeltaPhysics*movementSpeed);

        }
        public override void TickAbility() { 
            base.TickAbility();
        }
    }
}