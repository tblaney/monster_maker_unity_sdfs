using UnityEngine;

namespace snorri
{
    public class BoxColliderModule : ColliderModule
    {
        BoxCollider box;

        protected override void AddClasses()
        {
            base.AddClasses();

            box = ComponentCheck<BoxCollider>();
            this.collider = box as Collider;
        }

        protected override void Setup()
        {
            base.Setup();

            Refresh();

            /*
            string physicsMaterial = Vars.Get<string>("physic_material", "physic_material_smooth");
            if (physicsMaterial != "")
            {
                box.sharedMaterial = RESOURCES.Load<PhysicMaterial>("physics/" + physicsMaterial);
            }
            */
        }

        public override void Refresh(Map args = null)
        {
            base.Refresh(args);
            
            Vec size = Vars.Get<Bag<float>>("size", new Bag<float>(1f,1f,1f)).ToVec();
            Vec center = Vars.Get<Bag<float>>("center", new Bag<float>(0f,0f,0f)).ToVec();
            
            box.center = center.vec3;
            box.size = size.vec3;

            bool isTrigger = Vars.Get<bool>("is_trigger", false);
            box.isTrigger = isTrigger;
        }
    }
}