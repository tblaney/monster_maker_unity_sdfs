namespace snorri
{
    public abstract class Ability : Actor
    {
        /*
            VARS:
                [M] --> priority
        */

        public int Priority {
            get {
                return Vars.Get<int>("priority", 0);
            }
        }
        public bool IsRunning {get; set;}

        protected override void Setup()
        {
            base.Setup();

            this.abilities = Node.GetActor<Abilities>();

            IsActive = false;
        }

        public abstract bool CanRun();
        public virtual void Begin() { IsRunning = true; }
        public virtual void End() { IsRunning = false; }
        public virtual void TickAbility() {}
        public virtual void TickPhysicsAbility() {}

        protected virtual void Stop()
        {
            abilities.Clear();
        }
        
        protected Abilities abilities;
    }
}