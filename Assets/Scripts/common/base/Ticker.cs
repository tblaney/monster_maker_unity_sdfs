using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace snorri
{
    public abstract class Ticker : Actor
    {
        public override void Inform(ActorState state)
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
        
        public virtual void Tick() {}
        public virtual void TickPhysics() {}
        
    }
}
