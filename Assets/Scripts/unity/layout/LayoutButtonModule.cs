using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace snorri
{
    public class LayoutButtonModule : Module, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        Map Vars {get; set;}

        protected override void Setup()
        {
            base.Setup();

            Vars = new Map();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            CallbackCheck("hover_in");
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            CallbackCheck("hover_out");
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            CallbackCheck("click");
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            CallbackCheck("down");
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            CallbackCheck("up");
        }   

        void CallbackCheck(string typeName)
        {
            foreach (Task task in Vars.Get<Bag<Task>>(typeName, new Bag<Task>()))
            {
                task.Execute();
            }

            // operations:
            switch (typeName)
            {
                case "hover_in":
                    Node.ExecuteOperation("hover", true);
                    break;
                case "hover_out":
                    Node.ExecuteOperation("hover", false);
                    break;
                case "click":
                    Node.ExecuteOperation("click", true);
                    break;
                case "down":
                    Node.ExecuteOperation("press", true);
                    break;
                case "up":
                    Node.ExecuteOperation("press", false);
                    break;
            }
        }

        public void Listen(string typeName, Task task_callback)
        {
            Bag<Task> tasks = Vars.Get<Bag<Task>>(typeName, new Bag<Task>());
            tasks.Append(task_callback);

            Vars.Set<Bag<Task>>(typeName, tasks);
        }
    }
}