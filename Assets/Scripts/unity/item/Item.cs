namespace snorri
{
    public class Item : Element
    {
        public Map Vars {get; set;}

        public override int Id {get { return Vars.Get<int>("id", -1); } }
        public string MeshName { get { return Vars.Get<string>("mesh", ""); } }

        public Node Node {get; set;}

        public string NodeName { get { return Vars.Get<string>("node", ""); } }

        public Item()
        {

        }
        public Item(string name, Map vars)
        {
            Name = name;
            Vars = vars;
        }

        public void Render(Vec position, Node parentNode)
        {
            Map setupMap = new Map();
            setupMap.Set<string>("inherit_from", "item");
            setupMap.Set<string>("actors:item_actor:item_name", this.Name);
            parentNode.AddChild($"item_{Name}", setupMap);
            //Entity e = PREFAB.SpawnMesh(MeshName);
            /*
            Map overrideMap = new Map();
            overrideMap.Set<string>("actors:item_actor:mesh_name", MeshName);
            overrideMap.Log();

            Entity e = Entity.FromJson("item_basic", overrideMap);
            
            Point p = e.GetComponent<Point>();
            p.Position = position;

            Entity = e;
            */
        }




        public static string JsonPathItems = "items/";
        // statics
        public static Item FromJson(string name)
        {
            Map itemMap = Map.FromJson(JsonPathItems + name);
            if (itemMap == null)
            {
                LOG.Console("ERROR: __ITEMS__ could not find item for name: " + name);
                return null;
            }
            return new Item(name, itemMap);
        }
        public static Item FromJson(int id)
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
        public static int GetId(string name)
        {
            Item item = FromJson(name);
            return item.Id;
        }
        public static string GetName(int id)
        {
            Item item = FromJson(id);
            return item.Name;
        }
    }
}