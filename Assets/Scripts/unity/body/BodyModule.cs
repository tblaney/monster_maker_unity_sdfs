using UnityEngine;

namespace snorri
{
    public class BodyModule : Module
    {
        Rigidbody rb;

        TriggerListener listener;

        protected override void AddClasses()
        {
            base.AddClasses();

            rb = ComponentCheck<Rigidbody>();
        }

        protected override void Setup()
        {
            base.Setup();

            listener = new TriggerListener();
            listener.Listen(Trigger.WhenContextChange, new Task(WhenContextChange));

            rb.mass = Vars.Get<float>("mass", 1.0f);
            rb.drag = Vars.Get<float>("drag", 0.0f);
            rb.angularDrag = Vars.Get<float>("angular_drag", 0.05f);
            rb.useGravity = Vars.Get<bool>("is_gravity", true);

            rb.isKinematic = Vars.Get<bool>("is_kinematic", false);

            switch (Vars.Get<string>("constraints", "all"))
            {
                case "none":
                    rb.constraints = RigidbodyConstraints.None;
                    break;
                case "all":
                    rb.constraints = RigidbodyConstraints.FreezeAll;
                    break;
                case "position":
                    rb.constraints = RigidbodyConstraints.FreezePosition;
                    break;
                case "rotation":
                    rb.constraints = RigidbodyConstraints.FreezeRotation;
                    break;
            }

            switch (Vars.Get<string>("interpolate", "none"))
            {
                case "none":
                    rb.interpolation = RigidbodyInterpolation.None;
                    break;
                case "interpolate":
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                    break;
                case "extrapolate":
                    rb.interpolation = RigidbodyInterpolation.Extrapolate;
                    break;
            }

            switch (Vars.Get<string>("collision", "continuous"))
            {
                case "continuous":
                    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    break;
                case "discrete":
                    rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    break;
            }
        }

        void WhenContextChange()
        {
            // calbback for context change

            if (GAME.Context == "run")
            {
                bool isKinematic = Vars.Get<bool>("is_kinematic", false);
                if (!isKinematic)
                {
                    rb.isKinematic = false;
                }

                // rb.ResetInertiaTensor();
            } else 
            {
                bool isKinematic = Vars.Get<bool>("is_kinematic", false);
                if (!isKinematic)
                {
                    rb.isKinematic = true;
                }
            }
        }

        public void Halt()
        {
            Velocity = Vec.zero;
        }

        public Vec Velocity {
            get {
                return new Vec(rb.velocity);
            }
            set {
                rb.velocity = value.vec3;
            }
        }
        public bool Kinematic {
            get {
                return rb.isKinematic;
            }
            set {
                rb.isKinematic = value;
            }
        }
    }
}