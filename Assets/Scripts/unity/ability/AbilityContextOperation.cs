namespace snorri
{
    public class AbilityContextOperation : Ability
    {
        public override bool CanRun()
        {
            return GAME.Context == Vars.Get<string>("context", "loading");
        }
        public override void Begin() 
        { 
            base.Begin();
            string operationName = Vars.Get<string>("operation_name", "");
            if (operationName != "")
                Node.ExecuteOperation(operationName, true);

            LOG.Console("ability context operation occured with context: " + GAME.Context);
        }
        public override void End() 
        { 
            base.End();
            string operationName = Vars.Get<string>("operation_name", "");
            if (operationName != "")
                Node.ExecuteOperation(operationName, false);

            LOG.Console("ability context operation stopped with context: " + GAME.Context);
        }
    }
}