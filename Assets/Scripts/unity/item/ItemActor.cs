namespace snorri
{
    public class ItemActor : Actor
    {
        protected override void Setup()
        {
            base.Setup();
        }

        void Configure()
        {
            //Item item = Item.FromJson(Vars.Get<string>("item_name", ""));
            //if (item != null)
            //    item.Render(Vec.zero, this.Node);
        }
    }
}