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
        LinkedList<Actor>[,] actors;
        int xdim, ydim;
        LinkedList<Actor> backBuffer = new LinkedList<Actor>();
        LinkedList<Actor> bucket = null;
        LinkedList<Ring> rings = new LinkedList<Ring>();

        public ActorList(int xdim_, int ydim_) {
            xdim = xdim_; ydim = ydim_;
            actors = new LinkedList<Actor>[xdim+2, ydim+2];
            for (int x = 0; x < xdim+2; x++) {
                for (int y = 0; y < ydim+2; y++) {
                    actors[x,y] = new LinkedList<Actor>();
                }
            }
        }

        void Coords(Vector2 pos, out int x, out int y) {
            x = (int)(xdim * (pos.X + Util.FIELDWIDTH/2) / Util.FIELDWIDTH)+1;
            y = (int)(ydim * (pos.Y + Util.FIELDHEIGHT/2) / Util.FIELDHEIGHT)+1;
        }

        public void Add(Actor actor)
        {
            backBuffer.AddLast(actor);
            actor.Start();
        }

        public void AddRing(Ring actor)
        {
            rings.AddLast(actor);
        }

        public void RemoveRing(Ring actor)
        {
            rings.Remove(actor);
        }

        float RingStrength(Actor a) {
            float ringstrength = 0;
            foreach (var r in rings)
            {
                if (a == (Actor)r) continue;
                float rad = r.Strength - 2*(r.Position - a.Position).Length();
                if (rad < 0) rad = 0;
                ringstrength += rad;
            }
            return ringstrength;
        }

        void UpdateCell(int x, int y, float dt)
        {
            var node = actors[x, y].First;
            while (node != null)
            {
                node.Value.Update(dt / (1+RingStrength(node.Value)));
                int xp, yp;
                Coords(node.Value.Position, out xp, out yp);
                if (xp != x || yp != y)
                {
                    bucket.AddLast(node.Value);
                    var next = node.Next;
                    actors[x, y].Remove(node);
                    node = next;
                }
                else
                {
                    node = node.Next;
                }
            }
        }

        void Reinsert(Actor a, int x, int y)
        {
            if (x < 1 || x >= xdim + 1 || y < 1 || y >= xdim + 1)
            {
                a.Finish();
            }
            else
            {
                actors[x, y].AddLast(a);
            }
        }

        public void Update(float dt)
        {
            bucket = new LinkedList<Actor>();

            for (int i = 1; i <= xdim; i++)
            {
                for (int j = 1; j <= ydim; j++)
                {
                    UpdateCell(i, j, dt);
                }
            }

            foreach (var k in bucket)
            {
                int x; int y;
                Coords(k.Position, out x, out y);
                Reinsert(k, x, y);
            }
            
            bucket = null;


            for (int x = 1; x <= xdim; x++) 
            {
                for (int y = 1; y <= ydim; y++)
                {
                    foreach (var actor in actors[x, y])
                    {
                        foreach (var other in InternalActorsNear(actor.Position, actor.Radius))
                        {
                            actor.Collision(other);
                        }
                    }
                }
            }

            for (int x = 1; x <= xdim; x++) 
            {
                for (int y = 1; y <= ydim; y++)
                {
                    actors[x,y] = new LinkedList<Actor>(actors[x,y].Where(v =>  {
                        if (v.Dead) { 
                            v.Finish(); 
                            return false; 
                        } else { return true; } }));
                }
            }

            foreach (var k in backBuffer) {
                int x; int y;
                Coords(k.Position, out x, out y);
                Reinsert(k, x, y);
            }

            backBuffer = new LinkedList<Actor>();
        }

        IEnumerable<Actor> InternalActorsNear(Vector2 pos, float radius)
        {
            int x, y;
            Coords(pos, out x, out y);
            if (x < 1 || x >= xdim + 1 || y < 1 || y >= ydim + 1) yield break;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    foreach (var k in actors[x + i, y + j])
                    {
                        if ((k.Position - pos).LengthSquared() <= k.Radius + radius)
                            yield return k;
                    }
                }
            }
        }

        public IEnumerable<Actor> ActorsNear(Vector2 pos, float radius)
        {
            foreach (var k in InternalActorsNear(pos, radius))
            {
                yield return k;
            }
            if (bucket != null)
            {
                foreach (var k in bucket)
                {
                    if ((k.Position - pos).LengthSquared() <= k.Radius + radius)
                        yield return k;
                }
            }
        }

        public void Draw()
        {
            for (int x = 1; x <= xdim; x++)
            {
                for (int y = 1; y <= ydim; y++)
                {
                    foreach (var k in actors[x, y])
                    {
                        k.Draw();
                    }
                }
            }
        }

        public void Reset()
        {
            for (int x = 0; x < xdim + 2; x++)
            {
                for (int y = 0; y < ydim + 2; y++)
                {
                    foreach (var k in actors[x, y]) { k.Finish(); }
                    actors[x, y] = new LinkedList<Actor>();
                }
            }
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

        public static Explosion EXPLOSIONS;

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
            Actors.Add(player1);
            if (Util.MODE == Mode.TwoPlayer) Actors.Add(player2);
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
            EXPLOSIONS.AddExplosion(pos, new Vector3(RandRange(0.5f, 1), RandRange(0.5f, 1), RandRange(0.5f, 1)), 
                       50, RandRange(10,40), RandRange(1,3), RandRange(0.1f, 0.4f));
        }

        public static bool OnScreen(Vector2 pos)
        {
            return pos.X >= -FIELDWIDTH/2 && pos.X <= FIELDWIDTH/2 && pos.Y >= -FIELDHEIGHT/2 && pos.Y <= FIELDHEIGHT/2;
        }

        public static void EnemyDeath(Vector2 pos)
        {
            if (RANDOM.Next(11) == 0)
            {
                int sel = RANDOM.Next(26);
                Vector2 vel = new Vector2(RandRange(-3, 3), RandRange(-3, 3));
                if (sel <= 22)
                {
                    Sequencer.PlayOnce(pos, Sounds.Select(Sounds.Crash));
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
                    Sequencer.PlayOnce(pos, "tri1");
                    Actors.Add(new PowerUp(v => Util.DrawSprite(Textures.RatePowerup, v, 0, 1.0f),
                                       float.PositiveInfinity, pos, vel, ship => ship.FasterShots()));
                }
                else if (sel == 24)
                {
                    Sequencer.PlayOnce(pos, "tri3");
                    Actors.Add(new PowerUp(v => Util.DrawSprite(Textures.NumPowerup, v, 0, 1.0f),
                                       float.PositiveInfinity, pos, vel, ship => ship.MoreShots()));
                }
                else if (sel == 25)
                {
                    Sequencer.PlayOnce(pos, "tri2");
                    Actors.Add(new PowerUp(v => Util.DrawSprite(Textures.RingIcon, v, 0, 1.0f),
                                       float.PositiveInfinity, pos, vel, ship => ship.AddRing()));
                }
            }
        }
    }

}
