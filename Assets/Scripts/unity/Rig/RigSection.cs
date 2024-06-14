namespace snorri
{
    using UnityEngine;
    using System.Collections.Generic;

    [System.Serializable]
    public abstract class RigSection : Ticker
    {
        protected Node effectorRoot;
        public Node Root {
            get {return effectorRoot;}
        }
        protected Node effectorTarget;
        public Node Target{
            get { return effectorTarget; }
        }
        protected Node effectorTargetStatic;
        public Node TargetLocal
        {
            get {return effectorTargetStatic;}
        }
        public string Type { get; set; }
        protected override void Setup()
        {
            base.Setup();

            Type = Vars.Get<string>("type", "base");
        }
        protected override void Launch()
        {
            base.Launch();
        }
        public abstract void Build(Map args = null);

        public virtual void Refresh(Map args = null) {}
    }
}