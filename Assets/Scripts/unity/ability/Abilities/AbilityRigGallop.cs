namespace snorri
{
    using UnityEngine;
    public class AbilityRigGallop : AbilityRig
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
        string walkCheckName = "is_walk";

        int walkCheckIndex = -1;

        Vector3 targetPosition;

        protected override void Setup()
        {
            base.Setup();

            threshold = Vars.Get<float>("threshold", 1.0f);
            time = Vars.Get<float>("time", 1.0f);
            height = Vars.Get<float>("height", 0.5f);
            isRoot = Vars.Get<bool>("is_root", false);

            walkCheckName = Vars.Get<string>("walk_check_name", "is_walk");

            parentNode = NODE.Tree.Get<Node>(Node.Parent);
        }
        protected override void Launch()
        {
            base.Launch();

            localRootOffset = section.Root.transform.localPosition;

            Bag<bool> walkCheckVals = parentNode.Vars.Get<Bag<bool>>(walkCheckName, new Bag<bool>());
            walkCheckVals.Append(false, false);
            walkCheckIndex = walkCheckVals.Length - 1;
        }
        bool WalkCheck()
        {
            Bag<bool> walkCheckVals = parentNode.Vars.Get<Bag<bool>>(walkCheckName, new Bag<bool>());
            foreach (bool b in walkCheckVals)
            {
                if (!b)
                    return false;
            }
            return true;
        }
        public override bool CanRun()
        {
            //LOG.Console("ability rig check");

            if (section.Target == null || section.TargetLocal == null)
                return false;

            //if (GAME.Vars.Get<Vec>("input:direction", new Vec()).Magnitude() < 0.3f)
            //    return false;
                
            Bag<bool> walkCheckVals = parentNode.Vars.Get<Bag<bool>>(walkCheckName, new Bag<bool>());

            LOG.Console($"ability rig gallop nodes ready: {walkCheckVals.Length}");

            RefreshTargetPosition();
            float distance = Vector3.Distance(section.Target.transform.position, targetPosition);

            float thresholdFactor = threshold * section.Vars.Get<Bag<float>>("scale", new Bag<float>(1f,1f,1f)).Max();
            if (distance > thresholdFactor)
            {
                if (WalkCheck())
                    return true;
                
                walkCheckVals[this.walkCheckIndex] = true;
                parentNode.Vars.Set<Bag<bool>>(walkCheckName, walkCheckVals);

                return false;
            }

            walkCheckVals[this.walkCheckIndex] = false;
            parentNode.Vars.Set<Bag<bool>>(walkCheckName, walkCheckVals);

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
        }
        public override void End()
        {
            base.End();

            Bag<bool> walkCheckVals = parentNode.Vars.Get<Bag<bool>>(walkCheckName, new Bag<bool>());
            walkCheckVals[walkCheckIndex] = false;
            parentNode.Vars.Set<Bag<bool>>(walkCheckName, walkCheckVals);
        }
        void RefreshTargetPosition()
        {
            targetPosition = section.TargetLocal.transform.position;
        }
        public override void TickPhysicsAbility()
        {
            base.TickPhysicsAbility();

            if (section.Target == null) 
            {
                Stop();
                return;
            }

            localRootOffset = section.Vars.Get<Vector3>("effector_root_position");
            section.Root.transform.localPosition = section.Vars.Get<Vector3>("effector_root_position");
            section.TargetLocal.transform.localPosition = section.Vars.Get<Vector3>("effector_target_local_position");
            //section.TargetLocal.transform.position = targetPosition;

            RefreshTargetPosition();

            float distance = Vector3.Distance(section.Target.transform.position, targetPosition);

            timer += TIME.DeltaPhysics / time;
            float y = UTIL.CalculateSineY(timer, 0f, 1f, height*section.Vars.Get<Bag<float>>("scale", new Bag<float>(1f,1f,1f)).Min());

            // update pos
            section.Target.transform.position = Vector3.Lerp(
                section.Target.transform.position,
                targetPosition + new Vector3(0,y,0),
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