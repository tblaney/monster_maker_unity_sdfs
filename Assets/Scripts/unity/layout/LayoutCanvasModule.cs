using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace snorri
{
    public class LayoutCanvasModule : Module
    {
        Canvas canvas;
        CanvasScaler canvasScaler;
        GraphicRaycaster graphicRaycaster;

        protected override void AddClasses()
        {
            canvas = ComponentCheck<Canvas>();
            canvasScaler = ComponentCheck<CanvasScaler>();
            graphicRaycaster = ComponentCheck<GraphicRaycaster>();
        }
        protected override void Setup()
        {
            base.Setup();

            Configure();
        }

        void Configure()
        {
            ConfigureCanvas();
            ConfigureScaler();
        }

        void ConfigureCanvas()
        {
            string renderMode = Vars.Get<string>("render_mode", "overlay");
            int sortOrder = Vars.Get<int>("sort_order", 0);

            ConfigureRenderMode(renderMode);
            canvas.sortingOrder = sortOrder;
        }
        void ConfigureRenderMode(string renderMode)
        {
            switch (renderMode)
            {
                case "overlay":
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    break;
                case "world":
                    canvas.renderMode = RenderMode.WorldSpace;
                    break;
                case "screen":
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    break;
            }
            
        }
        void ConfigureScaler()
        {
            string scalerMode = Vars.Get<string>("scale_mode", "screen");
            switch (scalerMode)
            {
                case "pixel":
                    canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                    break;
                case "screen":
                    canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    
                    canvasScaler.referenceResolution = new Vector2(1920, 1080); // Example resolution
                    canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    canvasScaler.matchWidthOrHeight = 0.5f; // 0 is width, 1 is height, 0.5 is an equal balance
                    break;
                case "constant":
                    canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPhysicalSize;
                    break;
            }
        }
    }
}