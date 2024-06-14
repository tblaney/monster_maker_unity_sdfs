namespace snorri
{
    using UnityEngine;
    using System.Collections.Generic;

    [System.Serializable]
    public class RigSectionIk : RigSection
    {
        Bag<Node> bones;
        Bag<RigArm> arms;

        Node effectorPole;
        NodeIk ik;

        bool isBones, isIk;

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

            UpdatePositions(true, true, true);

            effectorTarget.transform.position = effectorTargetStatic.transform.position;
            
            isBones = Vars.Get<bool>("is_bones", true);
            isIk = Vars.Get<bool>("is_ik", true);

            Bag<float> lengthsCorrected = GetArmLengths();

            bones = new Bag<Node>();
            Bag<RigArm> arms = Node.GetBagOChildActors<RigArm>(true);
            arms.Sort();
            int i = 0;
            foreach (RigArm arm in arms)
            {
                Map armMap = new Map();
                armMap.Sync(args);
                armMap.Set<float>("length", lengthsCorrected[i]);
                arm.Build(armMap);

                bones.Append(arm.Node);

                LOG.Console("rig section ik building arms: " + arm.Node.Name);

                i++;
            }

            if (Vars.Get<bool>("is_local_ik", false))
            {
                ik = new NodeIk(
                    bones,
                    lengthsCorrected,
                    effectorTargetStatic,
                    effectorPole,
                    effectorRoot,
                    args
                );
            } else
            {
                ik = new NodeIk(
                    bones,
                    lengthsCorrected,
                    effectorTarget,
                    effectorPole,
                    effectorRoot,
                    args
                );
            }

            ik.SolveIk();

            this.arms = arms;

            LOG.Console("rig section build success!");
        }
        void InitEffectors()
        {
            bool hasRoot = Node.FindChild("effector_root", out effectorRoot);
            this.effectorTarget = new Node(
                nodeName:$"target_{this.Name}", 
                "",
                new Map(),
                isLinkToTree:true
            );
            this.effectorTarget.Build();
            bool hasTargetStatic = Node.FindChild("effector_target_static", out effectorTargetStatic);
            bool hasPole = Node.FindChild("effector_pole", out effectorPole);

            if (!hasRoot || !hasPole || !hasTargetStatic)
            {
                return;
            }
        }
        Bag<float> GetArmLengths()
        {
            Bag<float> lengths = Vars.Get<Bag<float>>($"lengths_{Type}", new Bag<float>(1f,1f,1f));
            Vector3 targetPos = Vars.Get<Vector3>("effector_target_local_position");
            Vector3 rootPos = Vars.Get<Vector3>("effector_root_position");
            float distance = Vector3.Distance(rootPos, targetPos);
           
            Bag<float> lengthsOut = new Bag<float>();

            foreach (float f in lengths)
            {
                lengthsOut.Append(distance*f, false);
            }

            return lengthsOut;
        }
        void UpdatePositions(bool isUpdateRoot = false, bool isUpdateTarget = false, bool isUpdatePole = false)
        {
            Vec scale = Vars.Get<Bag<float>>("scale", new Bag<float>(1f,1f,1f)).ToVec();
            Bag<int> direction = Vars.Get<Bag<int>>("direction", new Bag<int>(1,1,1));

            Vec origin = Vars.Get<Bag<float>>("origin", new Bag<float>(0f,0f,0f)).ToVec();
            Vec target = Vars.Get<Bag<float>>("target", new Bag<float>(0f,0f,0f)).ToVec();
            Vec poleOffset = Vars.Get<Bag<float>>("pole_offset", new Bag<float>(0f,0f,0f)).ToVec();

            Vector3 originCorrected = new Vector3(origin.vec3.x*scale.x*direction[0],
                origin.vec3.y*scale.y*direction[1], origin.vec3.z*scale.z*direction[2]);
            Vector3 targetCorrected = new Vector3(target.vec3.x*scale.x*direction[0],
                target.vec3.y*scale.y*direction[1], target.vec3.z*scale.z*direction[2]);
            Vector3 poleOffsetCorrected = new Vector3(
                poleOffset.x*direction[0], poleOffset.y*direction[1], poleOffset.z*direction[2]
            );

            float growthLerp = Vars.Get<float>("growth_factor", 1f);
            
            originCorrected = Vector3.Lerp(originCorrected, Vars.Get<Vector3>("section_origin", new Vector3(0f,0f,0f)), 1f - growthLerp);
            targetCorrected = Vector3.Lerp(targetCorrected, Vars.Get<Vector3>("section_origin", new Vector3(0f,0f,0f)), 1f - growthLerp);
            poleOffsetCorrected = Vector3.Lerp(poleOffsetCorrected, Vars.Get<Vector3>("section_origin", new Vector3(0f,0f,0f)), 1f - growthLerp);

            if (isUpdateRoot)
            {
                effectorRoot.transform.localPosition = originCorrected;
            }
            if (isUpdateTarget)
            {
                effectorTargetStatic.transform.localPosition = targetCorrected;
            }
            if (isUpdatePole)
            {
                effectorPole.transform.localPosition = originCorrected + poleOffsetCorrected;
            }

            Vars.Set<Vector3>("effector_target_local_position", targetCorrected);
            Vars.Set<Vector3>("effector_root_position", originCorrected);
        }
        void UpdateIk(Bag<float> lengths)
        {
            ik.UpdateLengths(lengths);
        }
        void UpdateArms(Bag<float> lengths, Map args)
        {
            int i = 0;
            foreach (RigArm arm in arms)
            {
                Map armMap = new Map();
                
                armMap.Sync(args);
                armMap.Set<float>("length", lengths[i]);

                arm.Refresh(armMap);

                i++;
            }
        }

        public override void Refresh(Map args = null)
        {
            base.Refresh(args);

            if (ik == null)
                return;

            if (arms == null)
                return;

            if (args == null)
                args = new Map();

            this.Vars.Sync(args);

            UpdatePositions(Node.Vars.Get<bool>("is_update_root", true), Node.Vars.Get<bool>("is_update_target", true), false);

            Bag<float> lengths = GetArmLengths();
            if (isIk)
            {
                this.UpdateIk(lengths);
                ik.SolveIk();
            }
            if (isBones)
            {
                UpdateArms(lengths, args);
            }
        }
    }
}