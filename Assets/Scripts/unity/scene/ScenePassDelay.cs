using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace snorri
{
    public class ScenePassDelay : ScenePass
    {
        protected override void Setup()
        {
            base.Setup();
        }
        public override void Execute() { 
            base.Execute();

            Task whenDelay = new Task(
                () => {
                    IsComplete = true;
                }
            );

            Map args = new Map();
            args.Set<Task>("task", whenDelay);
            args.Set<float>("time_delay", Vars.Get<float>("time_delay", 10f));
            Node.Execute("delay", args);
        }
    }
}
