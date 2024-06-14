namespace snorri
{
    public class AbilitiesTicker : Abilities
    {
        public override void Tick()
        {
            base.Tick();

            if (Current == null)
            {
                Choose();
                return;
            } else
            {
                Current.TickAbility();

                Choose();
            }

            if (isLog)
                LOG.Console($"abilities ticker: {Node.Vars.Get<string>("ability_current_name", "none")}");
        }
        public override void TickPhysics()
        {
            base.TickPhysics();

            if (Current != null)
                Current.TickPhysicsAbility();
        }
    }
}