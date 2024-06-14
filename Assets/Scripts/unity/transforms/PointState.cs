using Newtonsoft.Json;
namespace snorri
{
    [System.Serializable]
    public class PointState : Element
    {
        public Map Vars {get; set;}

        public PointState()
        {

        }
        public PointState(Map vars)
        {
            Name = "point_state";

            this.Vars = new Map();

            Vars.Set<Vec>("position", new Vec(vars.Get<Bag<float>>("position", new Bag<float>(0,0,0))));
            Vars.Set<Vec>("rotation", new Vec(vars.Get<Bag<float>>("rotation", new Bag<float>(0,0,0))));
            Vars.Set<Vec>("scale", new Vec(vars.Get<Bag<float>>("scale", new Bag<float>(1,1,1))));

            Vars.Set<bool>("is_local", vars.Get<bool>("is_local", false));
        }
        public PointState(Point p, bool isLocal = false)
        {
            Name = "point_state";

            Vars = new Map();

            Vars.Set<Vec>("position", new Vec(p.transform.position));
            Vars.Set<Vec>("rotation", new Vec(p.transform.rotation));
            Vars.Set<Vec>("scale", new Vec(p.transform.localScale));

            Vars.Set<bool>("is_local", isLocal);
        }
        public PointState(PointLayout pL)
        {
            Name = "point_state";
            Vars = new Map();

            Vars.Set<Vec>("position", new Vec(pL.rect.anchoredPosition));
            Vars.Set<Vec>("rotation", new Vec(pL.rect.rotation));
            Vars.Set<Vec>("scale", new Vec(pL.transform.localScale));
        }
        public PointState(PointState state)
        {
            this.Name = "point_state";

            Vars = new Map();
            Vars.Set<Vec>("position", state.Position);
            Vars.Set<Vec>("rotation", state.Rotation);
            Vars.Set<Vec>("scale", state.Scale);
        }

        public PointState Lerp(PointState state_out, float time)
        {
            PointState state = new PointState(this);
            state.Position = state.Position.Lerp(state_out.Position, time);
            state.Rotation = state.Rotation.Lerp(state_out.Rotation, time, true);
            state.Scale = state.Scale.Lerp(state_out.Scale, time);
            return state;
        }

        public Vec Position
        {
            get { return Vars.Get<Vec>("position", Vec.zero); }
            set { Vars.Set<Vec>("position", value); }
        }
        public Vec Rotation
        {
            get { return Vars.Get<Vec>("rotation", Vec.zero); }
            set { Vars.Set<Vec>("rotation", value); }
        }
        public Vec Scale
        {
            get { return Vars.Get<Vec>("scale", Vec.ones); }
            set { Vars.Set<Vec>("scale", value); }
        }

        public void Apply(Point p)
        {
            bool isLocal = Vars.Get<bool>("is_local", false);
            if (p is PointLayout pL)
            {
                pL.rect.anchoredPosition = Position.vec2;
                if (isLocal)
                    p.transform.localRotation = Rotation.quat;
                else
                    p.transform.rotation = Rotation.quat;
                p.transform.localScale = Scale.vec3;
            } else
            {
                if (isLocal)
                {
                    p.transform.localPosition = Position.vec3;
                    p.transform.localRotation = Rotation.quat;
                    p.transform.localScale = Scale.vec3;
                } else
                {
                    p.transform.position = Position.vec3;
                    p.transform.rotation = Rotation.quat;
                    p.transform.localScale = Scale.vec3;
                }
            }
        }
    }
}