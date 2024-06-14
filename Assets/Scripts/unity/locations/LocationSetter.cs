namespace snorri
{
    public class LocationSetter : Actor
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

            Node n = locationsMap.Get<Node>(locationName);

            if (n != null)
            {
                this.Node.Point.Position = n.Point.Position;
            }
        }
    }
}