namespace snorri
{
    using UnityEngine;
    using System.Collections.Generic;

    [System.Serializable]
    public class RigSectionSingle : RigSection
    {
        protected override void Launch()
        {
            base.Launch();
        }

        public override void Build(Map args = null)
        {
            if (args == null)
                args = new Map();

            Vars.Sync(args);

            InitEffectors();

            UpdatePositions();

            Bag<RigArm> arms = Node.GetBagOChildActors<RigArm>(true);
            foreach (RigArm arm in arms)
            {
                Map armMap = new Map();
                armMap.Sync(args);
                arm.Build(armMap);

                arm.Node.transform.position = effectorRoot.transform.position;
                //arm.Node.transform.up = (effectorTargetStatic.transform.position - effectorRoot.transform.position).normalized;
            }
        }
        void InitEffectors()
        {
            bool hasRoot = Node.FindChild("effector_root", out effectorRoot);
            bool hasTargetStatic = Node.FindChild("effector_target_static", out effectorTargetStatic);
            this.effectorTarget = new Node(
                nodeName:$"target_{this.Name}", 
                "",
                new Map(),
                isLinkToTree:true
            );
            this.effectorTarget.Build();
            if (!hasRoot)
            {
                LOG.Console("rig section build failed!");
                return;
            }
        }
        void UpdatePositions()
        {
            Vec scale = Vars.Get<Bag<float>>("scale", new Bag<float>(1f,1f,1f)).ToVec();
            Bag<int> direction = Vars.Get<Bag<int>>("direction", new Bag<int>(1,1,1));

            Vec origin = Vars.Get<Bag<float>>("origin", new Bag<float>(0f,0f,0f)).ToVec();
            Vec target = Vars.Get<Bag<float>>("target", new Bag<float>(0f,0f,0f)).ToVec();

            Vector3 originCorrected = new Vector3(origin.vec3.x*scale.x*direction[0],
                origin.vec3.y*scale.y*direction[1], origin.vec3.z*scale.z*direction[2]);
            Vector3 targetCorrected = new Vector3(target.vec3.x*scale.x*direction[0],
                target.vec3.y*scale.y*direction[1], target.vec3.z*scale.z*direction[2]);

            effectorRoot.transform.localPosition = originCorrected;
            effectorTargetStatic.transform.localPosition = targetCorrected;
            effectorTarget.transform.position = effectorTargetStatic.transform.position;

            Vars.Set<Vector3>("effector_target_local_position", targetCorrected);
            Vars.Set<Vector3>("effector_root_position", originCorrected);
        }
        void UpdateArms()
        {
            Bag<RigArm> arms = Node.GetBagOChildActors<RigArm>(true);
            foreach (RigArm arm in arms)
            {
                Map armMap = new Map();

                armMap.Sync(Vars);

                arm.Refresh(armMap);

                arm.Node.transform.position = effectorRoot.transform.position;
                //arm.Node.transform.up = (effectorTargetStatic.transform.position - effectorRoot.transform.position).normalized;
            }
        }

        public override void Refresh(Map args = null)
        {
            base.Refresh(args);

            if (args != null)
                Vars.Sync(args);

            UpdatePositions();

            UpdateArms();
        }
    }
}