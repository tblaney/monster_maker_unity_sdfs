namespace snorri
{
    public class Location : Actor
    {
        protected override void Setup()
        {
            base.Setup();

            ConfigureVars();
        }
        void ConfigureVars()
        {
            string locationName = Vars.Get<string>("location_name", "");
            if (locationName == "")
                return;

            Map locationsMap = GAME.Locations;

            locationsMap.Set<Node>(locationName, this.Node);
        }
    }
}