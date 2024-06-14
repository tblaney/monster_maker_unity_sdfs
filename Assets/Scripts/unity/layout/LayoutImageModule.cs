using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace snorri
{
    public class LayoutImageModule : Module
    {
        Image image;

        protected override void AddClasses()
        {
            image = ComponentCheck<Image>();
        }
        protected override void Setup()
        {
            base.Setup();

            Configure();
        }

        void Configure()
        {
            // setup texture if possible
            string textureName = Vars.Get<string>("texture", "");
            string textureType = Vars.Get<string>("texture_type", "");
            Bag<float> pivot = Vars.Get<Bag<float>>("texture_pivot", new Bag<float>(0.5f, 0.5f));
            Vec pivotVec = new Vec(pivot);

            if (textureName != "")
            {
                Texture2D tex = RESOURCES.GetTexture(textureName, textureType);
                if (tex != null)
                {
                    Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), pivotVec.vec2, 100.0f);
                    image.sprite = sprite;
                }
            }

            string colorHex = Vars.Get<string>("color", "#000000");
            Color colorHexU;
            if (ColorUtility.TryParseHtmlString(colorHex, out colorHexU))
            {
                image.color = colorHexU;
            }
        }
    }
}