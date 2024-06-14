using UnityEngine;

namespace snorri
{
    public class SphereColliderModule : ColliderModule
    {
        SphereCollider sphere;

        protected override void AddClasses()
        {
            base.AddClasses();

            sphere = ComponentCheck<SphereCollider>();
            this.collider = sphere as Collider;
        }

        protected override void Setup()
        {
            base.Setup();

            Refresh();

            /*
            string physicsMaterial = Vars.Get<string>("physic_material", "physic_material_smooth");
            if (physicsMaterial != "")
            {
                sphere.sharedMaterial = RESOURCES.Load<PhysicMaterial>("physics/" + physicsMaterial);
            }
            */
        }

        public override void Refresh(Map args = null)
        {
            base.Refresh(args);

            //LOG.Console("sphere collider module radius: " + Vars.Get<float>("radius", 1f));
            
            float radius = Vars.Get<float>("radius", 1f);

            Bag<float> center = Vars.Get<Bag<float>>("center", new Bag<float>(0f, 0f, 0f));
            
            sphere.center = new Vector3(center.x, center.y, center.z);
            sphere.radius = radius;

            bool isTrigger = Vars.Get<bool>("is_trigger", false);
            sphere.isTrigger = isTrigger;
        }
    }
}