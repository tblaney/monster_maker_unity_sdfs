using UnityEngine;
namespace snorri
{
    public static class NODE
    {
        public static Map Stages {get; set;} // stages

        public static Map Tree {get; set;} // hierarchy

        private static Task<string, GameObject> TaskToSpawnPrefab {get; set;}

        public static void Init(Task<string, GameObject> taskSpawnPrefab)
        {
            Stages = Map.FromJson("node_stages");
            Stages.Log();
            Tree = new Map();

            TaskToSpawnPrefab = taskSpawnPrefab;
        }

        public static GameObject NewPrefab(string prefabName)
        {
            GameObject obj = TaskToSpawnPrefab.Execute(prefabName);
            obj.name = prefabName;
            return obj;
        }
        public static GameObject New(string name)
        {
            GameObject obj = new GameObject(name);
            return obj;
        }

        public static void SetStage(string name)
        {

        }
            
    }
}