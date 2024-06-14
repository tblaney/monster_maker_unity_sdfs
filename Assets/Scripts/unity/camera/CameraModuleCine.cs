using UnityEngine;
using Cinemachine;

namespace snorri
{
    public class CameraModuleCine : Module
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

        CinemachineVirtualCamera vCamera;

        public float ArmDistance
        {
            get 
            { 
                var thirdPersonFollow = vCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
                return thirdPersonFollow.CameraDistance;
            }
            set 
            { 
                var thirdPersonFollow = vCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
                thirdPersonFollow.CameraDistance = value;
            }
        }
        public float ArmHeight
        {
            get 
            { 
                var thirdPersonFollow = vCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
                return thirdPersonFollow.VerticalArmLength;
            }
            set 
            { 
                var thirdPersonFollow = vCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
                thirdPersonFollow.VerticalArmLength = value;
            }
        }

        protected override void AddClasses()
        {
            LOG.Console("camera module cine add classes");
            vCamera = ComponentCheck<CinemachineVirtualCamera>();
        }

        protected override void Setup()
        {
            base.Setup();

            LOG.Console("camera module cine setup");

            Configure();
        }

        protected override void Launch()
        {
            base.Launch();

            LOG.Console("camera module cine launch");

            string target = Vars.Get<string>("target", "stage_overworld.player");
            Node n = NODE.Tree.Get<Node>(target, null);
            if (n == null)
                return;

            Point p = n.Point;

            vCamera.LookAt = p.transform;

            bool isFollow = Vars.Get<bool>("is_follow", false);
            if (isFollow)
                vCamera.Follow = p.transform;
        }

        void Configure()
        {
            string cameraMode = Vars.Get<string>("mode", "perspective");

            bool isOrtho = cameraMode.Equals("orthographic", System.StringComparison.OrdinalIgnoreCase);

            if (!isOrtho)
            {
                vCamera.m_Lens.FieldOfView = Vars.Get<float>("fov", 60f); // Default FOV is 60
            } else
            {
                vCamera.m_Lens.OrthographicSize = Vars.Get<float>("orthographic_size", 5f); // Default orthographic size
            }
        }
    }
}