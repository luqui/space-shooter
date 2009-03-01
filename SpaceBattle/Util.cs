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
        List<Actor> actors = new List<Actor>();
        List<Actor> backBuffer = new List<Actor>();
        List<Actor> rings = new List<Actor>();

        List<Explosion> explosions = new List<Explosion>();

        public void Add(Actor actor)
        {
            backBuffer.Add(actor);
            actor.Start();
        }

        public void Add(Explosion ex)
        {
            explosions.Add(ex);
        }

        public void AddRing(Actor actor)
        {
            rings.Add(actor);
        }

        public void RemoveRing(Actor actor)
        {
            rings.Remove(actor);
        }

        float RingStrength(Actor a) {
            float ringstrength = 0;
            foreach (var r in rings)
            {
                if (a == r) continue;
                ringstrength += ((r as Enemy).Behavior as RingBehavior).Strength / ((a.Position - r.Position).LengthSquared() + 0.01f);
            }
            return ringstrength;
        }

        public void Update(float dt)
        {
            if (Util.MODE == Util.Mode.OnePlayer)
            {
                float playerscale = 1 + RingStrength(Util.player1)/16;
                foreach (var a in actors)
                {
                    float strength = 1 + RingStrength(a)/16;
                    a.Update(dt * strength / playerscale);
                }
            }
            else {
                foreach (var a in actors)
                {
                    a.Update(dt / (1 + RingStrength(a)));
                }
            }

            for (int i = 0; i < actors.Count; i++) {
                for (int j = i+1; j < actors.Count; j++) { 
                    var a = actors[i];
                    var b = actors[j];
                    if ((a.Position - b.Position).Length() < a.Radius + b.Radius) {
                        a.Collision(b);
                        b.Collision(a);
                    }
                }
            }

            actors.RemoveAll(v =>
            {
                if (v.Dead || !Util.OnScreen(v.Position))
                {
                    v.Finish();
                    return true;
                }
                else
                {
                    return false;
                }
            });

            actors.AddRange(backBuffer);
            backBuffer = new List<Actor>();

            foreach (var e in explosions) { e.Update(dt); }
            explosions.RemoveAll(v => v.Done);
        }

        public IEnumerable<Actor> ActorsNear(Vector2 pos, float radius)
        {
            // slooow, need quadtree
            return actors.Where(a => (a.Position - pos).LengthSquared() < radius * radius);
        }

        public void Draw()
        {
            foreach (var v in actors) { v.Draw(); }
            foreach (var e in explosions) { e.Draw(); }
        }

        public void Reset()
        {
            actors.RemoveAll(v =>
            {
                if (!(v is PlayerShip))
                {
                    v.Finish();
                    return true;
                }
                else
                {
                    return false;
                }
            });
        }
    }

    static class Util
    {
        public static GraphicsDevice Device;
        public static SpriteBatch Batch;
        public static ActorList Actors;
        public static Sequencer Sequencer;
        public static Scheduler Scheduler;

        public static PlayerShip player1;
        public static PlayerShip player2;

        public const float FIELDWIDTH = 28.0f;
        public const float FIELDHEIGHT = 21.0f;

        public enum Mode { OnePlayer, TwoPlayer };
        public static Mode MODE = Mode.OnePlayer;

        public static int SCORE = 0;
        public static int BLANKS = 0;

        public static Random RANDOM = new Random();

        public static Vector2 RandomPosition()
        {
            return new Vector2(Scale(-FIELDWIDTH / 2, FIELDWIDTH / 2, (float)RANDOM.NextDouble()),
                               Scale(-FIELDHEIGHT / 2, FIELDHEIGHT / 2, (float)RANDOM.NextDouble()));
        }

        public static float RandRange(float min, float max)
        {
            return Scale(min, max, (float)RANDOM.NextDouble());
        }

        public static void Reset()
        {
            Actors.Reset();
        }

        public static float Scale(float min, float max, float x)
        {
            return x * (max - min) + min;
        }

        public static PlayerIndex OtherPlayer(PlayerIndex p)
        {
            return p == PlayerIndex.One ? PlayerIndex.Two : PlayerIndex.One;
        }

        public static PlayerShip GetPlayer(PlayerIndex p)
        {
            return p == PlayerIndex.One ? player1 : player2;
        }

        public static float AlphaHack = 1.0f;

        public static void DrawSprite(Texture2D tex, Vector2 pos, float rot, float scale, Vector4 color)
        {
            float sc = 1.0f / tex.Width;
            pos.Y = -pos.Y;
            color.W *= AlphaHack;
            Batch.Draw(tex, pos, null, new Color(color), -rot, new Vector2(tex.Width / 2, tex.Height / 2), sc * scale, SpriteEffects.None, 0);
        }

        public static void DrawSprite(Texture2D tex, Vector2 pos, float rot, float scale)
        {
            DrawSprite(tex, pos, rot, scale, new Vector4(1, 1, 1, 1));
        }

        public static void DrawText(Vector2 pos, string text)
        {
            pos.Y = -pos.Y;
            Batch.DrawString(Textures.Font, text, pos, Color.White, 0, new Vector2(12,12), 1.0f/24, SpriteEffects.None, 0);
        }

        public static void RandomExplosion(Vector2 pos)
        {
            Actors.Add(new Explosion(pos, new Vector3(RandRange(0.5f, 1), RandRange(0.5f, 1), RandRange(0.5f, 1)), 
                       50, RandRange(10,40), RandRange(1,3), RandRange(0.1f, 0.4f)));
        }

        public static bool OnScreen(Vector2 pos)
        {
            return pos.X >= -FIELDWIDTH/2 && pos.X <= FIELDWIDTH/2 && pos.Y >= -FIELDHEIGHT/2 && pos.Y <= FIELDHEIGHT/2;
        }

        public static void EnemyDeath(Vector2 pos)
        {
            if (RANDOM.Next(11) == 0)
            {
                int sel = RANDOM.Next(25);
                Vector2 vel = new Vector2(RandRange(-3, 3), RandRange(-3, 3));
                if (sel <= 22)
                {
                    Sequencer.PlayOnce(Sounds.Select(Sounds.Crash));
                    if (MODE == Mode.OnePlayer)
                    {
                        Actors.Add(PowerUps.RandomPowerup(player1.Position, new Vector2()));
                    }
                    else
                    {
                        Actors.Add(PowerUps.RandomPowerup(pos, vel));
                    }
                }
                else if (sel == 23)
                {
                    Sequencer.PlayOnce("tri1");
                    Actors.Add(new PowerUp(v => Util.DrawSprite(Textures.RatePowerup, v, 0, 1.0f),
                                       pos, vel, ship => ship.FasterShots()));
                }
                else if (sel == 24)
                {
                    Sequencer.PlayOnce("tri3");
                    Actors.Add(new PowerUp(v => Util.DrawSprite(Textures.NumPowerup, v, 0, 1.0f),
                                       pos, vel, ship => ship.MoreShots()));
                }
            }
        }
    }

}
