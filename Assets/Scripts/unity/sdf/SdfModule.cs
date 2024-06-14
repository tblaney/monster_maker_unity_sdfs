namespace snorri
{
    using UnityEngine;
    public class SdfModule : Module
    {
        MeshRenderer renderer;
        MeshFilter filter;

        public Material Material
        {
            get {
                return renderer.sharedMaterial;
            }
        }
        protected override void AddClasses()
        {
            base.AddClasses();

            renderer = this.ComponentCheck<MeshRenderer>();
            filter = this.ComponentCheck<MeshFilter>();

            renderer.sharedMaterial = RESOURCES.GetMaterial(Vars.Get<string>("material", "mat_sdf_viewer"));
        }
    }
}