namespace snorri
{
    public class AbilityPlayerFalling : AbilityPlayer
    {
        float initialVelocity;
        float time;

        protected override void Setup()
        {
            base.Setup();
        }
        protected override void Launch()
        {
            base.Launch();
        }

        public override bool CanRun()
        {
            if (caster == null)
                return false;

            Cast c = caster["grounded"];

            if (c == null)
                return false;

            if (c.Hits == 0)
                return true;

            float distance = c.Distance;

            return !body.IsGrounded;
        }
        public override void Begin() { 
            base.Begin(); 

            initialVelocity = -5f;
            time = 0f;
        }
        public override void End() { 
            base.End(); 
        }
        public override void TickPhysicsAbility() { 
            base.TickPhysicsAbility(); 

            if (body.IsGrounded)
            {   
                Stop();
                return;
            }
            time += TIME.Delta;
            Vec velocity = Node.Body.Velocity;

            float y = UTIL.GetVelocity(initialVelocity, time);

            velocity.y = y;

            Node.Body.Velocity = velocity;
        }
        public override void TickAbility() { 
            base.TickAbility(); 
        }
    }
}