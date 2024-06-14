namespace snorri
{
    public class ItemSpawnProcessor : Module
    {
        public string itemName;

        Node itemObjectSpawned;
        string itemNameCache;

        public override void Tick()
        {
            if (IsCooldown)
                return;

            if (itemName != itemNameCache)
            {
                Clear();

                itemNameCache = itemName;

                AttemptSpawn(itemName);
            }
        }
        public override void TickPhysics()
        {
        }


        void Clear()
        {
            if (itemObjectSpawned != null)
            {
                itemObjectSpawned.Terminate();
            }
        }

        void AttemptSpawn(string itemName)
        {
            Clear();

            if (itemObjectSpawned != null)
                return;

            Item item = Item.FromJson(itemName);

            if (item != null)
            {
                LOG.Console("item spawner found item! for: " + itemName);

                // attempt spawn
                item.Render(Vec.zero, this.Node);
                itemObjectSpawned = item.Node;
                
            } else
            {
                LOG.Console("block spawner did not find block: " + itemName);
            }

            Cooldown(1f);
        }
    }
}