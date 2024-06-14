using UnityEngine;
using System.Collections.Generic;

namespace snorri
{
    public class CellGenPass : ScenePass
    {
        Map Data {get; set;}
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
                Data = dataMap;
                return;
            }
            
            string cellDataFile = Vars.Get<string>("cell_data_file", "cell_data");
            Data = Map.FromJson(cellDataFile);

            Data.Log();

            Vars.Set<Map>("cell_data", Data);
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

            Node.Execute<string>("load", routineArgs);
        }

        void WhenTaskCallback(string key)
        {
            string currentNodeName = Spawned.Get<string>(key, "");
            if (currentNodeName != "")
            {
                Node currentNode = NODE.Tree.Get<Node>(currentNodeName, null);
                if (currentNode != null)
                    currentNode.Terminate();
            }

            // builds the nodes as children of this node, therefore this will have a decent amount of children
            Map cellMap = Data.Get<Map>(key, null);
            if (cellMap == null)
                return;

            Map surfacesMap = cellMap.Get<Map>("surfaces", new Map());
            foreach (string keyName in surfacesMap.Elements.Keys)
            {
                // surface could have an item
                Map surfaceMap = surfacesMap.Get<Map>(keyName, new Map());
                
                Map itemsMap = surfaceMap.Get<Map>("items", new Map());
                foreach (string itemName in itemsMap.Elements.Keys)
                {
                    LOG.Console($"cell pass item: {itemName}");
                    continue;

                    Map itemMap = itemsMap.Get<Map>(itemName);
                    Item item = Item.FromJson(itemName);
                    string nodeName = item.NodeName;

                    itemMap.Set<string>("inherit_from", nodeName);
                    Node newNode = Node.AddChild(key + "_" + itemName, itemMap);
                    Spawned.Set<string>(key, newNode.Name);
                }
            }
        }
        void WhenTaskComplete()
        {
            IsComplete = true;
        }
    }
}