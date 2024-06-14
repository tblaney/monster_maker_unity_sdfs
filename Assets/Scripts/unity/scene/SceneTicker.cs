namespace snorri
{
    public class SceneTicker : Ticker
    {
        [ReadOnly]
        public bool IsCompleted;

        protected Bag<ScenePass> passes;

        protected override void Setup()
        {
            base.Setup();

            IsCompleted = false;
        }
        protected override void Launch()
        {
            base.Launch();

            GAME.Context = "loading";
        }

        void RefreshPasses()
        {
            passes = Node.GetBagOChildActors<ScenePass>();
        }

        public override void Tick()
        {
            if (IsCompleted)
                return;

            RefreshPasses();

            bool isComplete = true;
            foreach (ScenePass pass in passes)
            {
                if (!pass.IsExecuted)
                {
                    isComplete = false;
                    
                    // need to execute
                    if (pass.CanRun())
                    {
                        pass.Execute();
                    }
                    continue;
                }
                if (!pass.IsComplete)
                {
                    isComplete = false;
                }
            }

            if (isComplete)
            {
                Map delayMap = new Map();
                delayMap.Set<Task>("task", new Task(Complete));
                delayMap.Set<float>("time_delay", 1f);
                Node.Execute("delay", delayMap);

                IsCompleted = true;
            }
        }
        public override void TickPhysics()
        {

        }

        public void AddPass(ScenePass p)
        {
            passes.Append(p);
        }

        protected virtual void Complete()
        {
            GAME.Context = "run";

            IsCompleted = true;

            LOG.Console("scene ticker has completed!");
        }
    }
}