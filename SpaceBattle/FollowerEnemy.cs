using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SpaceBattle
{
    class FollowerEnemy : Enemy
    {
        Vector2 position;
        Vector2 velocity;
        Actor target;
        bool dead = false;

        public override Vector2 Position { get { return position; } }
        public override float Radius { get { return 0.75f; } }
        public override bool Dead { get { return dead; } }

        public static Texture2D texture;

        public static void LoadContent(ContentManager Content) {
            texture = Content.Load<Texture2D>("FollowerEnemy");
        }

        const float SPEED = 1.0f;

        public FollowerEnemy(Vector2 pos, Actor targ)
        {
            target = targ;
            position = pos;
        }

        public override void Update(float dt)
        {
            velocity = SPEED * (target.Position - position) / (target.Position - position).Length();
            position += dt * velocity;
        }

        public override void Draw()
        {
            Util.DrawSprite(texture, position, 0, 1);
        }

        public override void Collision(Actor other)
        {
            if (other is Bullet) { dead = true; }
        }
    }
}
