namespace snorri
{
    public abstract class AbilityRig : Ability
    {
        protected RigSection section;
        protected InputActor input;
        protected CastTicker caster;

        protected override void Setup()
        {
            base.Setup();

            section = Node.GetActor<RigSection>();
            input = Node.GetActor<InputActor>();
            caster = Node.GetActor<CastTicker>();
        }
        public override void Begin()
        {
            base.Begin();
        }
        protected override void Launch()
        {
            base.Launch();
        }
        public override void TickAbility()
        {
            base.TickAbility();
        }
    }
}