using UnityEngine;

namespace snorri
{
    public class CapsuleColliderModule : ColliderModule
    {
        CapsuleCollider capsule;

        protected override void AddClasses()
        {
            base.AddClasses();

            capsule = ComponentCheck<CapsuleCollider>();
            this.collider = capsule as Collider;
        }

        protected override void Setup()
        {
            base.Setup();

            Refresh();

            /*
            string physicsMaterial = Vars.Get<string>("physic_material", "physic_material_smooth");
            if (physicsMaterial != "")
            {
                capsule.sharedMaterial = RESOURCES.Load<PhysicMaterial>("physics/" + physicsMaterial);
            }
            */
        }

        public override void Refresh(Map args = null)
        {
            base.Refresh(args);
            
            float height = Vars.Get<float>("height", 2f);
            float radius = Vars.Get<float>("radius", 1f);

            Bag<float> center = Vars.Get<Bag<float>>("center", new Bag<float>(radius/2, height/2, radius/2));
            
            capsule.center = new Vector3(center.x, center.y, center.z);
            capsule.height = height;
            capsule.radius = radius;

            bool isTrigger = Vars.Get<bool>("is_trigger", false);
            capsule.isTrigger = isTrigger;

            int direction = Vars.Get<int>("direction", 1);
            capsule.direction = direction;
        }
        
    }
}