using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;


namespace snorri
{
    public abstract class Operation : Actor
    {
        protected Task taskWhenComplete;
        protected Coroutine routine;

        protected override void WhenDisable()
        {
            base.WhenDisable();
            
            if (taskWhenComplete != null)
            {
                taskWhenComplete.Execute();
                taskWhenComplete = null;
            }

            Stop();
        }

        public virtual void Execute(
            bool val, 
            Task taskWhenExecute = null, 
            params Element[] args)
        {
            Stop();
        }

        public void Stop()
        {
            if (routine != null)
                Node.Entity.StopCoroutine(routine);

            WhenStop();
        }
        protected virtual void WhenStop()
        {
            // base cleanup
        }
    }
}