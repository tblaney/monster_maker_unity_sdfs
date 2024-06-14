using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;


namespace snorri
{
    public class PointOperationArg : Operation
    {           
        public override void Execute(bool val, Task taskWhenExecute = null, params Element[] args)
        {
            base.Execute(val, taskWhenExecute, args);

            Task<PointState> taskCallback = new Task<PointState>(
                (PointState p) =>
                {
                    Node.Point.State = p;
                }
            );

            if (args.Length > 0)
            {
                PointState p = args[0] as PointState;
                if (p != null)
                {
                    Map argsLerp = new Map();

                    argsLerp.Set<string>("lerp_type", "point");
                    argsLerp.Set<PointState>("val_in", Node.Point.State);
                    argsLerp.Set<PointState>("val_out", p);
                    argsLerp.Set<float>("time", Vars.Get<float>("time", 0.5f));
                    argsLerp.Set<Task<PointState>>("task_when_callback", taskCallback);
                    argsLerp.Set<Task>("task_when_complete", taskWhenExecute);

                    Node.Execute<PointState>("lerp", argsLerp);
                }
            }
        }
    }
}