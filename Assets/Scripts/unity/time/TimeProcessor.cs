namespace snorri
{
    public class TimeProcessor : Ticker
    {
        protected override void Launch()
        {
            base.Launch();

            if (TIME.Data == null)
            {
                TIME.Data = new TimeData();
            }
        }
        public override void Tick()
        {
            TimeData data = TIME.Data;
            data.TickTime();

            TIME.Data = data;
        }
        public override void TickPhysics()
        {

        }

    }
}