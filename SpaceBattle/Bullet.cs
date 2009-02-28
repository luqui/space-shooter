using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace SpaceBattle
{
    class Bullet : Actor
    {
        Vector2 position;
        public override Vector2 Position { get { return position; } }
        public override float Radius { get { return 0.25f; } }
        Vector2 velocity;
        public Vector2 Velocity { get { return velocity; } }
        Vector4 color;

        bool dead = false;
        public override bool Dead { get { return dead; } } 

        public Bullet(Vector2 pos, Vector2 vel, Vector4 color)
        {
            position = pos;
            velocity = vel;
            this.color = color;
        }

        public override void Update(float dt)
        {
            position += dt * velocity;
            if (!Util.OnScreen(position)) { dead = true; } 
        }

        public override void Draw()
        {
            float rot = (float)Math.Atan2(velocity.Y, velocity.X);
            Util.DrawSprite(Textures.Bullet, position, rot, 0.75f, color);
        }

        public override void Die() { dead = true; }
    }
}
