namespace snorri
{
    public abstract class AbilityPlayer : Ability
    {
        protected BodyTicker body;
        protected CameraModule camera;
        protected CastTicker caster;
        protected InputActor input;
        protected AnimModule animator;

        protected override void Setup()
        {
            base.Setup();

            body = Node.GetActor<BodyTicker>();
            caster = Node.GetActor<CastTicker>();
            input = Node.GetActor<InputActor>();
            animator = Node.GetActor<AnimModule>();
        }
        protected override void Launch()
        {
            base.Launch();

            camera = GAME.Vars.Get<CameraModule>("camera_main", null);

            if (camera == null)
                LOG.Console("ability player could not locate main camera");
        }
        public override void TickAbility()
        {
            base.TickAbility();

            if (camera == null)
                camera = GAME.Vars.Get<CameraModule>("camera_main", null);
        }
    }
}