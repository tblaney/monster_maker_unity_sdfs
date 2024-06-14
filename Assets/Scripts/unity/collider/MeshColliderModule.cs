using UnityEngine;

namespace snorri
{
    public class MeshColliderModule : ColliderModule
    {
        MeshCollider mesh;

        protected override void AddClasses()
        {
            base.AddClasses();

            mesh = ComponentCheck<MeshCollider>();
            this.collider = mesh as Collider;
        }

        protected override void Setup()
        {
            base.Setup();

            bool isTrigger = Vars.Get<bool>("is_trigger", false);
            mesh.isTrigger = isTrigger;

            bool isConvex = Vars.Get<bool>("is_convex", false);
            mesh.convex = isConvex;
        }
    }
}