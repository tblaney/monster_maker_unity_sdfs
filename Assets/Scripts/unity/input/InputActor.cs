using UnityEngine;

namespace snorri
{
    public class InputActor : Actor
    {
        protected override void Setup()
        {
            base.Setup();
        }

        void ConfigureVars()
        {
            this.Node.Vars.Set<Task<string, bool>>("is_clicked", new Task<string, bool>(IsClicked));
            this.Node.Vars.Set<Task<string, bool>>("is_pressed", new Task<string, bool>(IsPressed));
        }

        public bool IsClicked(string keyName)
        {
            string key = GAME.Vars.Get<string>("input:keys:" + keyName, "");
            if (key =="")
            {
                // LOG.Console($"input actor is clicked, could not find name: {keyName}");
                return false;
            }

            // LOG.Console($"input actor is clicked: {key}");

            switch (key)
            {
                default:
                    return Input.GetKeyDown(key);
                case "left":
                    return Input.GetMouseButtonDown(0);
                    break;
                case "right":
                    return Input.GetMouseButtonDown(1);
                    break;
                case "middle":
                    return Input.GetMouseButtonDown(2);
                    break;
                case "middle delta positive":
                    return Input.mouseScrollDelta.y > 0f;
                case "middle delta negative":
                    return Input.mouseScrollDelta.y < 0f;
            }
            return false;
        }
        public bool IsPressed(string keyName)
        {
            string key = GAME.Vars.Get<string>("input:keys:" + keyName, "");
            if (key =="")
                return false;

            switch (key)
            {
                default:
                    return Input.GetKey(key);
                case "left":
                    return Input.GetMouseButton(0);
                    break;
                case "right":
                    return Input.GetMouseButton(1);
                    break;
                case "middle":
                    return Input.GetMouseButton(2);
                    break;
                case "middle delta positive":
                    return Input.mouseScrollDelta.y > 0f;
                case "middle delta negative":
                    return Input.mouseScrollDelta.y < 0f;
            }
            return false;
        }
    }
}