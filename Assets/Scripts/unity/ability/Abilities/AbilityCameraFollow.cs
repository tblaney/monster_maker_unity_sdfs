namespace snorri
{
    using UnityEngine;
    
    public class AbilityCameraFollow : AbilityCamera
    {
        Vec offset;

        float rotationSpeed;
        float movementSpeed;

        Bag<float> armHeightRange;
        Bag<float> armDistanceRange;

        float verticalRotationSpeed;

        Point target;

        bool isRotate = false;

        protected override void Setup()
        {
            base.Setup();

            offset = new Vec(Vars.Get<Bag<float>>("offset", new Bag<float>(0f, 1f, 0f)).All());

            isRotate = Vars.Get<bool>("is_rotate", false);

            rotationSpeed = Vars.Get<float>("rotation_speed", 8f);
            movementSpeed = Vars.Get<float>("movement_speed", 8f);

            armHeightRange = Vars.Get<Bag<float>>("arm_height_range", new Bag<float>(0.5f, 10f));
            armDistanceRange = Vars.Get<Bag<float>>("arm_distance_range", new Bag<float>(2f, 20f));

            verticalRotationSpeed = Vars.Get<float>("vertical_speed", 8f);

            string nodeTarget = Vars.Get<string>("target", "stage_overworld.player");
            target = NODE.Tree.Get<Node>(nodeTarget).Point;
        }
        protected override void Launch()
        {
            base.Launch();

        }

        public override bool CanRun()
        {
            return true;
        }

        public override void Begin() { 
            base.Begin(); 
            
            offset = new Vec(0,1,0);
        }
        public override void End() { base.End(); }
        public override void TickAbility() { 
            base.TickAbility(); 
        }
        public override void TickPhysicsAbility() { 
            base.TickPhysicsAbility(); 

            Node.Point.Position = Node.Point.Position.Lerp(target.Position.Add(this.offset), TIME.DeltaPhysics*movementSpeed);

            if (!isRotate)
                return;
                
            Vec mouseDelta = GAME.Vars.Get<Vec>("input:mouse_delta", new Vec());
            
            //Node.Point.Rotate(0f, mouseDelta.x*TIME.Delta*rotationSpeed, 0f, true);
            Vec angles = Node.Point.Angles;
            float yAngle = angles.y;
            float yAngleTarget = Mathf.Lerp(yAngle, yAngle + mouseDelta.x*10f, TIME.DeltaPhysics*rotationSpeed);
            Node.Point.Angles = new Vec(0f, yAngleTarget, 0f);

            if (cm == null)
            {
                LOG.Console("ability camera follow doesnt have cm for some reason");
                return;
            }

            float y = mouseDelta.y;
            if (y > 0.2f)
            {
                float factor = 1f;

                // want to move up
                float armHeight = cm.ArmHeight;
                if (armHeight < armHeightRange.y* factor)
                {
                    // can still move up
                    float armHeightNew = Mathf.Lerp(armHeight, armHeight+y*10f, TIME.DeltaPhysics*verticalRotationSpeed);
                    cm.ArmHeight = armHeightNew;
                }

                float armDistance = cm.ArmDistance;
                if (armDistance < armDistanceRange.y)
                {
                    // can still move down
                    float armDistanceNew = Mathf.Lerp(armDistance, armDistance+y*1f, TIME.DeltaPhysics*verticalRotationSpeed);
                    //cm.ArmDistance = armDistanceNew;
                } 


            } else if (y < -0.2f)
            {
                float armHeight = cm.ArmHeight;
                if (armHeight > armHeightRange.x)
                {
                    // can still move down
                    float armHeightNew = Mathf.Lerp(armHeight, armHeight+y*10f, TIME.DeltaPhysics*verticalRotationSpeed);
                    cm.ArmHeight = armHeightNew;
                } 

                float armDistance = cm.ArmDistance;
                if (armDistance > armDistanceRange.x)
                {
                    // can still move down
                    float armDistanceNew = Mathf.Lerp(armDistance, armDistance+y*1f, TIME.DeltaPhysics*verticalRotationSpeed);
                    //cm.ArmDistance = armDistanceNew;
                } 
            } else 
            {
                float armHeight = this.cm.ArmHeight;
                float armDistance = this.cm.ArmDistance;

                if (armHeight < armHeightRange.x)
                {
                    // lerp to min
                    float armHeightNew = Mathf.Lerp(armHeight, armHeightRange.x, TIME.DeltaPhysics*verticalRotationSpeed);
                    cm.ArmHeight = armHeightNew;
                } else if (armHeight > armHeightRange.y)
                {
                    // lerp to max
                    float armHeightNew = Mathf.Lerp(armHeight, armHeightRange.y, TIME.DeltaPhysics*verticalRotationSpeed);
                    cm.ArmHeight = armHeightNew;
                }

                if (armDistance < armDistanceRange.x)
                {
                    // lerp to min
                    float armDistanceNew = Mathf.Lerp(armHeight, armDistanceRange.x, TIME.DeltaPhysics*verticalRotationSpeed);
                    //cm.ArmDistance = armDistanceNew;
                } else if (armDistance > armDistanceRange.y)
                {
                    // lerp to max
                    float armDistanceNew = Mathf.Lerp(armHeight, armDistanceRange.y, TIME.DeltaPhysics*verticalRotationSpeed);
                    //cm.ArmDistance = armDistanceNew;
                }
            }
        }
    }
}