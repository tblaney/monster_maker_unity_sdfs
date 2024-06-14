namespace snorri
{
    using UnityEngine;
    public class AbilityRigWalk : AbilityRig
    {
        float threshold = 1f;
        float time = 1f;
        float height = 0.5f;

        float timer;

        Vector3 cache;

        float x;
        float x1;
        float x2;

        Node parentNode;
        Vector3 localRootOffset;

        bool isRoot = false;
        bool isWalkCheck = true;
        string walkCheckName = "is_walk";

        protected override void Setup()
        {
            base.Setup();

            threshold = Vars.Get<float>("threshold", 1.0f);
            time = Vars.Get<float>("time", 1.0f);
            height = Vars.Get<float>("height", 0.5f);
            isRoot = Vars.Get<bool>("is_root", false);

            isWalkCheck = Vars.Get<bool>("is_walk_check", true);
            walkCheckName = Vars.Get<string>("walk_check_name", "is_walk");

            parentNode = NODE.Tree.Get<Node>(Node.Parent);
        }
        protected override void Launch()
        {
            base.Launch();

            localRootOffset = section.Root.transform.localPosition;
        }
        public override bool CanRun()
        {
            //LOG.Console("ability rig check");

            if (section.Target == null || section.TargetLocal == null)
                return false;
                
            bool isWalk = parentNode.Vars.Get<bool>(walkCheckName, true);
            if (!isWalk && isWalkCheck)
                return false;

            if (Vars.Get<bool>("is_input_cap", false))
            {
                if (GAME.Vars.Get<Vec>("input:direction", new Vec()).Magnitude() >= Vars.Get<float>("input_cap"))
                    return false;
            }
                
            float distance = Vector3.Distance(section.Target.transform.position,  section.TargetLocal.transform.position);
            //LOG.Console($"ability rig move distance: {distance}");
            
            float thresholdFactor = threshold * section.Vars.Get<Bag<float>>("scale", new Bag<float>(1f,1f,1f)).Min();
            if (distance > thresholdFactor)
            {
                return true;
            }

            return false;
        }
        public override void Begin()
        {
            base.Begin();

            timer = 0f;
            
            //section.Root.transform.localPosition = localRootOffset;
            section.Root.transform.localPosition = section.Vars.Get<Vector3>("effector_root_position");
            section.TargetLocal.transform.localPosition = section.Vars.Get<Vector3>("effector_target_local_position");
            localRootOffset = section.Root.transform.localPosition;

            if (isWalkCheck)
                parentNode.Vars.Set<bool>(walkCheckName, false);
        }
        public override void End()
        {
            base.End();

            if (isWalkCheck)
                parentNode.Vars.Set<bool>(walkCheckName, true);
        }
        public override void TickPhysicsAbility()
        {
            base.TickPhysicsAbility();

            if (section.Target == null) 
            {
                Stop();
                return;
            }

            //section.TargetLocal.transform.localPosition = section.Vars.Get<Vector3>("effector_target_local_position");
            //localRootOffset = section.Vars.Get<Vector3>("effector_root_position");
            //localRootOffset = section.Vars.Get<Vector3>("effector_root_position");
            //section.Root.transform.localPosition = section.Vars.Get<Vector3>("effector_root_position");
            //section.TargetLocal.transform.localPosition = section.Vars.Get<Vector3>("effector_target_local_position");


            float distance = Vector3.Distance(section.Target.transform.position, section.TargetLocal.transform.position);

            timer += TIME.DeltaPhysics / time;
            float y = UTIL.CalculateSineY(timer, 0f, 1f, height*section.Vars.Get<Bag<float>>("scale", new Bag<float>(1f,1f,1f)).Min());

            // update pos
            section.Target.transform.position = Vector3.Lerp(
                section.Target.transform.position,
                section.TargetLocal.transform.position + new Vector3(0,y,0),
                timer
            );
            
            if (isRoot)
            {
                section.Root.transform.localPosition = Vector3.Lerp(
                    section.Root.transform.localPosition,
                    localRootOffset + new Vector3(0,y/2,0),
                    timer
                );
            }

            if (distance < 0.05f)
            {
                Stop();
            } else if (timer > 1f)
            {
                Stop();
            }
        }
    }
    
}