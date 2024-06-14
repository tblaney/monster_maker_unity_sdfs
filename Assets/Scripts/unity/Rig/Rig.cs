namespace snorri
{
    using UnityEngine;
    using System.Collections.Generic;
    using System;

    public class Rig : Module
    {
        [SerializeField]
        private Vector3 scale = new Vector3(1f,1f,1f);
        [SerializeField]
        private Vector3 scaleBones = new Vector3(1f,1f,1f);
        [SerializeField]
        private float growthFactor = 1f;
        [SerializeField]
        private Bag<float> legLengths = new Bag<float>(0.0f, 0.6f, 0.5f);
        [SerializeField]
        private Bag<float> armLengths = new Bag<float>(0.0f, 0.5f, 0.5f);
        [SerializeField]
        private float acceleration = 2f;

        Vector3 targetScale;
        Vector3 targetScaleBones;
        public Vector3 legLengthsCache = new Vector3(1f,1f,1f);

        Node childNode;
        protected override void Setup()
        {
            base.Setup();

            targetScale = UTIL.GetRandomVector(new Vector3(0.3f, 0.3f, 0.3f), new Vector3(1.6f, 1.6f, 1.6f));
            targetScaleBones = UTIL.GetRandomVector(new Vector3(0.5f,0.5f,0.5f), new Vector3(1.0f, 1.0f, 1.0f));
        }
        protected override void Launch()
        {
            base.Launch();

            Bag<RigSection> bagOfSections = Node.GetBagOChildActors<RigSection>();
            foreach (RigSection section in bagOfSections)
            {
                section.Build(Vars.Get<Map>("args", new Map()));
            }
        }
        void FixedUpdate()
        {
            VarsUpdate();

            Bag<RigSection> bagOfSections = Node.GetBagOChildActors<RigSection>();
            foreach (RigSection section in bagOfSections)
            {
                section.Refresh(Vars.Get<Map>("args", new Map()));
            }
        }
        void VarsUpdate()
        {
            Vars.Set<Bag<float>>("args:scale", new Bag<float>(scale.x, scale.y, scale.z));
            Vars.Set<Bag<float>>("args:scale_bones", new Bag<float>(scaleBones.x, scaleBones.y, scaleBones.z));
            Vars.Set<Bag<float>>("args:lengths_arms", armLengths);
            Vars.Set<Bag<float>>("args:lengths_legs", legLengths);
            Vars.Set<float>("args:growth_factor", growthFactor);

            if (Vars.Get<bool>("is_random", true))
            {
                RandomScaleUpdate();
                RandomScaleBonesUpdate();
                RandomLegUpdate();
            }
        }

        void RandomScaleUpdate()
        {
            float distance = Vector3.Distance(scale, targetScale);
            if (distance > 0.05f)
            {
                scale = Vector3.Lerp(scale, targetScale, TIME.Delta*acceleration);
            } else
            {
                targetScale = UTIL.GetRandomVector(new Vector3(0.3f, 0.3f, 0.3f), new Vector3(1.6f, 1.6f, 1.6f));
            }
        }
        void RandomScaleBonesUpdate()
        {
            float distance = Vector3.Distance(scaleBones, targetScaleBones);
            if (distance > 0.05f)
            {
                scaleBones = Vector3.Lerp(scaleBones, targetScaleBones, TIME.Delta*acceleration);
            } else
            {
                targetScaleBones = UTIL.GetRandomVector(new Vector3(0.5f,0.5f,0.5f), new Vector3(1.0f, 1.0f, 1.0f));
            }
        }
        void RandomLegUpdate()
        {
            float distanceY = Mathf.Abs(legLengths[1] - legLengthsCache.y);
            if (distanceY < 0.05)
            {
                legLengthsCache.y = UnityEngine.Random.Range(0.3f, 1.0f);
            }
            float distanceZ = Mathf.Abs(legLengths[2] - legLengthsCache.z);
            if (distanceZ < 0.05)
            {
                legLengthsCache.z = UnityEngine.Random.Range(0.3f, 1.0f);
            }

            legLengths[1] = UnityEngine.Mathf.Lerp(legLengths[1], legLengthsCache.y, TIME.Delta*acceleration);
            legLengths[2] = UnityEngine.Mathf.Lerp(legLengths[2], legLengthsCache.z, TIME.Delta*acceleration);
        }
    }
}