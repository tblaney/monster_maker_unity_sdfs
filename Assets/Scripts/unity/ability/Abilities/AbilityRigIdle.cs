namespace snorri
{
    using UnityEngine;

    public class AbilityRigIdle : AbilityRig
    {
        float time = 1f;
        float height = 0.5f;
        float timer;
        string axes;
        bool isRoot = false;
        bool isTarget = false;
        int dir = 1;

        Vector3 localTargetOffset;
        Vector3 localRootOffset;

        bool isMovementEffected = true;
        bool isRootFollow = false;
        string rootFollowName;

        protected override void Setup()
        {
            base.Setup();

            time = Vars.Get<float>("time", 1.0f);
            height = Vars.Get<float>("height", 0.5f);
            axes = Vars.Get<string>("axes", "y");
            isRoot = Vars.Get<bool>("is_root", false);
            isTarget = Vars.Get<bool>("is_target", true);
            isRootFollow = Vars.Get<bool>("is_root_follow", false);
            rootFollowName = Vars.Get<string>("root_follow_name", "");
            dir = Vars.Get<int>("direction", 1);

            if (isTarget)
            {
                Node.Vars.Set<bool>("is_update_target", false);
            }
            if (isRoot)
            {
                Node.Vars.Set<bool>("is_update_root", false);
            }

            isMovementEffected = Vars.Get<bool>("is_movement_effected", true);
        }
        protected override void Launch()
        {
            base.Launch();

            localTargetOffset = section.TargetLocal.transform.localPosition;
            //localTargetOffset = section.Vars.Get<Vector3>("effector_target_local_position", section.TargetLocal.transform.localPosition);
            localRootOffset = section.Root.transform.localPosition;
            //localRootOffset = section.Vars.Get<Vector3>("effector_root_position", section.Root.transform.localPosition);

            this.height = this.height * section.Vars.Get<Bag<float>>("scale", new Bag<float>(1f,1f,1f)).Min();
        }
        public override bool CanRun()
        {
            LOG.Console("ability rig check");

            if (section.TargetLocal == null)
                return false;

            if (section.Root == null)
                return false;

            //if (!isMovementEffected && GAME.Vars.Get<Vec>("input:direction", new Vec()).Magnitude() > 0.1)
            //    return false;

            return true;
        }
        public override void Begin()
        {
            base.Begin();

            localTargetOffset = section.Vars.Get<Vector3>("effector_target_local_position", section.TargetLocal.transform.localPosition);
            localRootOffset = section.Vars.Get<Vector3>("effector_root_position", section.Root.transform.localPosition);

            timer = 0f;
        }
        public override void End()
        {
            base.End();
        }
        public override void TickPhysicsAbility()
        {
            base.TickPhysicsAbility();

            if (section.TargetLocal == null)
            {
                Stop();
                return;
            }


            float timeFactor = 0f;
            Vec input = GAME.Vars.Get<Vec>("input:direction", new Vec());
            if (input.Magnitude() > 0.1f)
            {
                if (isMovementEffected)
                {
                    timeFactor = input.Magnitude();
                } else
                {
                    //Stop();
                    //return;
                }
            }
            float timeNow = time - timeFactor;
            if (timeNow < 0.1f)
                timeNow = 0.1f;

            timer += TIME.DeltaPhysics / timeNow;
            float scaleFactor = 1f;
            if (isMovementEffected)
                scaleFactor = input.Magnitude();

            float y = UTIL.CalculateSineY(timer, 0f, 1f, height+(height*scaleFactor));

            localTargetOffset = section.Vars.Get<Vector3>("effector_target_local_position", Vector3.zero);
            localRootOffset = section.Vars.Get<Vector3>("effector_root_position", Vector3.zero);

            Vector3 axesVec = new Vector3(0,0,0);
            if (axes.Contains("x"))
            {
                axesVec.x = 1f;
            }
            if (axes.Contains("y"))
            {
                axesVec.y = 1f;
            }
            if (axes.Contains("z"))
            {
                axesVec.z = 1f;
            }

            if (axes.Contains("-"))
            {
                axesVec = axesVec*-1f;
            }

            if (isTarget)
            {
                section.TargetLocal.transform.localPosition = Vector3.Lerp(
                    section.TargetLocal.transform.localPosition,
                    localTargetOffset + new Vector3(y*axesVec.x, y*axesVec.y, y*axesVec.z)*dir,
                    timer
                );

                section.Target.transform.position = Vector3.Lerp(
                    section.Target.transform.position,
                    section.TargetLocal.transform.position,
                    TIME.Delta*8f
                );
            }
            if (isRoot)
            {
                if (isRootFollow)
                {
                    Node n = GAME.Locations.Get<Node>(rootFollowName, null);
                    if (n != null)
                    {
                        section.Root.transform.position = Vector3.Lerp(
                            section.Root.transform.position,
                            n.transform.position + new Vector3(y*axesVec.x, y*axesVec.y, y*axesVec.z)*dir,
                            timer
                        );
                        return;
                    }
                }
                section.Root.transform.localPosition = Vector3.Lerp(
                    section.Root.transform.localPosition,
                    localRootOffset + new Vector3(y*axesVec.x, y*axesVec.y, y*axesVec.z)*dir,
                    timer
                );
            }
        }
    }
    
}