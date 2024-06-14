using UnityEngine;
using System;

namespace snorri
{
    [UnityEngine.DisallowMultipleComponent]
    public class NodeSystem : MonoBehaviour
    {
        public Bag<NodeStage> Stages {get; set;}

        private TriggerListener Listener {get; set;}

        void Awake()
        {
            NODE.Init(new Task<string, GameObject>(SpawnPrefab));

            Stages = new Bag<NodeStage>();

            Listener = new TriggerListener();
            Listener.Listen(Trigger.WhenStageChange, new Task(WhenStageChange));
        }

        void WhenStageChange()
        {
            UnlinkCheck();

            LOG.Console("node system stage change refresh: " + GAME.Stage);

            Map stageMap = NODE.Stages.Get<Map>(GAME.Stage, new Map());
            NodeStage stage = new NodeStage(stageMap);

            stage.Build();

            Stages.Append(stage);
        }
        void UnlinkCheck(bool isForce = false)
        {
            Bag<NodeStage> removals = new Bag<NodeStage>();
            foreach (NodeStage stage in Stages)
            {
                if (!stage.IsPersisitent | isForce)
                {
                    stage.Terminate();

                    removals.Append(stage);
                }
            }

            Stages.Remove(removals);
        }

        // spawners
        public GameObject SpawnPrefab(string name)
        {
            LOG.Console($"node system spawn prefab: {name}");
            string resourceName = name;
            GameObject prefabObject = RESOURCES.GetGameObject(resourceName);
            
            GameObject newObject = Instantiate(prefabObject);

            return newObject;
        }
    }
}