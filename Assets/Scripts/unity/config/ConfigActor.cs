namespace snorri
{
    public class ConfigActor : Actor
    {
        protected override void Setup()
        {
            base.Setup();

            CONFIG.Init(Map.FromJson("config"));
        }
    }
}