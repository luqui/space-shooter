using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace SpaceBattle
{
    class Bullet
    {
        Vector2 position;
        public Vector2 Position { get { return position; } }
        Vector2 velocity;

        static Texture2D texture;

        public Bullet(Vector2 pos, Vector2 vel)
        {
            position = pos;
            velocity = vel;
        }

        public static void LoadContent(ContentManager Content)
        {
            texture = Content.Load<Texture2D>("Bullet");
        }

        public void Update(float dt)
        {
            position += dt * velocity;
        }

        public void Draw()
        {
            float rot = (float)Math.Atan2(velocity.Y, velocity.X);
            Util.DrawSprite(texture, position, rot, 1);
        }
    }
}
