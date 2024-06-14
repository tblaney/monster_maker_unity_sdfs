namespace snorri
{
    [UnityEngine.DisallowMultipleComponent]
    public class Point : Module
    {
        protected override void Setup()
        {
            base.Setup();
            
            bool hasVals = Vars.Has("position");
            if (!hasVals)
                hasVals = Vars.Has("rotation");
            if (!hasVals)
                hasVals = Vars.Has("scale");
                
            if (hasVals)
            {
                PointState state = new PointState(Vars);
                this.State = state;
            }
        }
        public PointState State
        {
            get
            {
                return new PointState(this);
            }
            set
            {
                value.Apply(this);
            }
        }
        public Point Parent
        {
            get 
            { 
                UnityEngine.Transform transformParent = this.transform.parent; 
                Point p = transformParent.gameObject.GetComponent<Point>();
                if (p == null)
                    p = transformParent.gameObject.AddComponent<Point>();

                return p;
            }
            set { this.transform.SetParent(value.transform); }
        }

        // publics
        public virtual Vec Position { get { return new Vec(transform.position); } set { transform.position = value.vec3; } }
        public virtual VecInt PositionInt { get { return new VecInt(transform.position); } set { transform.position = value.vec3; } }
        public virtual Vec Rotation { get { return new Vec(transform.rotation); } set { transform.rotation = value.quat; } }
        public virtual Vec Angles { get { return new Vec(transform.eulerAngles); } set { transform.rotation = value.quat; } }
        public virtual Vec Scale { get { return new Vec(transform.localScale); } set { transform.localScale = value.vec3; } }
        public virtual Vec North { get { return new Vec(transform.forward); } }
        public virtual Vec East { get { return new Vec(transform.right); } }
        public virtual Vec West { get { return new Vec(-transform.right); } }
        public virtual Vec South { get { return new Vec(-transform.forward); } }
        public virtual Vec Up { get { return new Vec(transform.up); }}
        public virtual Vec Down { get { return (new Vec(-transform.up)); }}
        public virtual Vec PositionLocal { get { return new Vec(transform.localPosition); } set { transform.localPosition = value.vec3; } }
        public virtual Vec RotationLocal {
            get { return new Vec(transform.localRotation); }
            set { transform.localRotation = value.quat; }
        }

        public void Rotate(float x, float y, float z, bool isWorld = false)
        {
            UnityEngine.Space space = UnityEngine.Space.Self;
            if (isWorld)
            {
                space = UnityEngine.Space.World;
            }
            this.transform.Rotate(x, y, z, space);
        }
    }
}