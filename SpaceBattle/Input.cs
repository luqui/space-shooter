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
        public abstract float BulletWeight();
        public abstract bool FireRing();
        public abstract bool SwitchBehaviors();
        public abstract bool SwitchSeekers();
        public abstract bool SwitchDamages();
        public abstract bool Back();
        public abstract void Update();
        public abstract bool Pause();
    }

    class XBoxInput : Input
    {
        public XBoxInput(PlayerIndex pidx)
        {
            Player = pidx;
            state = new GamePadState();
            prevState = new GamePadState();
        }
        public readonly PlayerIndex Player;
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
            return state.Triggers.Right > 0.25f;
        }

        public override float BulletWeight()
        {
            return 4.0f/3 * (state.Triggers.Right - 0.25f);
        }

        public override bool FireRing()
        {
            return state.Buttons.LeftShoulder == ButtonState.Pressed && prevState.Buttons.LeftShoulder == ButtonState.Released
                || state.Buttons.RightShoulder == ButtonState.Pressed && prevState.Buttons.RightShoulder == ButtonState.Released;
        }

        public override void Update()
        {
            prevState = state;
            state = GamePad.GetState(Player);
        }

        public override bool SwitchBehaviors()
        {
            return state.Buttons.Y == ButtonState.Pressed && prevState.Buttons.Y == ButtonState.Released;
        }

        public override bool SwitchSeekers()
        {
            return state.Buttons.X == ButtonState.Pressed && prevState.Buttons.X == ButtonState.Released;
        }

        public override bool SwitchDamages()
        {
            return state.Buttons.A == ButtonState.Pressed && prevState.Buttons.A == ButtonState.Released;
        }

        public override bool Back()
        {
            return state.Buttons.Back == ButtonState.Pressed && prevState.Buttons.Back == ButtonState.Released;
        }

        public override bool Pause()
        {
            return state.Buttons.Start == ButtonState.Pressed && prevState.Buttons.Start == ButtonState.Released;
        }
    }

    class MouseKBInput : Input
    {
        KeyboardState kState;
        KeyboardState lastKState;
        MouseState mState;
        MouseState lastMState;

        public override Vector2 Direction()
        {
            Func<Keys,float> sc = k => kState.IsKeyDown(k) ? 1 : 0;
            Vector2 r = sc(Keys.W) * new Vector2(0, 1)
                      + sc(Keys.S) * new Vector2(0, -1)
                      + sc(Keys.A) * new Vector2(-1, 0)
                      + sc(Keys.D) * new Vector2(1, 0);
            if (r.LengthSquared() > 0) r.Normalize();
            return r;
        }

        public override Vector2 Aim(Vector2 pos)
        {
            Vector3 scr = new Vector3(mState.X, mState.Y, 0);
            Vector3 world = Vector3.Transform(scr, Matrix.Invert(Util.WorldToScreen));
            return new Vector2(world.X, -world.Y);
        }

        public override bool FiringEnemy()
        {
            return mState.RightButton == ButtonState.Pressed;
        }

        public override bool FiringBullet()
        {
            return mState.LeftButton == ButtonState.Pressed;
        }

        public override float BulletWeight()
        {
            return 0.5f;
        }

        public override bool FireRing()
        {
            return kState.IsKeyDown(Keys.Space) && lastKState.IsKeyUp(Keys.Space);
        }

        public override bool SwitchBehaviors()
        {
            return kState.IsKeyDown(Keys.D1) && lastKState.IsKeyUp(Keys.D1);
        }

        public override bool SwitchSeekers()
        {
            return kState.IsKeyDown(Keys.D2) && lastKState.IsKeyUp(Keys.D2);
        }

        public override bool SwitchDamages()
        {
            return kState.IsKeyDown(Keys.D3) && lastKState.IsKeyUp(Keys.D3);
        }

        public override bool Back()
        {
            return kState.IsKeyDown(Keys.Back) && lastKState.IsKeyUp(Keys.Back);
        }

        public override void Update()
        {
            lastMState = mState;
            mState = Mouse.GetState();
            lastKState = kState;
            kState = Keyboard.GetState();
        }

        public override bool Pause()
        {
            return kState.IsKeyDown(Keys.Pause) && lastKState.IsKeyUp(Keys.Pause);
        }
    }
}