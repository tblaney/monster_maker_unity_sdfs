using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace snorri
{
    public class LayoutCanvasGroupModule : Module
    {
        CanvasGroup canvasGroup;

        protected override void AddClasses()
        {
            canvasGroup = ComponentCheck<CanvasGroup>();
        }
        protected override void Setup()
        {
            base.Setup();

            Configure();
        }

        void Configure()
        {
            float alpha = Vars.Get<float>("opacity", 1f);
            canvasGroup.alpha = alpha;
        }

        public float Opacity {
            get { return canvasGroup.alpha; }
            set { canvasGroup.alpha = value; }
        }
    }
}