using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Unity.Jobs;

namespace snorri
{
    public abstract class OperationJob : Operation
    {
        protected abstract void BeginJob();
        protected virtual void WhenJobComplete()
        {

        }

        public bool IsJobScheduled {get; set;}
        private JobHandle jobHandle;

        public override void Execute(bool val, 
            Task taskWhenExecute = null, 
            params Element[] args)
        {
            base.Execute(val, taskWhenExecute, args);

            Map waitVars = new Map();
            waitVars.Set<Task<int, bool>>("task_wait_condition", new Task<int, bool>(WhenJobWait));

            this.Node.Execute("wait", waitVars);

            IsJobScheduled = true;

            BeginJob();
        }

        bool WhenJobWait(int index)
        {
            if (IsJobScheduled && jobHandle.IsCompleted)
            {
                // we have completed the job
                jobHandle.Complete();
                IsJobScheduled = false;

                WhenJobComplete();

                return true;
            }

            return false;
        }
    }
}