using UnityEngine;
using System;
using System.Collections.Generic;

namespace snorri
{
    public class Node : IMap, INameable
    {
        public Map Vars {get; set;}
        public Map VarsSetup {get; set;} // should be discarded on link - its a one time use thing

        public string Name 
        {
            get
            {
                return Vars.Get<string>("name", "");
            } 
            set
            {   
                Vars.Set<string>("name", value);
            }
        }
        public string Parent { 
            get {
                return Vars.Get<string>("parent", "");
            }
            set {
                Vars.Set<string>("parent", value);
            }
        }
        public Bag<string> Children { 
            get {
                Bag<string> bagOfChildren = Vars.Get<Bag<string>>("children", null);
                if (bagOfChildren == null)
                {
                    bagOfChildren = new Bag<string>();
                    Vars.Set<Bag<string>>("children", bagOfChildren);
                }
                return bagOfChildren;
            }
            set {
                Vars.Set<Bag<string>>("children", value);
            }
        }
        public NodeEntity Entity { 
            get {
                return Vars.Get<NodeEntity>("entity", null);
            }
            set {
                Vars.Set<NodeEntity>("entity", value);
            }
        }
        public Point Point {
            get {
                NodeEntity entity = this.Entity;
                if (entity != null)
                    return entity.Point;

                return null;
            }
        }
        public Transform transform
        {
            get {
                NodeEntity entity = this.Entity;
                if (entity != null)
                    return entity.transform;

                return null;
            }
        }
        public Body Body
        {
            get {
                return Vars.Get<Body>("body", null);
            } 
            set {
                Vars.Set<Body>("body", value);
            }
        }
        
        public bool IsLinked { 
            get {
                return Vars.Get<bool>("is_linked", false);
            }
            set {
                Vars.Set<bool>("is_linked", value);
            }
        }
        public bool IsBuilt { 
            get {
                return Vars.Get<bool>("is_built", false);
            }
            set {
                Vars.Set<bool>("is_built", value);
            }
        }
        public Bag<IActor> Actors
        {
            get {
                Bag<IActor> bagOfActors = Vars.Get<Bag<IActor>>("actors", null);
                if (bagOfActors == null)
                {
                    bagOfActors = new Bag<IActor>();
                    Vars.Set<Bag<IActor>>("actors", bagOfActors);
                }
                return bagOfActors;
            }
            private set
            {
                Vars.Set<Bag<IActor>>("actors", value);
            }
        }
        public Bag<Ticker> Tickers
        {
            get {
                Bag<Ticker> bagOfTickers = Vars.Get<Bag<Ticker>>("tickers", null);
                if (bagOfTickers == null)
                {
                    bagOfTickers = new Bag<Ticker>();
                    Vars.Set<Bag<Ticker>>("tickers", bagOfTickers);
                }
                return bagOfTickers;
            }
            private set {
                Vars.Set<Bag<Ticker>>("tickers", value);
            }
        }
        
        public Node(Map vars)
        {
            Vars = vars;
        }
        public Node(string nodeName, bool isLinkToTree = false)
        {
            // init from just name, so load from json

            // init from json
            string typeName = nodeName.Split('_')[0];
            Map m = JSON.GetResourceMap(nodeName, $"nodes/{typeName}");
            this.VarsSetup = m;

            LOG.Console("new node! " + nodeName);
            
            Vars = new Map();

            Name = nodeName;
            // no parent

            Setup(isLinkToTree);

            if (isLinkToTree)
            {
                // VarsSetup.Log();
            }
        }
        public Node(string nodeName, string parentNodeName, Map varsSetup, bool isLinkToTree = false)
        {
            // init from name, parent, override map
            this.VarsSetup = varsSetup;

            Vars = new Map();
            
            if (parentNodeName != "")
            {
                Name = parentNodeName + "." + nodeName;
                Parent = parentNodeName;
            } else
            {
                Name = nodeName;
                Parent = "";
            }

            LOG.Console($"new node! {nodeName}");

            Setup(isLinkToTree);

            if (isLinkToTree)
            {
                // VarsSetup.Log();
            }
        }

        void Setup(bool isLinkToTree = false)
        {
            InheritSetup();
            ChildrenSetup(isLinkToTree);
            
            if (isLinkToTree)
                LinkToTree();
        }
        void InheritSetup()
        {
            LOG.Console("node inherit setup: " + this.Name);
            string inheritName = VarsSetup.Get<string>("inherit_from", "");
            if (inheritName != "")
            {
                Node inheritNode = new Node(inheritName, false);
                this.InheritFrom(inheritNode);
            }   
        }
        public void InheritFrom(Node otherNode)
        {
            otherNode.VarsSetup.Sync(this.VarsSetup, new Bag<string>("inherit_from"));
            this.VarsSetup = otherNode.VarsSetup;

            // LOG.Console($"node {Name} has been inherited from {otherNode.Name}, with vars:");
            // this.VarsSetup.Log();
        }
        void ChildrenSetup(bool isLinkToTree = false)
        {
            Map childMap = VarsSetup.Get<Map>("children", new Map());
            foreach (string childKey in childMap.Elements.Keys)
            {
                Map childSetupMap = childMap.Get<Map>(childKey, new Map());

                if (this.VarsSetup.Get<bool>("is_pass_layer_on", false))
                {
                    childSetupMap.Set<int>("layer", this.VarsSetup.Get<int>("layer", 0));
                    childSetupMap.Set<bool>("is_pass_layer_on", true);
                }

                string childName = this.Name + "." + childKey;
                
                Node childNode = null;
                if (Children.Contains(childName))
                {
                    if (isLinkToTree)
                    {
                        childNode = NODE.Tree.Get<Node>(childName, null);
                    } else
                    {
                        childNode = new Node(childKey, this.Name, childSetupMap, isLinkToTree);
                    }
                    childNode.SyncSetupMap(childSetupMap);
                } else
                {
                    childNode = new Node(childKey, this.Name, childSetupMap, isLinkToTree);
                    Children.Append(childName);
                }
            } 
        }
        public void SyncSetupMap(Map newSetupMap)
        {
            this.VarsSetup.Sync(newSetupMap);
        }
        void LinkToTree()
        {
            NODE.Tree.Set<Node>(Name, this);

            IsLinked = true;
        }

        public void Terminate()
        {
            LOG.Console($"node terminated! {this.Name}");

            if (Parent != "")
            {
                Node parentNode = NODE.Tree.Get<Node>(Parent);
                parentNode.Children.Remove(this.Name);
            }

            this.Actors = new Bag<IActor>();
            this.Tickers = new Bag<Ticker>();

            // delete children first from tree
            foreach (string child in this.Children)
            {
                Node n = NODE.Tree.Get<Node>(child, null);
                if (n == null)
                    continue;

                n.Terminate();
            }

            if (IsLinked)
            {
                // clears the Node
                NODE.Tree.Remove(this.Name);
            }

            if (IsBuilt)
            {
                Entity.Terminate();
            }
        }

        public string GetName()
        {
            string[] nameParts = Name.Split('.');

            return nameParts[nameParts.Length - 1];
        }
        
        public void Build()
        {
            if (!IsLinked)
                return;

            if (IsBuilt)
                return;
            
            NodeEntity entityOut = null;
            GameObject obj = null;
            bool isPrefab = VarsSetup.Get<bool>("is_prefab", false);
            bool isAssignParent = true;

            if (isPrefab)
            {
                string prefabName = VarsSetup.Get<string>("prefab_name", GetName());
                obj = NODE.NewPrefab(prefabName); 
                obj.name = GetName();
            } else
            {
                if (Parent != "")
                {
                    Node parentNode = NODE.Tree.Get<Node>(Parent, null);
                    bool hasObj = parentNode.Entity.FindChild(GetName(), out obj);
                    if (!hasObj)
                    {
                        obj = NODE.New(GetName());
                    } else
                    {
                        isAssignParent = false;
                    }

                    obj.layer = VarsSetup.Get<int>("layer", 0);

                    //if (parentNode.VarsSetup.Get<bool>("is_pass_layer_on", false))
                    //{
                    //    obj.layer = parentNode.VarsSetup.Get<int>("layer", 0);
                    //    this.VarsSetup.Set<bool>("is_pass_layer_on", true);
                    //    this.VarsSetup.Set<int>("layer", obj.layer);
                    //}

                } else
                {
                    obj = NODE.New(GetName());

                    obj.layer = VarsSetup.Get<int>("layer", 0);
                }
            }
            
            PopulateActors(obj);

            // entity should be last thing we add
            if (Parent != "" && isAssignParent)
            {
                // should set gameobject parent
                Node parentNode = NODE.Tree.Get<Node>(Parent, null);

                obj.transform.SetParent(parentNode.Entity.transform);

                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = new Vector3(1,1,1);
            }

            entityOut = obj.AddComponent<NodeEntity>();

            Entity = entityOut;
            Entity.Link(this.Name, this);

            IsBuilt = true;

            //Entity.Launch();

            BuildChildren();
        }
        void BuildChildren()
        {
            if (!IsLinked)
                return;

            foreach (string childName in Children)
            {
                Node n = NODE.Tree.Get<Node>(childName, null);
                if (n != null)
                    n.Build();
            }
        }

        void PopulateActors(GameObject obj)
        {
            Map actorMap = VarsSetup.Get<Map>("actors", new Map());

            SetupDefaultActors(actorMap);

            foreach (string actorTypeName in actorMap.Elements.Keys)
            {
                Map actorTypeMap = actorMap.Get<Map>(actorTypeName, new Map());
                Type classType = UTIL.GetSnorriType(actorTypeName);

                object o = null;

                if (typeof(Module).IsAssignableFrom(classType))
                {
                    Component component = obj.GetComponent(classType);
                    if (component == null)
                        component = obj.AddComponent(classType);

                    o = component as object;
                } else
                {
                    LOG.Console($"node create instance! {actorTypeName}");
                    o = UTIL.CreateInstance(classType);
                }

                IActor a = o as IActor;

                a.Link(this, actorTypeMap);

                this.Actors.Append(a);

                if (o is Ticker ticker)
                {
                    this.Tickers.Append(ticker);
                }
            }
        }
        void SetupDefaultActors(Map actorMap)
        {
            // point check:
            if (!actorMap.Has("point") && !actorMap.Has("point_layout"))
            {
                actorMap.Set<Map>("point", new Map());
            }
        }

        // runtime:
        public Node AddChild(string nodeName, Map overrideMap)
        {
            if (!Children.Contains(this.Name + "." + nodeName))
            {
                Node childNode = new Node(nodeName, this.Name, overrideMap, IsLinked);

                if (IsBuilt)
                    childNode.Build();

                Children.Append(childNode.Name);

                return childNode;

            } else
            {
                LOG.Console("tried adding node but one already exists: " + nodeName + ", parent: " + this.Name);
            }
            return null;
        }
        public bool FindChild(string name, out Node childNode)
        {
            if (!name.Contains("."))
            {
                name = this.Name + "." + name;
            }

            LOG.Console($"node find child: {name}");

            // searches children and all children
            childNode = null;
            if (Children.Has(name))
            {
                childNode = NODE.Tree.Get<Node>(name);
                if (childNode != null)
                {
                    LOG.Console($"node found child! {name}");
                    return true;
                }
            }
            LOG.Console($"node find could not locate child! {name}");
            return false;
        }
        public bool FindActor(string name, out Actor actor)
        {
            IActor iactor = Actors.Get(name);
            actor = null;
            if (iactor == null)
                return false;

            actor = iactor as Actor;
            return true;
        }
        public bool FindTicker(string name, out Ticker ticker)
        {
            IActor iactor = Actors.Get(name);
            ticker = null;
            if (iactor == null)
                return false;

            ticker = iactor as Ticker;
            return true;
        }
        public bool FindModule(string name, out Module module)
        {
            IActor iactor = Actors.Get(name);
            module = null;
            if (iactor == null)
                return false;

            module = iactor as Module;
            return true;
        }
        public bool FindOperation(string name, out Operation op)
        {
            IActor iactor = Actors.Get(name);
            op = null;
            if (iactor == null)
                return false;

            op = iactor as Operation;
            return true;
        }

        public T GetActor<T>(bool forceAdd = false) where T : IActor
        {
            foreach (IActor actor in this.Actors)
            {
                if (actor is T val)
                {
                    return val;
                }
            }
            if (forceAdd)
            {
                return AddActor<T>();
            }
            return default(T);
        }
        public Bag<T> GetBagOActors<T>()
        {
            Bag<T> bagOut = new Bag<T>();

            foreach (IActor actor in this.Actors)
            {
                if (actor is T val)
                {
                    bagOut.Append(val);
                }
            }

            return bagOut;
        }
        public Bag<T> GetBagOChildActors<T>(bool isLimitedToFirstLevel = false)
        {
            Bag<T> bagOut = GetBagOActors<T>();
            foreach (string childName in Children)
            {
                Node NodeChild = NODE.Tree.Get<Node>(childName, null);
                if (NodeChild != null)
                {
                    if (isLimitedToFirstLevel)
                    {
                        bagOut.AppendBag(NodeChild.GetBagOActors<T>());
                    } else
                    {
                        bagOut.AppendBag(NodeChild.GetBagOChildActors<T>());
                    }
                }
            }
            return bagOut;
        }
        public T AddActor<T>(Map vars = null) where T : IActor
        {
            Type classType = typeof(T);

            object o = null;

            if (typeof(Module).IsAssignableFrom(classType))
            {
                Component component = Entity.gameObject.GetComponent(classType);
                if (component == null)
                    component = Entity.gameObject.AddComponent(classType);

                o = component as object;
            } else
            {
                o = UTIL.CreateInstance(classType);
            }

            IActor a = o as IActor;

            if (vars == null)
                vars = new Map();
                
            a.Link(this, vars);

            a.Inform(ActorState.Setup);
            a.Inform(ActorState.Launch);

            this.Actors.Append(a);

            if (o is Ticker ticker)
            {
                this.Tickers.Append(ticker);
            }

            return (T)o;
        }
        public void AddActor(string actorTypeName)
        {
            Type classType = UTIL.GetSnorriType(actorTypeName);

            object o = null;

            if (typeof(Module).IsAssignableFrom(classType))
            {
                Component component = Entity.gameObject.GetComponent(classType);
                if (component == null)
                    component = Entity.gameObject.AddComponent(classType);

                o = component as object;
            } else
            {
                o = UTIL.CreateInstance(classType);
            }

            IActor a = o as IActor;

            a.Link(this, new Map());

            a.Inform(ActorState.Setup);
            a.Inform(ActorState.Launch);

            this.Actors.Append(a);

            if (o is Ticker ticker)
            {
                this.Tickers.Append(ticker);
            }
        }
        public Coroutine Execute(string routineName, Map args)
        {
            LOG.Console($"node has been executed with name: {routineName}");
            switch (routineName)
            {
                case "delay":
                    return Entity.InvokeTask(args.Get<Task>("task", null),
                        args.Get<float>("time_delay", 0.1f));

                case "wait":
                    return Entity.InvokeWait(args.Get<Task<int, bool>>("task_wait_condition", null),
                        args.Get<Task>("task_when_complete", null));
            }

            return null;
        }
        public Coroutine Execute<T>(string routineName, Map args)
        {
            switch (routineName)
            {
                case "load":
                    return Entity.InvokeLoad<T>(
                        source:args.Get<IEnumerable<T>>("source_collection", null),
                        actionCallback:args.Get<Task<T>>("task_when_callback", null),
                        counter:args.Get<int>("counter", 200),
                        actionOnEnd:args.Get<Task>("task_when_end", null));

                case "lerp":
                    return Entity.InvokeLerp<T>(
                        args.Get<string>("lerp_type", "float"),
                        args.Get<T>("val_in", default(T)),
                        args.Get<T>("val_out", default(T)),
                        args.Get<float>("time", 1f),
                        args.Get<Task<T>>("task_when_callback", null),
                        args.Get<Task>("task_when_end", null));
            }

            return null;
        }

        public void ExecuteOperation(string operationName, bool isVal = true, Task taskWhenComplete = null, params Element[] args)
        {
            if (!IsBuilt)
                return;
            // executes an operation
            bool hasOp = FindOperation(operationName, out Operation op);
            if (hasOp)
            {
                op.Execute(isVal, taskWhenComplete, args);
            }
        }
    }
}