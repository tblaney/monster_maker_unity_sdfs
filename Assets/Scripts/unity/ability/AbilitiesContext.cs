namespace snorri
{
    public class AbilitiesContext : Abilities
    {
        TriggerListener trigger;

        protected override void Setup()
        {
            base.Setup();

            trigger = new TriggerListener();
            trigger.Listen(Trigger.WhenContextChange, new Task(TriggerCallback));
        }

        void TriggerCallback()
        {   
            // evaluate from scratch
            Ability ab = Node.Vars.Get<Ability>("ability_current", null);
            if (ab != null)
            {
                ab.End();
            }
            Node.Vars.Remove("ability_current");
            Choose();
        }
    }
}