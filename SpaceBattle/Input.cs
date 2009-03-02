using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SpaceBattle
{
    abstract class Input
    {
        public abstract Vector2 Direction();
        public abstract Vector2 Aim(Vector2 pos);
        public abstract bool FiringEnemy();
        public abstract bool FiringBullet();
        public abstract bool FireRing();
        public abstract void Update();
    }

    class XBoxInput : Input
    {
        public XBoxInput(PlayerIndex pidx)
        {
            player = pidx;
            state = new GamePadState();
            prevState = new GamePadState();
        }
        PlayerIndex player;
        GamePadState prevState;
        GamePadState state;
        const float TRIGGERLENGTH = 10.0f;

        public override Vector2 Direction()
        {
            return state.ThumbSticks.Left;
        }

        public override Vector2 Aim(Vector2 pos)
        {
            return pos + TRIGGERLENGTH * state.ThumbSticks.Right;
        }

        public override bool FiringEnemy()
        {
            return state.Triggers.Left > 0.5f;
        }

        public override bool FiringBullet()
        {
            return state.Triggers.Right > 0.5f;
        }

        public override bool FireRing()
        {
            return state.Buttons.LeftShoulder == ButtonState.Pressed && prevState.Buttons.LeftShoulder == ButtonState.Released;
        }

        public override void Update()
        {
            prevState = state;
            state = GamePad.GetState(player);
        }
    }
}