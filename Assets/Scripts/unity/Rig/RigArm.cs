namespace snorri
{
    using UnityEngine;
    using System.Collections.Generic;

    [System.Serializable]
    public class RigArm : Actor, IId
    {
        Node bone;
        float length;

        public int Id {get; set;}

        public Vector3 TargetPosition {
            get {
                return bone.transform.position + bone.transform.up*length;
            }
        }

        SdfSurface boneSurface;
        SdfSurface jointSurface;

        public string Type;

        protected override void Setup()
        {
            base.Setup();

            Type = Vars.Get<string>("type", "base");
            Id = Vars.Get<int>("id", 0);
            if (Id == 0)
            {
                if (Node.Name.Contains("arm_end"))
                {
                    Id = 1;
                } else if (Node.Name.Contains("arm_mid"))
                {
                    Id = 2;
                } else if (Node.Name.Contains("arm_root"))
                {
                    Id = 3;
                }
            }
        }

        bool isBones;

        public void Build(Map args = null)
        {
            this.Vars.Sync(args);

            bool isBones = Vars.Get<bool>("is_bones", true);
            this.isBones = isBones;
            bool hasBone = Node.FindChild("rig_arm_bone", out bone);
            if (!hasBone && isBones)
            {
                LOG.Console("rig arm found no bone!");
                return;
            }
            if (isBones)
            {
                boneSurface = bone.GetActor<SdfSurface>();
                if (boneSurface == null)
                    return;

                Vars.Set<float>("bone_corners", boneSurface.Radius);
                Vars.Set<float>("bone_blend_strength", boneSurface.BlendStrength);
                Vars.Set<Vector3>("bone_scale", boneSurface.Scale);

                UpdateBones();
            }

            jointSurface = Node.GetActor<SdfSurface>();
            if (jointSurface == null)
                return;

            Vars.Set<float>("joint_corners", jointSurface.Radius);
            Vars.Set<float>("joint_blend_strength", jointSurface.BlendStrength);
            Vars.Set<Vector3>("joint_scale", jointSurface.Scale);

            UpdateJoints();
        }
        void UpdateBones()
        {
            boneSurface = bone.GetActor<SdfSurface>();
            if (boneSurface == null)
                return;

            float growthLerp = Vars.Get<float>("growth_factor", 1f);
            Vec boneScale = Vars.Get<Bag<float>>("scale_bones", new Bag<float>(1f,1f,1f)).ToVec().Multiply(growthLerp);

            Vector3 boneScaleCurrent = Vars.Get<Vector3>("bone_scale");
            boneSurface.Scale = new Vector3(boneScaleCurrent.x*boneScale.x, Vars.Get<float>("length", boneScaleCurrent.y)/2f, boneScaleCurrent.z*boneScale.z);
            this.length = Vars.Get<float>("length", boneScaleCurrent.y);
            boneSurface.Node.transform.localPosition = new Vector3(
                0f, -this.length/2f, 0f
            );

            boneSurface.Radius = Vars.Get<float>("bone_corners")*boneScale.x;
            boneSurface.BlendStrength = Vars.Get<float>("bone_blend_strength")*Vars.Get<Bag<float>>("scale", new Bag<float>(1f,1f,1f)).Max() * growthLerp;
        
            boneSurface.RefreshCollider();
            boneSurface.GrowthFactor = (growthLerp);
        }
        void UpdateJoints()
        {
            jointSurface = Node.GetActor<SdfSurface>();
            if (jointSurface == null)
                return;

            float growthLerp = Vars.Get<float>("growth_factor", 1f);
            Vec boneScale = Vars.Get<Bag<float>>("scale_bones", new Bag<float>(1f,1f,1f)).ToVec().Multiply(growthLerp);

            Vector3 scaleCurrent = Vars.Get<Vector3>("joint_scale");
            jointSurface.Scale = new Vector3(scaleCurrent.x*boneScale.x, scaleCurrent.y*boneScale.y, scaleCurrent.z*boneScale.z);
            jointSurface.Radius = Vars.Get<float>("joint_corners")*boneScale.x;
            jointSurface.BlendStrength = Vars.Get<float>("joint_blend_strength")*Vars.Get<Bag<float>>("scale", new Bag<float>(1f,1f,1f)).Max()*growthLerp;
        
            jointSurface.RefreshCollider();
            jointSurface.GrowthFactor = (growthLerp);
        }
        public void Refresh(Map args = null)
        {
            if (args != null)
                this.Vars.Sync(args);

            if (isBones)
                UpdateBones();

            UpdateJoints();

            if (isBones)
                UpdateOrientation();
        }
        void UpdateOrientation()
        {
            //Vector3 direction = (TargetPosition - bone.transform.position).normalized;
            Vector3 direction = bone.transform.up.normalized;

            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(-90, 0, 0);
            if (bone.transform.position.y  < TargetPosition.y)
            {
                rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(90, 0, 0);
            }
            // Apply the rotation
            bone.transform.rotation = rotation;
        }
    }
}