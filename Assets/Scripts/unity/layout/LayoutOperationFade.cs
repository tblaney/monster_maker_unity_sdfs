using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;


namespace snorri
{
    public class LayoutOperationFade : Operation
    {
        /*
            VARS:
                --> opacity_in
                --> opacity_out
                --> time
        */

        LayoutCanvasGroupModule canvasGroup;

        protected override void Setup()
        {
            base.Setup();

            canvasGroup = Node.GetActor<LayoutCanvasGroupModule>();
        }

        public override void Execute(
            bool val, 
            Task taskWhenExecute = null, 
            params Element[] args)
        {
            base.Stop();

            Task<float> taskCallback = new Task<float>(
                (float f) =>
                {
                    canvasGroup.Opacity = f;
                }
            );

            Map argsLerp = new Map();

            argsLerp.Set<string>("lerp_type", "float");
            argsLerp.Set<float>("val_in", canvasGroup.Opacity);
            if (val)
                argsLerp.Set<float>("val_out", Vars.Get<float>("opacity_in", 0f));
            else
                argsLerp.Set<float>("val_out", Vars.Get<float>("opacity_out", 1f));

            argsLerp.Set<float>("time", Vars.Get<float>("time", 0.5f));
            argsLerp.Set<Task<float>>("task_when_callback", taskCallback);
            argsLerp.Set<Task>("task_when_end", taskWhenExecute);

            Node.Execute<float>("lerp", argsLerp);
        }

        protected override void WhenStop()
        {
            // base cleanup
        }
    }
}