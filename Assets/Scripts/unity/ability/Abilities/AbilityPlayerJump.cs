using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace snorri
{
    public class AbilityPlayerJump : AbilityPlayer
    {   
        float jumpHeight = 1.5f;
        float maxHeight;

        protected override void Setup()
        {
            base.Setup();

            jumpHeight = Vars.Get<float>("jump_height", 1.5f);
        }

        public override bool CanRun()
        {
            return body.IsGrounded & input.IsClicked("jump");
        }

        public override void Begin()
        {
            base.Begin();

            LOG.Console("ability player jump begin!");

            Vec velocity = Node.Body.Velocity;
            velocity.y = UTIL.Sqrt(jumpHeight * -2f * UnityEngine.Physics.gravity.y);
            Node.Body.Velocity = velocity;

            maxHeight = -100f;
        }

        public override void TickPhysicsAbility()
        {
            base.TickPhysicsAbility();

            if ((body.IsGrounded && ReachedMaxHeight()))
            {    
                Stop();
                return;
            }

            Vec input = GAME.Vars.Get<Vec>("input:direction", new Vec());
            Vec moveDir = Node.Body.GetWorldDirectionFromInput(input, this.camera.Node.Point);

            Map args = new Map();
            Node.Body.Rotate(moveDir, args);
            //Node.Point.Rotation = Node.Point.Rotation.Lerp(new Vec(Quaternion.LookRotation(moveDir.vec3, Vector3.up)), TIME.DeltaPhysics*12f);

        }
        public bool ReachedMaxHeight()
        {
            if (Node.Point.Position.y >= maxHeight)
            {
                maxHeight = Node.Point.Position.y;
                return false;
            } else
            {
                return true;
            }
        }
        protected override void Stop()
        {
            base.Stop();

            LOG.Console("ability player jump stop!");
        }
    }
}
