using UnityEngine;
using System.Collections.Generic;

namespace snorri
{
    public class CellPass : ScenePass
    {
        Map Data {
            get {
                return Vars.Get<Map>("cell_data", new Map());
            }
        }
        Map Spawned {get; set;}

        TriggerListener listener;

        protected override void Setup()
        {
            base.Setup();

            Spawned = new Map();

            listener = new TriggerListener();
            listener.Listen(Trigger.WhenCellUpdate, new Task<Element>(WhenCellUpdate));

            Configure();

            CELL.Init(this.Vars);
        }

        void Configure()
        {
            Map dataMap = Vars.Get<Map>("cell_data", null);
            if (dataMap != null)
            {
                return;
            }
            
            string cellDataFile = Vars.Get<string>("cell_data_file", "cell_data");
            dataMap = Map.FromJson(cellDataFile);

            dataMap.Log();

            Vars.Set<Map>("cell_data", dataMap);
        }

        void WhenCellUpdate(Element e)
        {
            string cellToUpdateName = e.Name;

            WhenTaskCallback(cellToUpdateName);
        }

        public override void Execute()
        {
            base.Execute();

            LOG.Console("cell pass execute");

            // data vars is organized by key position string, with value being dictionary of params
            IEnumerable<string> keys = Data.Elements.Keys;
            Task<string> taskWhenCallback = new Task<string>(WhenTaskCallback);
            Task taskWhenComplete = new Task(WhenTaskComplete);

            Map routineArgs = new Map();
            routineArgs.Set<IEnumerable<string>>("source_collection", keys);
            routineArgs.Set<Task<string>>("task_when_callback", taskWhenCallback);
            routineArgs.Set<Task>("task_when_end", taskWhenComplete);
            routineArgs.Set<int>("counter", Vars.Get<int>("counter", 200));

            this.Node.Execute<string>("load", routineArgs);
        }

        void WhenTaskCallback(string key)
        {
            Despawn(key);

            // builds the nodes as children of this node, therefore this will have a decent amount of children
            Map cellMap = Data.Get<Map>(key, null);
            if (cellMap == null)
                return;

            Map surfacesMap = cellMap.Get<Map>("surfaces", new Map());
            foreach (string keyName in surfacesMap.Elements.Keys)
            {
                // surface could have an item
                Map surfaceMap = surfacesMap.Get<Map>(keyName, new Map());
                
                Map itemMap = surfaceMap.Get<Map>("item", null);
                if (itemMap == null)
                    continue;

                string itemName = itemMap.Get<string>("item_name", "");

                Item item = Item.FromJson(itemName);
                string nodeName = item.NodeName;

                LOG.Console($"cell pass has item: {itemName}, with node: {nodeName}");

                itemMap.Set<string>("inherit_from", nodeName);
                Spawn(key, key + "_" + itemName, itemMap);
            }
        }
        public void Spawn(string key, string nodeName, Map overrideMap)
        {
            Node newNode = Node.AddChild(nodeName, overrideMap);
            Bag<string> bagOCurrentNodes = Spawned.Get<Bag<string>>(key, new Bag<string>());
            bagOCurrentNodes.Append(nodeName);
            Spawned.Set<Bag<string>>(key, bagOCurrentNodes);
        }
        public void Despawn(string key)
        {
            Bag<string> bagOCurrentNodes = Spawned.Get<Bag<string>>(key, null);
            if (bagOCurrentNodes != null && bagOCurrentNodes.Length > 0)
            {
                foreach (string currentNodeName in bagOCurrentNodes)
                {
                    Node currentNode = NODE.Tree.Get<Node>(currentNodeName, null);
                    if (currentNode != null)
                        currentNode.Terminate();
                }

                Spawned.Remove(key);
            }
        }
        void WhenTaskComplete()
        {
            IsComplete = true;
        }
    }
}