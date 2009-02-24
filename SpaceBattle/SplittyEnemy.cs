using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SpaceBattle
{
    class SplittyEnemy : Enemy
    {
        Vector2 position;
        Vector2 velocity;
        bool dead = false;
        float timer = 0.0f;
        bool teeny = false;

        public override Vector2 Position { get { return position; } }
        public override float Radius { get { return teeny ? 0.25f : 0.75f; } }
        public override bool Dead { get { return dead; } }

        public static Texture2D texture;

        public static void LoadContent(ContentManager Content) {
            texture = Content.Load<Texture2D>("SplittyEnemy");
        }

        float SPEED = 3.0f;
        float RATE = 3.0f;

        public SplittyEnemy(Vector2 pos)
        {
            position = pos;
        }

        public override void Update(float dt)
        {
            timer += RATE * dt;
            velocity = SPEED * new Vector2((float)Math.Cos(timer), (float)Math.Sin(timer));
            if (teeny) { velocity *= 0.5f; }
            position += dt * velocity;
        }

        public override void Draw()
        {
            Util.DrawSprite(texture, position, 0, teeny ? 0.3f : 1.0f);
        }

        public override void Collision(Actor other)
        {
            if (other is Bullet)
            {
                if (!teeny)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float theta = (float)(2 * Math.PI * i / 3);
                        SplittyEnemy sub = new SplittyEnemy(position + 1.25f*new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)));
                        sub.teeny = true;
                        sub.RATE = 2 * RATE;
                        sub.SPEED = 2 * SPEED;
                        sub.timer += i / RATE;
                        Util.Actors.Add(sub);
                    }
                }
                dead = true;
            }
        }
    }
}
