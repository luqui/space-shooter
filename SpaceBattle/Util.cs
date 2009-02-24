using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceBattle
{
    class ActorList
    {
        List<Actor> nonBullets = new List<Actor>();
        List<Bullet> bullets = new List<Bullet>();

        public void Add(Actor actor)
        {
            Bullet b = actor as Bullet;
            if (b != null) { bullets.Add(b); }
            else { nonBullets.Add(actor); }
        }

        public void Update(float dt)
        {
            foreach (var a in new List<Actor>(nonBullets)) { a.Update(dt); }
            foreach (var b in bullets) { b.Update(dt); }

            List<Actor> newActors = new List<Actor>();
            foreach (var a in nonBullets)
            {
                bool hit = false;
                List<Bullet> newBullets = new List<Bullet>();
                foreach (var b in bullets)
                {
                    if (a != b && (b.Position - a.Position).LengthSquared() < 0.25f)
                    {
                        hit = true;
                    }
                    else
                    {
                        newBullets.Add(b);
                    }
                }

                if (!hit) { newActors.Add(a); }
                bullets = newBullets;
            }

            nonBullets = newActors;
        }

        public void Draw()
        {
            foreach (var v in nonBullets) { v.Draw(); }
            foreach (var v in bullets) { v.Draw(); }
        }
    }

    static class Util
    {
        public static GraphicsDevice Device;
        public static SpriteBatch Batch;
        public static ActorList Actors;

        public static void DrawSprite(Texture2D tex, Vector2 pos, float rot, float scale) {
            float sc = 1.0f / tex.Width;
            pos.Y = -pos.Y;
            Batch.Draw(tex, pos, null, Color.White, -rot, new Vector2(tex.Width / 2, tex.Height / 2), sc*scale, SpriteEffects.None, 0);
        }
    }
}
