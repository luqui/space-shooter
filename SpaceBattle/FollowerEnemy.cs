using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SpaceBattle
{
    class FollowerEnemy : Actor
    {
        Vector2 position;
        Vector2 velocity;
        Actor target;

        public override Vector2 Position { get { return position; } }

        public static Texture2D texture;

        public static void LoadContent(ContentManager Content) {
            texture = Content.Load<Texture2D>("FollowerEnemy");
        }

        const float SPEED = 0.5f;

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
    }
}
