using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace snorri
{
    [UnityEngine.DisallowMultipleComponent]
    public class NodeEntity : MonoBehaviour, INameable
    {
        public string Name { get; set; }
        public Node Node {get; set;}

        public Bag<GameObject> Children {get; set;}

        public bool IsWoke {get; private set;}
        public bool IsStarted {get; private set;}

        public Point Point {get; set;}

        public void Link(string name, Node node)
        {
            LOG.Console("node entity link! " + name);

            // invoked immediately after 'Awake', so it functions effectively the same
            Name = name;
            Node = node;

            Point = node.GetActor<Point>();

            foreach (IActor actor in Node.Actors)
            {
                actor.Inform(ActorState.Setup);
            }

            IsWoke  = true;
        }

        void Start()
        {
            foreach (IActor actor in Node.Actors)
            {
                actor.Inform(ActorState.Launch);
            }

            IsStarted = true;
        }
        void OnEnable()
        {
            if (Node == null)
                return;

            foreach (IActor actor in Node.Actors)
            {
                actor.Inform(ActorState.Enable);
            }
        }
        void OnDisable()
        {
            if (Node == null)
                return;

            foreach (IActor actor in Node.Actors)
            {
                actor.Inform(ActorState.Disable);
            }
        }
        void OnDestroy()
        {
            if (Node == null)
                return;

            foreach (IActor actor in Node.Actors)
            {
                actor.Inform(ActorState.Destroy);
            }
        }
        void Update()
        {
            foreach (Ticker ticker in Node.Tickers)
            {
                ticker.Inform(ActorState.Tick);
            }
        }
        void FixedUpdate()
        {
            foreach (Ticker ticker in Node.Tickers)
            {
                ticker.Inform(ActorState.TickPhysics);
            }
        }
        
        public void Activate(bool val)
        {
            this.gameObject.SetActive(val);
        }
        public void Terminate()
        {
            DestroyImmediate(this.gameObject);
        }
        public bool FindChild(string name, out GameObject obj)
        {
            obj = null;
            foreach (Transform child in this.transform)
            {
                if (child.gameObject.name == name)
                {
                    obj = child.gameObject;
                    return true;
                }
            } return false;
        }

        public Coroutine InvokeTask(Task actionOnDelay, float time)
        {
            return StartCoroutine(RoutineInvoke(actionOnDelay, time));
        }
        public Coroutine InvokeWait(
            Task<int, bool> actionWaitCondition, Task actionOnComplete
        )
        {
            return StartCoroutine(RoutineWait(actionWaitCondition, actionOnComplete));
        }
        public Coroutine InvokeLoad<T>(
            IEnumerable<T> source, Task<T> actionCallback, int counter = 200, Task actionOnEnd = null
        )
        {
            return StartCoroutine(RoutineLoad<T>(
                source, actionCallback, counter, actionOnEnd
            ));
        }  
        public Coroutine InvokeLerp<T>(string name, T valIn, T valOut, float time = 1f, Task<T> taskCallback = null, Task taskWhenEnd = null)
        {
            if (!this.gameObject.activeInHierarchy)
            {
                if (taskCallback != null)
                    taskCallback.Execute(valOut);
                return null;
            }
            TaskBase taskCallbackLerp = null;

            switch (name)
            {
                case "float":
                    taskCallbackLerp = new Task<float,float,float,float>(
                        (x, y, z) => {
                            return Mathf.Lerp(x, y, z);
                        }
                    );
                    break;
                case "vec":
                    taskCallbackLerp = new Task<Vec,Vec,float,Vec>(
                        (x, y, z) => {
                            return x.Lerp(y, z);
                        }
                    );
                    break;
                case "point":
                    taskCallbackLerp = new Task<PointState,PointState,float,PointState>(
                        (x, y, z) => {
                            return x.Lerp(y, z);
                        }
                    );
                    break;
                case "rotation":
                    taskCallbackLerp = new Task<Vec,Vec,float,Vec>(
                        (x, y, z) => {
                            return x.Lerp(y, z, true);
                        }
                    );
                    break;
                case "text":
                    taskCallbackLerp = new Task<string,string,float,string>(
                        (x,y,z) => {
                            char[] chars = y.ToCharArray();
                            float countFloat = ((float)(chars.Length))*z;
                            int count = (int)countFloat;
                            string textOut = "";
                            int i = 0;
                            foreach (char c in chars)
                            {
                                textOut+=c.ToString();
                                i++;
                                if (i > count)
                                    break;
                            }

                            return textOut;
                        }
                    );
                    break;
            }
            if (taskCallbackLerp == null)
                return null;


            Coroutine routineLoad = StartCoroutine(RoutineLerp<T>(valIn, valOut, taskCallbackLerp, time, taskCallback, taskWhenEnd));

            return routineLoad;
        } 
        private Coroutine InvokeLerp<T>(T valIn, T valOut, TaskBase taskCallbackLerp, float time = 1f, Task<T> taskCallback = null)
        {
            if (!this.gameObject.activeInHierarchy)
            {
                if (taskCallback != null)
                    taskCallback.Execute(valOut);
                return null;
            }
            if (taskCallbackLerp == null)
                return null;

            Coroutine routineLoad = StartCoroutine(RoutineLerp<T>(valIn, valOut, taskCallbackLerp, time, taskCallback));

            return routineLoad;
        } 
        private IEnumerator RoutineInvoke(Task actionOnDelay, float time)
        {
            yield return new WaitForSeconds(time);
            
            if (actionOnDelay != null)
                actionOnDelay.Execute();
        }
        private IEnumerator RoutineLoad<T>(IEnumerable<T> source, Task<T> actionCallback, int counter = 200, Task actionOnEnd = null)
        {
            int i = 0;
            foreach (T val in source)
            {
                actionCallback.Execute(val);
                i++;
                if (i%counter == 0)
                {
                    yield return null;
                }
            }

            if (actionOnEnd != null)
                actionOnEnd.Execute();
        }
        private IEnumerator RoutineWait(Task<int, bool> actionWaitCondition, Task actionOnComplete)
        {
            while (!actionWaitCondition.Execute(0))
            {
                yield return null;
            }   
            actionOnComplete.Execute();
        }
        private IEnumerator RoutineLerp<T>(T valIn, T valOut, TaskBase taskCallbackLerp, float time = 1f, Task<T> taskCallback = null, Task taskWhenComplete = null)
        {
            yield return null;

            Task<T,T,float,T> lerp_task = taskCallbackLerp as Task<T,T,float,T>;
            if (lerp_task == null)
                yield break;

            float t = 0f;
            if (taskCallback != null)
                taskCallback.Execute(valIn);

            while (t <= 1.0f)
            {   
                T val = lerp_task.Execute(valIn, valOut, t);

                if (taskCallback != null)
                    taskCallback.Execute(val);

                t += TIME.DeltaPhysics/time;
                yield return null;
            }

            if (taskCallback != null)
                taskCallback.Execute(valOut);

            if (taskWhenComplete != null)
                taskWhenComplete.Execute();

        }
    }
}