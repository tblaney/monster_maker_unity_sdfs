namespace snorri
{
    public class MotionTicker : Ticker
    {
        public Bag<MotionRig> Rigs {get; set;}
        public string Current { 
            get { return Vars.Get<string>("current", ""); }
            set { Vars.Set<string>("current", value); }
        }
        public string Past { 
            get { return Vars.Get<string>("past", ""); }
            set { Vars.Set<string>("past", value); }
        }
        public float Time {get; set;}

        public int FPS { get { return Vars.Get<int>("fps", 15); } }

        public void Set(string motionName, float transitionTime = 0f)
        {
            if (transitionTime < 0.01f)
            {
                Vars.Set<float>("transition_time", 0f);

            } else
            {
                Vars.Set<float>("transition_time", transitionTime);
            }

            Past = Current;
            Current = motionName;
        }

        protected override void Setup()
        {
            base.Setup();
            
            Rigs = new Bag<MotionRig>();
            foreach (string rigName in Vars.Get<Bag<string>>("rigs", new Bag<string>()))
            {
                //MotionRig newRig = new MotionRig(rigName, this.Node);
                //Rigs.Append(newRig);
            }       
        }

        public override void Tick()
        {
            base.Tick();

            RefreshRigs();
        }

        void RefreshRigs()
        {
            /*
                MAIN REFRESH ALGORITHM
                
                    1. loop through bones in all rigs

            */

            string animCurrent = Current;

            if (animCurrent == "") // no animation set
                return;

            float animTime = Time;
            animTime += TIME.Delta;

            // are we in a transition?
            float transitionTime = Vars.Get<float>("transition_time", 0f);
            if (transitionTime > 0.01f)
            {
                string animPast = Past;

                for (int i = 0; i < Rigs.Length; i++)
                {
                    MotionRig rig = Rigs[i];
                   // MotionRigState statePast = rig.GetState(animPast, animTime);
                    //sMotionRigState stateCurrent = rig.GetState(animCurrent, 0f);
                }

                Vars.Set<float>("transition_time", transitionTime - TIME.Delta);
            } else
            {
                bool inTransition = Vars.Get<bool>("in_transition", false);
                if (inTransition)
                {
                    // first frame out of transition
                    Vars.Set<bool>("in_transition", false);

                    animTime = 0f;
                } else
                {

                }
            }
        }
    }
}