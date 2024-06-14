using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace snorri
{
    public abstract class Actor : Element, IActor, IMap
    {
        public string Name { get; set;}
        public Node Node {get; set;}
        public Map Vars {get; set;}

        // properties
        public bool IsCooldown {get; private set;}
        public bool IsActive {get; set;}

        public Point Point {get; set;}

        // fields
        protected TriggerListener triggerListener;

        protected bool isLog = false;



        // ----------------------------------------------------
        // init
        public void Link(Node n, Map vars)
        {
            Node = n;
            Vars = vars;

            Name = Vars.Get<string>("name", "actor");
        }
        public virtual void Inform(ActorState state)
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
            }
        }

        // ----------------------------------------------------
        // virtuals called from initiatlize
        protected virtual void Setup() { SetupVars(); }
        void SetupVars() 
        {
            if (Vars == null)
                Vars = new Map();      
                    
            SaveCheck();
            
            isLog = Vars.Get<bool>("is_log", false);

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
        protected virtual void Launch() 
        {
        }
        protected virtual void WhenEnable() {}
        protected virtual void WhenDisable() {}
        protected virtual void WhenDestroy() {}

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

        // ----------------------------------------------------
        // Cooldown
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

        // invokes

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
    }
}
