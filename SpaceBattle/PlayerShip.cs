using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpaceBattle
{
    class PlayerShip : Actor
    {
        Vector2 position;
        public override Vector2 Position { get { return position; } }
        Vector2 velocity;

        PlayerIndex player;
        Texture2D texture;
        Texture2D crosshair;

        float bulletTimeout = 0.0f;
        bool triggering = false;

        const float BULLETTIME = 0.1f;
        const float SPEED = 3.0f;
        const float BULLETSPEED = 20.0f;
        const float TRIGGERLENGTH = 10.0f;

        public PlayerShip(PlayerIndex player) { this.player = player; }

        public void LoadContent(ContentManager Content)
        {
            if (player == PlayerIndex.One)
            {
                texture = Content.Load<Texture2D>("RedShip");
                crosshair = Content.Load<Texture2D>("RedCrosshair");
            }
            else if (player == PlayerIndex.Two)
            {
                texture = Content.Load<Texture2D>("BlueShip");
                crosshair = Content.Load<Texture2D>("BlueCrosshair");
            }
        }

        public override void Update(float dt)
        {
            velocity = SPEED * GamePad.GetState(player).ThumbSticks.Left;
            position += dt * velocity;
            
            bool wasTriggering = triggering;
            triggering = GamePad.GetState(player).Triggers.Right > 0.5f;

            bulletTimeout -= dt;
            if (bulletTimeout <= 0) {
                Vector2 dir = GamePad.GetState(player).ThumbSticks.Right;
                if (!triggering && dir.LengthSquared() > 0.125f)
                {
                    if (wasTriggering) {
                        Util.Actors.Add(new FollowerEnemy(position + TRIGGERLENGTH * dir, this));
                    }
                    else if (GamePad.GetState(player).Buttons.RightShoulder == ButtonState.Pressed) {
                        Util.Actors.Add(new Bullet(position + dir, BULLETSPEED * dir));
                    }
                    bulletTimeout = BULLETTIME;
                }
            }

        }

        public override void Draw()
        {
            float rot = (float)Math.Atan2(velocity.Y, velocity.X);
            Util.DrawSprite(texture, position, rot - (float)Math.PI/2, 1);

            if (triggering)
            {
                Vector2 dir = GamePad.GetState(player).ThumbSticks.Right;
                Util.DrawSprite(crosshair, position + TRIGGERLENGTH * dir, 0, 0.5f);
            }
        }
    }
}
