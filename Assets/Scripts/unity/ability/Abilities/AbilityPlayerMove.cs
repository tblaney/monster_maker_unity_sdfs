namespace snorri
{
    using UnityEngine;

    public class AbilityPlayerMove : AbilityPlayer
    {
        float rotationSpeed;
        float verticalMovementSpeed;
        float yFactor;

        float yForce;

        protected override void Setup()
        {
            base.Setup();
            rotationSpeed = Vars.Get<float>("rotation_speed", 8f);
            verticalMovementSpeed = Vars.Get<float>("vertical_movement_speed", 10f);
            yFactor = Vars.Get<float>("y_factor", 10f);
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

            if (camera == null)
                return;

            Map args = new Map();

            args.Set<bool>("is_gravity", false);
            Vec input = GAME.Vars.Get<Vec>("input:direction", new Vec());
            Vec moveDir = Node.Body.GetWorldDirectionFromInput(input, this.camera.Node.Point);
            
            float yForceCurrent = GetYForce();

            Map castArgs = new Map();
            Vector3 origin = Node.transform.position + new Vector3(moveDir.x, 20f, moveDir.z);
            castArgs.Set<Bag<float>>("args:origin", new Bag<float>(origin.x, origin.y, origin.z));
            Cast terrainCast = caster.GetCast("terrain", castArgs);
            if (input.Magnitude() > 0.1f && terrainCast.Hits > 0)
            {
                Map m = terrainCast.GetClosest(Node.Point.Position);

                if (m.Get<Vec>("hit_position").y > Node.Point.Position.y + 0.1f)
                    yForceCurrent = yFactor;
                
                args.Set<float>("y_force", yForceCurrent);
            }

            this.yForce = Mathf.Lerp(this.yForce, yForceCurrent, TIME.DeltaPhysics*verticalMovementSpeed);
            args.Set<float>("y_force", this.yForce);

            Node.Body.MoveTowardsDirection(moveDir, args);
        }
        public void Move(Vec moveDir, Cast cast, Map args)
        {
            Vec normal = cast.GetNormal(0);
            float dot = normal.Dot(new Vec(0f,1f,0f));

            Vec forward = moveDir;
            Vec perpendicular = normal.Cross(forward).Normalize();
            Vec finalForward = perpendicular.Cross(normal).Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(finalForward.vec3, normal.vec3);
        
            Node.Body.MoveTowardsDirection(finalForward, args);
        }
        public override void TickAbility() { 
            base.TickAbility(); 
        }
        private int GetYForce()
        {
            if (!body.IsGrounded)
                return -5;
            
            Cast groundCast = caster["grounded"];
            if (groundCast == null || groundCast.Hits == 0)
            {
                return -5;
            }

            //float groundedDistance = c.Distance;

            return -1;
        }
    }
}