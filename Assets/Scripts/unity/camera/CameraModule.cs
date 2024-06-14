using UnityEngine;

namespace snorri
{
    public class CameraModule : Module
    {
        /*
            VARS:
                --> mode: perspective, orthographic
                --> fov
                --> orthographic_size
                --> near_clip
                --> far_clip
                --> bg_color
        */

        Camera camera;
        public Camera Cam {get { return camera; }}

        protected override void AddClasses()
        {
            camera = ComponentCheck<Camera>();
        }

        protected override void Setup()
        {
            base.Setup();

            Configure();
        }

        void Configure()
        {
            string cameraMode = Vars.Get<string>("mode", "perspective");
            camera.orthographic = cameraMode.Equals("orthographic", System.StringComparison.OrdinalIgnoreCase);

            if (!camera.orthographic)
            {
                camera.fieldOfView = Vars.Get<float>("fov", 60f); // Default FOV is 60
            } else
            {
                camera.orthographicSize = Vars.Get<float>("orthographic_size", 5f); // Default orthographic size
            }

            camera.nearClipPlane = Vars.Get<float>("near_clip", 0.3f); // Default near clip
            camera.farClipPlane = Vars.Get<float>("far_clip", 1000f); // Default far clip

            string bgColorHex = Vars.Get<string>("bg_color", "#000000"); // Default to black
            Color bgColor;
            if (ColorUtility.TryParseHtmlString(bgColorHex, out bgColor))
            {
                camera.backgroundColor = bgColor;
            }

            bool isMain = Vars.Get<bool>("is_main", false);
            if (isMain)
                GAME.Vars.Set<CameraModule>("camera_main", this);
        }
    }
}