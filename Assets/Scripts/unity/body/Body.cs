namespace snorri
{
    using UnityEngine;

    public class Body : Element, IMap
    {
        public Map Vars {get; set;}
        BodyModule module;

        // options:
        float speedTarget;
        float idleThreshold = 0.1f;
        float accelerationMovement;
        float accelerationRotation;
        float offAngleRange;

        // factors:
        public float speedFactor = 1f;

        // runtime
        public float SpeedCurrent;
        Quaternion rotationCurrent;


        public Body(BodyModule module, Map vars)
        {
            this.Vars = vars;
            this.module = module;

            ConfigureVars();
        }
        
        void ConfigureVars()
        {
            speedTarget = Vars.Get<float>("speed_target", 8f);
            idleThreshold = Vars.Get<float>("idle_threshold", 0.1f);
            accelerationMovement = Vars.Get<float>("acceleration_movement", 8f);
            accelerationRotation = Vars.Get<float>("acceleration_rotation", 8f);
            offAngleRange = Vars.Get<float>("off_angle_range", 26f);
        }

        public Vec GetWorldDirectionFromInput(Vec move, Point camera)
        {
            Vec targetDir = new Vec(move.x, 0f, move.y);
            targetDir = targetDir.Normalize();
            Vec cameraForward = camera.North;
            cameraForward.y = 0f;
            Vec cameraRight = camera.East;
            cameraRight.y = 0f;
            cameraForward = cameraForward.Multiply(move.y);
            cameraRight = cameraRight.Multiply(move.x);
            targetDir = cameraForward.Add(cameraRight);
            targetDir.Normalize();   
            return targetDir;
        }

        public void MoveFromInput(Vec input, Point camera)
        {
            Vec worldDir = GetWorldDirectionFromInput(input, camera);
            MoveTowardsDirection(worldDir);
        }

        public void MoveTowardsDirection(Vec move, Map args = null)
        {
            if (args == null)
                args = new Map();

            bool isFixedDelta = args.Get<bool>("is_fixed_delta", false);
            float speedFactor = args.Get<float>("speed_factor", 1f);
            bool isGravity = args.Get<bool>("is_gravity", true);
            float yForce = args.Get<float>("y_force", 0f);
            Vector3 vecUp = args.Get<Vector3>("vector_up", Vector3.up);

            float timeDelta = TIME.Delta;
            if (isFixedDelta)
            {
                timeDelta = TIME.DeltaPhysics;
            }

            bool isMagnitude = true;
            float speedTarget = this.speedTarget;
            if (move.Magnitude() < idleThreshold)
            {
                speedTarget = 0f;
                isMagnitude = false;
            }

            SpeedCurrent = Mathf.Lerp(SpeedCurrent, speedTarget, timeDelta*accelerationMovement);

            Vec moveFixed = new Vec(move.Normalize());
            moveFixed.y = 0f;

            Quaternion targetRotation = Quaternion.LookRotation(moveFixed.vec3, vecUp);
            Quaternion rotationNew = Quaternion.Lerp(rotationCurrent, targetRotation, timeDelta*accelerationRotation);

            Vec currentDir = module.Node.Point.North;
            currentDir.y = 0f;
            Vec targetDir = new Vec(move);
            targetDir.y = 0f;

            // rotate y
            if (isMagnitude)
            {
                rotationCurrent = rotationNew;
                module.Node.Point.Rotation = new Vec(rotationCurrent);
            }

            // rotate z
            float dot = currentDir.x*-targetDir.z + currentDir.z*targetDir.x;
            dot = -dot;
            Vec angles = module.Node.Point.Angles;
            module.Node.Point.Angles = new Vec(0f, angles.y, Mathf.LerpAngle(angles.z, dot*offAngleRange, TIME.Delta*accelerationRotation*(2*(speedFactor)))); 

            // set velocity
            Vec velocity = move.Normalize().Multiply(SpeedCurrent*speedFactor);
            if (yForce != 0f)
            {
                velocity.y = yForce;
            } else if (isGravity)
            {
                velocity.y = module.Velocity.y;
            }
            module.Velocity = velocity;
        }
        public void Rotate(Vec move, Map args)
        {
            bool isFixedDelta = args.Get<bool>("is_fixed_delta", false);
            float speedFactor = args.Get<float>("speed_factor", 1f);
            bool isGravity = args.Get<bool>("is_gravity", true);
            float yForce = args.Get<float>("y_force", 0f);

            float timeDelta = TIME.Delta;
            if (isFixedDelta)
            {
                timeDelta = TIME.DeltaPhysics;
            }

            bool isMagnitude = true;
            float speedTarget = this.speedTarget;
            if (move.Magnitude() < idleThreshold)
            {
                speedTarget = 0f;
                isMagnitude = false;
            }

            SpeedCurrent = Mathf.Lerp(SpeedCurrent, speedTarget, timeDelta*accelerationMovement);

            Vec moveFixed = new Vec(move.Normalize());
            moveFixed.y = 0f;

            Quaternion targetRotation = Quaternion.LookRotation(moveFixed.vec3, Vector3.up);
            Quaternion rotationNew = Quaternion.Lerp(rotationCurrent, targetRotation, timeDelta*accelerationRotation);

            Vec currentDir = module.Node.Point.North;
            currentDir.y = 0f;
            Vec targetDir = new Vec(move);
            targetDir.y = 0f;

            // rotate y
            if (isMagnitude)
            {
                rotationCurrent = rotationNew;
                module.Node.Point.Rotation = new Vec(rotationCurrent);
            }

            // rotate z
            float dot = currentDir.x*-targetDir.z + currentDir.z*targetDir.x;
            dot = -dot;
            Vec angles = module.Node.Point.Angles;
            module.Node.Point.Angles = new Vec(0f, angles.y, Mathf.LerpAngle(angles.z, dot*offAngleRange, TIME.Delta*accelerationRotation*(2*(speedFactor)))); 
        }

        public Vec Velocity {
            get { return module.Velocity; }
            set { module.Velocity = value; }
        }
    }
}