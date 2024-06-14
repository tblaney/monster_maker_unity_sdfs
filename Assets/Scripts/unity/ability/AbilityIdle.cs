namespace snorri
{
    public class AbilityIdle : Ability
    {
        public override bool CanRun()
        {
            return true;
        }
        public override void Begin() 
        { 
            base.Begin();

            LOG.Console("ability idle start!");
        }
        public override void End() 
        { 
            base.End();

            LOG.Console("ability idle stop!");
        }
    }
}