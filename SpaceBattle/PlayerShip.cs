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
    class PlayerShip
    {
        Vector2 position;
        public Vector2 Position { get { return position; } }
        Vector2 velocity;

        PlayerIndex player;
        Texture2D texture;

        float bulletTimeout = 0.0f;

        const float BULLETTIME = 0.1f;
        const float SPEED = 3.0f;
        const float BULLETSPEED = 20.0f;

        public PlayerShip(PlayerIndex player) { this.player = player; }

        public void LoadContent(ContentManager Content)
        {
            if (player == PlayerIndex.One)
            {
                texture = Content.Load<Texture2D>("RedShip");
            }
            else if (player == PlayerIndex.Two)
            {
                texture = Content.Load<Texture2D>("BlueShip");
            }
        }

        public void Update(float dt)
        {
            velocity = SPEED * GamePad.GetState(player).ThumbSticks.Left;
            position += dt * velocity;

            bulletTimeout -= dt;
            if (bulletTimeout <= 0) {
                Vector2 dir = GamePad.GetState(player).ThumbSticks.Right;
                if (dir.LengthSquared() > 0.125f) {
                    Util.Bullets.Add(new Bullet(position + dir, BULLETSPEED * dir));
                    bulletTimeout = BULLETTIME;
                }
            }
        }

        public void Draw()
        {
            float rot = (float)Math.Atan2(velocity.Y, velocity.X);
            Util.DrawSprite(texture, position, rot - (float)Math.PI/2, 1);
        }
    }
}
