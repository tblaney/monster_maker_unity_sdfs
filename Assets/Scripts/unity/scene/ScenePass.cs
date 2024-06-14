using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace snorri
{
    public abstract class ScenePass : Actor
    {
        public Bag<ScenePass> dependentPasses;

        public bool IsComplete {get; set;}
        public bool IsExecuted {get; set;}

        protected SceneTicker ticker;


        protected override void Setup()
        {
            base.Setup();

            ticker = Node.GetActor<SceneTicker>();

            // form dependent passes:
            dependentPasses = new Bag<ScenePass>();
            Bag<string> bagOfPassNames = Vars.Get<Bag<string>>("dependent_passes", new Bag<string>());
            foreach (string name in bagOfPassNames)
            {
                bool hasScenePass = Node.FindActor(name, out Actor scenePassActor);
                if (hasScenePass)
                    dependentPasses.Append(scenePassActor as ScenePass);
            }
        }


        public virtual bool CanRun()
        {
            if (IsComplete)
            {
                return false;
            }

            if (dependentPasses != null && dependentPasses.Length > 0)
            {
                bool isReady = true;
                foreach (ScenePass pass in dependentPasses)
                {
                    if (!pass.IsComplete)
                    {
                        isReady = false;
                        break;
                    }
                }

                return isReady;
            } else
            {
                return true;
            }
        }
        public virtual void Execute() { IsExecuted = true; }
    }
}
