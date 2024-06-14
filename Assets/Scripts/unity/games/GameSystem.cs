using UnityEngine;

// serves as the sole script you should need in a unity scene
namespace snorri
{
    [UnityEngine.DisallowMultipleComponent]
    public class GameSystem : MonoBehaviour
    {
        bool firstUpdate = false;
        
        public string setupStage = "stage_setup";
        public string startStage = "stage_main_menu";

        void Awake()
        {
            GAME.Init();    

            this.gameObject.AddComponent<NodeSystem>();

            firstUpdate = true;
        }

        void Start()
        {
            // should initiate the starting scene

            GAME.Stage = GAME.Vars.Get<string>(setupStage, "");
        }

        void Update()
        {
            if (firstUpdate)
            {
                GAME.Stage = GAME.Vars.Get<string>(startStage, "");
                firstUpdate = false;
            }
        }
    }
}