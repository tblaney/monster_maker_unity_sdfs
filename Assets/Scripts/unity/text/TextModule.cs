using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace snorri
{
    public class TextModule : Module
    {
        TextMeshProUGUI mesh;
        protected override void AddClasses()
        {
            mesh = ComponentCheck<TextMeshProUGUI>();
        }
        protected override void Setup() 
        {
            base.Setup();

            Configure();
        }

        public void Set(string text)
        {
            mesh.text = text;
        }

        void Configure()
        {
            string text = Vars.Get<string>("text", "");
            Set(text);

            string colorHex = Vars.Get<string>("color", "#FFFFFF"); // Default to white
            Color textColor;
            if (ColorUtility.TryParseHtmlString(colorHex, out textColor))
            {
                mesh.color = textColor;
            }

            mesh.enableAutoSizing = Vars.Get<bool>("is_auto_size", false);
            if (mesh.enableAutoSizing)
            {
                float minFontSize = Vars.Get<float>("font_size_min", 10f); // Default minimum font size
                float maxFontSize = Vars.Get<float>("font_size_max", 30f); // Default maximum font size
                mesh.fontSizeMin = minFontSize;
                mesh.fontSizeMax = maxFontSize;
            }
            else
            {
                mesh.fontSize = Vars.Get<float>("font_size", 24f); // Default font size when auto-sizing is disabled
            }

            string alignment = Vars.Get<string>("text_alignment", "center");
            switch (alignment.ToLower())
            {
                case "left":
                    mesh.alignment = TextAlignmentOptions.Left;
                    break;
                case "right":
                    mesh.alignment = TextAlignmentOptions.Right;
                    break;
                case "center":
                    mesh.alignment = TextAlignmentOptions.Center;
                    break;
            }

            string fontName = Vars.Get<string>("font_name", "comic_land"); // Default font name is null
            if (!string.IsNullOrEmpty(fontName))
            {
                TMP_FontAsset font = RESOURCES.Load<TMP_FontAsset>($"fonts/{fontName}");
                if (font != null)
                {
                    mesh.font = font;
                }
            }

            bool isFitToWidth = Vars.Get<bool>("is_fit_to_width", false);
            bool isFitToHeight = Vars.Get<bool>("is_fit_to_height", false);

            if (isFitToWidth || isFitToHeight)
            {
                ContentSizeFitter fitter = this.ComponentCheck<ContentSizeFitter>();

                fitter.horizontalFit = isFitToWidth ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = isFitToHeight ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
            }
        }

        public Vec Box
        {
            get
            {
                mesh.ForceMeshUpdate();

                var textSize = mesh.GetRenderedValues(false);

                float padding = 60f; // or any other value
                textSize += new Vector2(padding*(1.5f), padding);

                return new Vec(textSize.x, textSize.y);
            }
        }
        public string Text 
        {
            get { return mesh.text; }
            set { mesh.text = value; }
        }
        public Vec Color
        {
            get { return new Vec(mesh.color); }
            set { mesh.color = value.color; }
        }
    }
}