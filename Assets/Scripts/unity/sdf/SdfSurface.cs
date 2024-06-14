using UnityEngine;

namespace snorri
{
    public enum ShapeType
    {
        Sphere = 0,
        Cube = 1,
        Torus = 2,
        Plane = 3,
        Mandelbulb = 4,
        Fbm = 5,
        Cylinder = 6
    }
    public enum BlendMode
    {
        Union = 0,
        SmoothUnion = 1,
        Subtraction = 2,
        SmoothSubtraction = 3,
        UnionSmoothSubtraction = 4,
        Intersection = 5,
        SmoothIntersection = 6,
    }

    public class SdfSurface : Actor
    {
        /*
        public Vector3 Offset
        {
            get {
                Bag<float> offsets = Vars.Get<Bag<float>>("offset", new Bag<float>(1f,1f,1f));
                return new Vector3(offsets[0],offsets[1],offsets[2]);
            }
        }
        */
        private static readonly bool IsColliders = true;

        public Vector3 Position
        {
            get {
                return Node.transform.position;
            }
        }
        public Vector3 Rotation
        {
            get
            {
                //if (ShapeType != ShapeType.Cube)
                //{
                    return new Vector3(Node.transform.rotation.eulerAngles.x * Mathf.Deg2Rad,
                        Node.transform.rotation.eulerAngles.y * Mathf.Deg2Rad, Node.transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
                //}

                //return new Vector3(Node.transform.localEulerAngles.x * Mathf.Deg2Rad,
                //        Node.transform.localEulerAngles.y * Mathf.Deg2Rad, Node.transform.localEulerAngles.z * Mathf.Deg2Rad);
            }
        }

        public Vector3 Scale { 
            get { 
                Bag<float> scale = Vars.Get<Bag<float>>("scale", new Bag<float>(0.1f,0.1f,0.1f)); 
                return new Vector3(scale[0], scale[1], scale[2]);
            } 
            set {
                Vars.Set<Bag<float>>("scale", new Bag<float>(value.x, value.y, value.z));
            }
        }
        public float Radius { get { return Vars.Get<float>("radius", 1f); } 
            set {
                Vars.Set<float>("radius", value);
            }
        }

        public ShapeType ShapeType {get; set;}
        public BlendMode BlendMode {get; set;}
        public float BlendStrength {get; set;}

        public Color Diffuse {get; set;}

        public float GrowthFactor {get; set;}

        public void SetScale(Vector3 scaleIn)
        {
            Bag<float> scale = Vars.Get<Bag<float>>("scale", new Bag<float>(0.1f,0.1f,0.1f));
            scale[0] = scaleIn.x;
            scale[1] = scaleIn.y;
            scale[2] = scaleIn.z;
            Vars.Set<Bag<float>>("scale", scale);
        }

        protected override void Setup()
        {
            base.Setup();

            GrowthFactor = 1f;

            Diffuse = UTIL.GetColorFromHex(Vars.Get<string>("color"));
            BlendStrength = Vars.Get<float>("blend", 0f);

            switch (Vars.Get<string>("shape_type", "sphere"))
            {
                case "sphere":
                    ShapeType = ShapeType.Sphere;
                    break;
                case "cube":
                    ShapeType = ShapeType.Cube;
                    break;
                case "torus":
                    ShapeType = ShapeType.Torus;
                    break;
                case "plane":
                    ShapeType = ShapeType.Plane;
                    break;
                case "mandelbulb":
                    ShapeType = ShapeType.Mandelbulb;
                    break;
                case "fbm":
                    ShapeType = ShapeType.Fbm;
                    break;
                case "cylinder":
                    ShapeType = ShapeType.Cylinder;
                    break;
            }

            switch (Vars.Get<string>("blend_mode", "union"))
            {
                case "union":
                    BlendMode = BlendMode.Union;
                    break;
                case "union_smooth":
                    BlendMode = BlendMode.SmoothUnion;
                    break;
            }

            if (!IsColliders)
                return;

            bool hasCollider = Vars.Get<bool>("has_collider", false);
            if (hasCollider)
                InitCollider();
        }

        Node colliderNode;
        public void RefreshCollider()
        {
            if (!IsColliders)
                return;
                
            bool hasCollider = Vars.Get<bool>("has_collider", false);
            if (!hasCollider)
                return;

            ColliderModule module = colliderNode.GetActor<ColliderModule>();

            if (module == null)
                return;
                
            Map args = new Map();
            bool isValid = false;

            switch (Vars.Get<string>("shape_type", "sphere"))
            {
                case "sphere":
                    args.Set<Bag<float>>("center",
                        new Bag<float>(0f,0f,0f)
                    );
                    //LOG.Console($"sdf surface collider radius: {this.Radius}");
                    args.Set<float>("radius",
                        this.Radius
                    );
                    isValid = true;
                    break;
                case "cube":
                    args.Set<Bag<float>>("center",
                        new Bag<float>(0f,0f,0f)
                    );
                    Vector3 scale = this.Scale;
                    args.Set<Bag<float>>("size",
                        new Bag<float>(scale.x, scale.y, scale.z)
                    );
                    isValid = true;
                    break;
                case "cylinder":
                    args.Set<Bag<float>>("center",
                        new Bag<float>(0f,0f,0f)
                    );
                    args.Set<float>("radius",
                        this.Radius
                    );
                    args.Set<float>("height",
                        this.Scale.y
                    );
                    isValid = true;
                    break;
            }
            
            if (isValid)
            {
                module.Refresh(args);
            }
        }

        void InitCollider()
        {
            Map args = new Map();
            bool isValid = false;

            switch (Vars.Get<string>("shape_type", "sphere"))
            {
                case "sphere":
                    args.Set<string>("inherit_from", "collider_sphere");
                    args.Set<Bag<float>>("actors:sphere_collider_module:center",
                        new Bag<float>(0f,0f,0f)
                    );
                    //LOG.Console($"sdf surface collider radius: {this.Radius}");
                    args.Set<float>("actors:sphere_collider_module:radius",
                        this.Radius
                    );
                    isValid = true;
                    break;
                case "cube":
                    args.Set<string>("inherit_from", "collider_cube");
                    args.Set<Bag<float>>("actors:box_collider_module:center",
                        new Bag<float>(0f,0f,0f)
                    );
                    Vector3 scale = this.Scale;
                    args.Set<Bag<float>>("actors:sphere_collider_module:size",
                        new Bag<float>(scale.x, scale.y, scale.z)
                    );
                    isValid = true;
                    break;
                case "cylinder":
                    args.Set<string>("inherit_from", "collider_capsule");
                    args.Set<Bag<float>>("actors:capsule_collider_module:center",
                        new Bag<float>(0f,0f,0f)
                    );
                    args.Set<float>("actors:sphere_collider_module:radius",
                        this.Radius
                    );
                    args.Set<float>("actors:sphere_collider_module:height",
                        this.Scale.y
                    );
                    isValid = true;
                    break;
            }

            if (isValid)
            {
                args.Set<int>("layer", this.Node.transform.gameObject.layer);
                colliderNode = this.Node.AddChild("collider", args);
            } else
            {
                colliderNode = null;
            }
        }
    }
}