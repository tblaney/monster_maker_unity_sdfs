using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
namespace snorri
{
    public abstract class Module : MonoBehaviour, IActor, INameable
    {
        // these classes are primarily responsible for communicating with the UnityEngine, hence the need for AddComponents

        /*
            An example usage would be a BodyModule (rigidbody), which you can communicate with through the Node
        */

        // IActor properts, methods:

        public string Name {get; set;}
        public Node Node {get; set;}
        public Map Vars {get; set;}

        public bool IsCooldown {get; set;}

        protected TriggerListener triggerListener;

        public void Link(Node n, Map vars)
        {
            Node = n;
            Vars = vars;

            Name = Vars.Get<string>("name", "module");

            AddClasses();
        }
        public void Inform(ActorState state)
        {
            switch (state)
            {
                case ActorState.Setup:
                    Setup();
                    break;
                case ActorState.Launch:
                    Launch();
                    break;
                case ActorState.Enable:
                    WhenEnable();
                    break;
                case ActorState.Disable:
                    WhenDisable();
                    break;
                case ActorState.Destroy:
                    WhenDestroy();
                    break;
                case ActorState.Tick:
                    Tick();
                    break;
                case ActorState.TickPhysics:
                    TickPhysics();
                    break;
            }
        }

        protected virtual void AddClasses() {}

        protected virtual void Setup() { SetupVars(); }
        void SetupVars() 
        {
            if (Vars == null)
                Vars = new Map();      
                    
            SaveCheck();

            Map launchMap = Vars.Get<Map>("launch_map", null);
            if (launchMap != null)
            {
                foreach (string key in launchMap.Elements.Keys)
                {
                    Map elementLaunchMap = launchMap.Get<Map>(key, new Map());
                    if (elementLaunchMap.Length > 0)
                    {
                        // has params
                        InvokeMethodByNameWithParameters(UTIL.ConvertStringToMethod(key), elementLaunchMap);
                    } else
                    {
                        // no params
                        InvokeMethodByName(UTIL.ConvertStringToMethod(key));
                    }
                }
            }

            triggerListener = new TriggerListener();
            triggerListener.Listen(Trigger.WhenSave, new Task(Save));
        }
        protected virtual void Save()
        {   
            string saveName = Vars.Get<string>("save_name", "");
            if (saveName != "")
            {
                Vars.Write(saveName);
            }
        }
        protected virtual void SaveCheck()
        {
            string saveName = Vars.Get<string>("save_name", "");
            if (saveName != "")
            {
                Map vars = JSON.GetSaveMap(saveName);
                if (vars != null)
                    Vars = vars;
            }
        }
        void InvokeMethodByName(string methodName)
        {
            MethodInfo methodInfo = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            methodInfo?.Invoke(this, null); // Pass 'null' for methods without parameters
        }

        void InvokeMethodByNameWithParameters(string methodName, Map parameters)
        {
            MethodInfo methodInfo = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (methodInfo != null)
            {
                ParameterInfo[] paramsInfo = methodInfo.GetParameters();
                object[] args = new object[paramsInfo.Length];

                for (int i = 0; i < paramsInfo.Length; i++)
                {
                    if (parameters.Has(paramsInfo[i].Name))
                    {
                        args[i] = parameters.GetObject(paramsInfo[i].Name);
                    }
                    else
                    {
                        LOG.Console($"Missing parameter for method {methodName}: {paramsInfo[i].Name}");
                        return;
                    }
                }

                methodInfo.Invoke(this, args);
            }
            else
            {
                LOG.Console($"Method not found: {methodName}");
            }
        }

        protected virtual void Launch() {}
        protected virtual void WhenDisable() {}
        protected virtual void WhenEnable() {}
        protected virtual void WhenDestroy() {}

        void Update()
        {
            Tick();
        }
        void FixedUpdate()
        {
            TickPhysics();
        }

        public virtual void Tick() {}
        public virtual void TickPhysics(){}

        public T ComponentCheck<T>() where T : Component
        {
            T val = this.gameObject.GetComponent<T>();
            if (val == null)
                return this.gameObject.AddComponent<T>();

            return val;
        }

        protected void Cooldown(float time)
        {
            IsCooldown = true;
            Vars.Set<Coroutine>("actor_cooldown", Node.Entity.InvokeTask(new Task(CancelCooldown), time));
        }
        protected void CancelCooldown()
        {
            //Node.Entity.CancelInvoke();
            Coroutine routine = Vars.Get<Coroutine>("actor_cooldown", null);
            if (routine != null)
                Node.Entity.StopCoroutine(routine);

            IsCooldown = false;
        }
    }
}