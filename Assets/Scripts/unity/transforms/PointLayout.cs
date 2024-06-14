using UnityEngine;

namespace snorri
{
    [UnityEngine.DisallowMultipleComponent]
    public class PointLayout : Point
    {
        public RectTransform rect;

        protected override void AddClasses()
        {
            rect = ComponentCheck<RectTransform>();
        }

        protected override void Setup()
        {
            base.Setup();

            Configure();
        }

        void Configure()
        {
            Bag<float> pivotPoint = Vars.Get<Bag<float>>("pivot", new Bag<float>(0f,0f));

            if (Vars.Has("anchor_min") && Vars.Has("anchor_max"))
            {
                Bag<float> anchorPointMin = Vars.Get<Bag<float>>("anchor_min", new Bag<float>(0f,0f));
                Bag<float> anchorPointMax = Vars.Get<Bag<float>>("anchor_max", new Bag<float>(0f,0f));

                AnchorMin = new Vec(anchorPointMin);
                AnchorMax = new Vec(anchorPointMax);

                rect.offsetMin = Vector2.zero; // This sets left and bottom offsets to 0
                rect.offsetMax = Vector2.zero; // This sets right and top offsets to 0
            } else
            {
                Bag<float> anchorPoint = Vars.Get<Bag<float>>("anchor", new Bag<float>(0f,0f));
                Anchor = new Vec(anchorPoint);
            }

            Pivot = new Vec(pivotPoint);
        }

        public Vec Pivot {
            get { return new Vec(rect.pivot); }
            set {
                rect.pivot = value.vec2;
            }
        }
        public Vec Anchor {
            get { return new Vec(rect.anchorMin); }
            set {
                rect.anchorMin = value.vec2;
                rect.anchorMax = value.vec2;
            }
        }
        public Vec AnchorMin {
            get { return new Vec(rect.anchorMin); }
            set {
                rect.anchorMin = value.vec2;
            }
        }
        public Vec AnchorMax {
            get { return new Vec(rect.anchorMax); }
            set {
                rect.anchorMax = value.vec2;
            }
        }

        // publics
        public override Vec Position { get { return new Vec(rect.anchoredPosition); } set { rect.anchoredPosition = value.vec2; } }
        public override VecInt PositionInt { get { return new VecInt(rect.anchoredPosition); } set { rect.anchoredPosition = value.vec2; } }
    }
}