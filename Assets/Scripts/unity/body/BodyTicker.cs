namespace snorri
{
    using UnityEngine;

    public class BodyTicker : Ticker
    {
        public bool IsGrounded {
            get { return Vars.Get<bool>("is_grounded", false); }
            set { Vars.Set<bool>("is_grounded", value); }
        }

        float groundedThreshold;

        BodyModule module;
        Body body;

        CastTicker caster;

        protected override void Setup()
        {
            base.Setup();

            module = Node.GetActor<BodyModule>();

            body = new Body(module, Vars);

            Node.Body = body;

            groundedThreshold = Vars.Get<float>("grounded_threshold", 1f);
        }

        protected override void Launch()
        {
            base.Launch();

            caster = Node.GetActor<CastTicker>();
        }

        public override void Tick()
        {
            base.Tick();

            RefreshGrounded();
        }
        public override void TickPhysics()
        {
            base.TickPhysics();
        }

        void RefreshGrounded()
        {
            Cast cast = caster["grounded"];
            if (cast == null)
            {
                LOG.Console("body ticker found no relevant cast");
                IsGrounded = false;
                return;
            }

            if (cast.Hits == 0)
            {
                LOG.Console("body ticker found no hits!");
                IsGrounded = false;
                return;
            }

            float distance = cast.Distance;

            if (distance > 0.2f)
            {
                IsGrounded = false;
            } else
            {
                IsGrounded = true;
            }

            LOG.Console($"body ticker is grounded: {IsGrounded}, {distance}");
        }
    }
}