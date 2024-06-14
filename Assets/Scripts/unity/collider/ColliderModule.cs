using UnityEngine;

namespace snorri
{
    public abstract class ColliderModule : Module
    {
        protected Collider collider;

        protected override void AddClasses()
        {
            base.AddClasses();
        }
        protected override void Setup()
        {
            base.Setup();

            string pmName = Vars.Get<string>("physic_material", "physic_material_smooth");
            PhysicMaterial mat = RESOURCES.Load<PhysicMaterial>("physics/" + pmName);
            LOG.Console($"collider module has physic mat: {mat.name}");
            this.collider.sharedMaterial = mat;
        }
        public virtual void Refresh(Map args = null)
        {
            if (args != null)
                Vars.Sync(args);
        }
    }
}