using UnityEngine;
namespace snorri
{
    public class LightModule : Module
    {
        Light light;

        protected override void AddClasses()
        {
            light = this.gameObject.AddComponent<Light>();

        }

        protected override void Setup()
        {
            base.Setup();

            ConfigureVars();
        }

        void ConfigureVars()
        {
            Bag<int> rgb = Vars.Get<Bag<int>>("color", new Bag<int>(255,255,255));
            float r = rgb[0] / 255f;
            float g = rgb[1] / 255f;
            float b = rgb[2] / 255f;
            Color color = new Color(r, g, b);
            light.color = color;

            string typeName = Vars.Get<string>("type", "global");
            switch (typeName)
            {
                case "global":
                    light.type = LightType.Directional;
                    UnityEngine.RenderSettings.sun = light;
                    break;
            }

            float intensity = Vars.Get<float>("intensity", 1f);
            light.intensity = intensity;

            bool isShadows = Vars.Get<bool>("is_shadows", true);
            if (isShadows)
            {
                light.shadows = LightShadows.Hard;
            }
        }
    }
}