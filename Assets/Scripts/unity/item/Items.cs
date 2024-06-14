namespace snorri
{
    public class Items : Element
    {
        // functions as an inventory, registers with game vars

        public Map Vars {get; set;}
        public Items(string name, Map vars)
        {
            this.Name = name;
            Vars = vars;

            GAME.Vars.Set<Map>($"items:{name}", vars);
        }

        public void AddItem(string itemName, int amount)
        {
            if (Vars.Has(itemName))
            {
                Vars.Set<int>($"{itemName}:{amount}", amount);
            }
        }

        public static string JsonPathItems = "items/";
        // statics
        public static Items FromJson(string name)
        {
            Map itemMap = Map.FromJson(JsonPathItems + name);
            if (itemMap == null)
            {
                LOG.Console("ERROR: __ITEMS__ could not find item for name: " + name);
                return null;
            }
            return new Items(name, itemMap);
        }
        public static Items FromJson(int id)
        {
            Map idToNameMap = Map.FromJson(JsonPathItems + "_map");
            if (idToNameMap == null)
            {
                LOG.Console("ERROR: __ITEMS__ could not find item-id-name map");
                return null;
            }
            string itemName = idToNameMap.Get<string>(id.ToString(), "");
            if (itemName == "")
            {
                LOG.Console($"ERROR: __ITEMS__ could not find item name for id {id}");
                return null;
            }

            return FromJson(itemName);
        }
        public static Items FromJsonSave(string name)
        {
            return new Items(name, Map.FromJsonSave(JsonPathItems + name));
        }
    }
}