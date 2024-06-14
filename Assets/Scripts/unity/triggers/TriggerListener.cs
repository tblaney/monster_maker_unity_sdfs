namespace snorri
{
    public class TriggerListener
    {
        Bag<Element> triggers;
        public TriggerListener()
        {
            triggers = new Bag<Element>();
            Trigger.e += WhenTrigger;
        }
        void WhenTrigger(object sender, Element args)
        {
            // LOG.Console("trigger actor when trigger: " + args.Name);
            foreach (Element e in triggers)
            {
                // LOG.Console("trigger actor when trigger consider elem: " + e.Name);

                if (e.Name == args.Name)
                {
                    // match
                    Task taskBase = e as Task;
                    if (taskBase != null)
                    {
                        taskBase.Execute();
                        continue;
                    }

                    Task<Element> taskElement = e as Task<Element>;
                    if (taskElement != null)
                    {
                        taskElement.Execute(args);
                        continue;
                    }
                }
            }
        }
        public void Listen(string name, Task taskCallback)
        {
            taskCallback.Name = name;

            triggers.Append(taskCallback as Element);
        }
        public void Listen(string name, Task<Element> taskCallback)
        {
            taskCallback.Name = name;

            triggers.Append(taskCallback as Element);
        }

   
    }
}