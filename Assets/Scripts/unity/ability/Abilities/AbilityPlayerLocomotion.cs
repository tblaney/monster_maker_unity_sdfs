namespace snorri
{
    using UnityEngine;

    public class AbilityPlayerLocomotion : AbilityPlayer
    {
        float yFactor;
        float verticalMovementSpeed;
        float rotationSpeed;

        float yForceCurrent;
        
        protected override void Setup()
        {
            base.Setup();

            yFactor = Vars.Get<float>("y_factor", 20f);
            verticalMovementSpeed = Vars.Get<float>("vertical_speed", 16f);
            rotationSpeed = Vars.Get<float>("rotation_speed", 8f);
        }
        protected override void Launch()
        {
            base.Launch();
        }

        public override bool CanRun()
        {
            return body.IsGrounded;
        }
        public override void Begin() { 
            base.Begin(); 
        }
        public override void End() { 
            base.End(); 
        }
        public override void TickPhysicsAbility() { 
            base.TickPhysicsAbility(); 

            LOG.Console("locomotion tick!");

            if (camera == null)
                return;

            Map args = new Map();
            args.Set<bool>("is_gravity", false);
            Vec input = GAME.Vars.Get<Vec>("input:direction", new Vec());
            //LOG.Console($"locomotion input: {input.vec3}");

            Vec moveDir = Node.Body.GetWorldDirectionFromInput(input, this.camera.Node.Point);
            Vec moveDirNormalized = moveDir.Normalize();

            float yForce = GetYForce();
            Vec velocityDir = new Vec(moveDir); 

            Cast terrainCast = caster["terrain"];
            if (input.Magnitude() > 0.1f && terrainCast.Hits > 0)
            {
                Map terrainHit = terrainCast.GetClosest(Node.Point.Position);
                Vec hitPosition = terrainHit.Get<Vec>("hit_position");
                if (hitPosition.y > Node.Point.Position.y - 0.1f)
                {
                    yForce = yFactor;
                    velocityDir = velocityDir.Multiply(0.3f);
                }
            }

            yForceCurrent = Mathf.Lerp(yForceCurrent, yForce, TIME.DeltaPhysics*verticalMovementSpeed);
            
            args.Set<float>("y_force", yForceCurrent);

            if (isLog)
            {
                LOG.Console($"player locomotion has y force: {yForceCurrent}");
            }

            Node.Body.MoveTowardsDirection(velocityDir, args);
        }
        public override void TickAbility() { 
            base.TickAbility(); 

            animator.SetFloat("speed", Node.Body.SpeedCurrent);
        }

        private int GetYForce()
        {
            if (!body.IsGrounded)
                return -5;
            
            Cast groundCast = caster["grounded"];
            if (groundCast == null || groundCast.Hits == 0)
            {
                return -5;
            } else
            {
                Vec normal = groundCast.GetNormal(0);
                float dot = normal.Dot(Vec.up);
                return -(int)dot;
            }

            return -1;
        }
    }
}