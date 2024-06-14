using System;

namespace snorri
{
    public class Task : TaskBase
    {
        public Action task;
        public Task(Action task)
        {
            this.task = task;
        }
        public Task (params Action[] actions)
        {
            this.task = () =>
            {
                foreach (Action task in actions)
                {
                    if (task != null)
                        task();
                }
            };
        }
        public void Execute()
        {
            if (task != null)
                task();
        }
    }
    public class Task<T> : TaskBase
    {
        public Action<T> task;
        public Task(Action<T> task)
        {
            this.task = task;
        }
        public void Execute(T val)
        {
            if (task != null)
                task(val);
        }
    }
    public class Task<T, U> : TaskBase
    {
        public Func<T, U> task;
        public Task(Func<T, U> func)
        {
            this.task = func;
        }
        public U Execute(T val)
        {
            if (task != null)
                return task(val);
            
            return default(U);
        }
    }
    public class Task<T, U, V> : TaskBase
    {
        public Func<T, U, V> task;
        public Task(Func<T, U, V> func)
        {
            this.task = func;
        }
        public V Execute(T val1, U val2)
        {
            if (task != null)
                return task(val1, val2);
            
            return default(V);
        }
    }
    public class Task<T, U, V, W> : TaskBase
    {
        public Func<T, U, V, W> task;
        public Task(Func<T, U, V, W> func)
        {
            this.task = func;
        }
        public W Execute(T val1, U val2, V val3)
        {
            if (task != null)
                return task(val1, val2, val3);
            
            return default(W);
        }
    }
    public class TaskBase : Element
    {

    }
}