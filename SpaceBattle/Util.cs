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
        public readonly int XDim, YDim;
        LinkedList<Actor> backBuffer = new LinkedList<Actor>();
        LinkedList<Actor> bucket = null;
        LinkedList<Ring> rings = new LinkedList<Ring>();
        public bool Reject = false;

        public ActorList(int xdim_, int ydim_) {
            XDim = xdim_; YDim = ydim_;
            actors = new LinkedList<Actor>[XDim+2, YDim+2];
            for (int x = 0; x < XDim+2; x++) {
                for (int y = 0; y < YDim+2; y++) {
                    actors[x,y] = new LinkedList<Actor>();
                }
            }
        }

        void Coords(Vector2 pos, out int x, out int y) {
            x = (int)(XDim * (pos.X + Util.FIELDWIDTH/2) / Util.FIELDWIDTH)+1;
            y = (int)(YDim * (pos.Y + Util.FIELDHEIGHT/2) / Util.FIELDHEIGHT)+1;
        }

        public void Add(Actor actor)
        {
            backBuffer.AddLast(actor);
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
            if (x < 1 || x >= XDim + 1 || y < 1 || y >= YDim + 1)
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

            for (int i = 1; i <= XDim; i++)
            {
                for (int j = 1; j <= YDim; j++)
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


            for (int x = 1; x <= XDim; x++) 
            {
                for (int y = 1; y <= YDim; y++)
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

            if (Reject) return;

            for (int x = 1; x <= XDim; x++) 
            {
                for (int y = 1; y <= YDim; y++)
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
                k.Start();
                Reinsert(k, x, y);
            }

            backBuffer = new LinkedList<Actor>();
        }

        IEnumerable<Actor> InternalActorsNear(Vector2 pos, float radius)
        {
            int x, y;
            Coords(pos, out x, out y);
            if (x < 1 || x >= XDim + 1 || y < 1 || y >= YDim + 1) yield break;
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

        public void Finish()
        {
            for (int x = 1; x < XDim+1; x++)
            {
                for (int y = 1; y < YDim+1; y++)
                {
                    foreach (var k in actors[x, y])
                    {
                        k.Finish();
                    }
                }
            }
        }

        public void Draw()
        {
            for (int x = 1; x <= XDim; x++)
            {
                for (int y = 1; y <= YDim; y++)
                {
                    foreach (var k in actors[x, y])
                    {
                        k.Draw();
                    }
                }
            }
        }
    }

    public class Pair<T,U> {
        public Pair(T t, U u) { Fst = t; Snd = u; }
        public readonly T Fst;
        public readonly U Snd;
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

        public static Explosion Explosions;
        public static int DeathCount = 0;

        public const float FIELDWIDTH = 28.0f;
        public const float FIELDHEIGHT = 21.0f;
        public static Matrix WorldToScreen;

        public static void LoadSinglePlayerGame()
        {
            Util.player1.PositionSetter(new Vector2(-FIELDWIDTH/4,0));
            for (int ix = 0; ix < 10; ix++)
            {
                var enemy = new Enemy(new Vector2(0, (float)Math.Sin(ix-5 / 10) * FIELDHEIGHT / 2), Util.player1, null, null, null) { dtUpgrade = 0.0f }; 
                Actors.Add(enemy);
            }
        }
        public static void LoadMultiPlayerGame()
        {
            Util.player1.PositionSetter(new Vector2(-FIELDWIDTH/4,0));
            Util.player2.PositionSetter(new Vector2(FIELDWIDTH/4,0));
        }

        public enum Mode { OnePlayer, TwoPlayer, Menu, Exit};
        private static Mode _MODE = Mode.Menu;
        public static Mode MODE
        {
            set
            {
                _MODE = value;
                ResetActors();
                //if number of players is set, fix the actors list.
                if (_MODE == Mode.OnePlayer || _MODE == Mode.TwoPlayer)
                {
                    if (value == Mode.OnePlayer)
                        LoadSinglePlayerGame();
                    if (value == Mode.TwoPlayer)
                        LoadMultiPlayerGame();
                }
            }
            get
            {
                return _MODE;
            }
        }

        public static void ResetActors()
        {
            Actors.Finish();
            Actors.Reject = true;
            Actors = new ActorList(Actors.XDim, Actors.YDim);
            Actors.Add(Util.player1);
            if (_MODE == Mode.TwoPlayer)
                Actors.Add(Util.player2);
        }

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

        public static void DrawSprite(Texture2D tex, Vector2 pos, float rot, Vector2 scale, Vector4 color)
        {
            float sc = 1.0f / tex.Width;
            pos.Y = -pos.Y;
            color.W *= AlphaHack;
            Batch.Draw(tex, pos, null, new Color(color), -rot, new Vector2(tex.Width / 2, tex.Height / 2), sc * scale, SpriteEffects.None, 0);
        }

        public static void DrawSprite(Texture2D tex, Vector2 pos, float rot, float scale, Vector4 color)
        {
            DrawSprite(tex, pos, rot, new Vector2(scale, scale), color);
        }

        public static void DrawSprite(Texture2D tex, Vector2 pos, float rot, float scale)
        {
            DrawSprite(tex, pos, rot, scale, new Vector4(1, 1, 1, 1));
        }

        public static void DrawText(Vector2 pos, string text)
        {
            DrawText(pos, text, 1.0f);
        }
        public static void DrawText(Vector2 pos, string text, float scale)
        {
            pos.Y = -pos.Y;
            Batch.DrawString(Textures.Font, text, pos, Color.White, 0, new Vector2(12,12), scale/24, SpriteEffects.None, 0);
        }

        public static void RandomExplosion(Vector2 pos)
        {
            Explosions.AddExplosion(pos, new Vector3(RandRange(0.5f, 1), RandRange(0.5f, 1), RandRange(0.5f, 1)), 
                       50, RandRange(10,40), RandRange(1,3), RandRange(0.1f, 0.4f));
        }

        public static bool OnScreen(Vector2 pos)
        {
            return pos.X >= -FIELDWIDTH/2 && pos.X <= FIELDWIDTH/2 && pos.Y >= -FIELDHEIGHT/2 && pos.Y <= FIELDHEIGHT/2;
        }
        public static Vector2 WrapToScreen(Vector2 pos)
        {
            if(pos.X < -FIELDWIDTH / 2)
                pos.X += FIELDWIDTH;
            else if (pos.X > FIELDWIDTH / 2)
                pos.X -= FIELDWIDTH;
            
            if(pos.Y < -FIELDHEIGHT / 2)
                pos.Y += FIELDHEIGHT;
            else if(pos.Y > FIELDHEIGHT / 2)
                pos.Y -= FIELDHEIGHT;

            return pos;
        }

        public static void Destroy()
        {
            DeathCount = 0;
            SCORE = 0;
            Explosions = new Explosion();
            player1 = new PlayerShip(PlayerIndex.One, player1.Input);
            player2 = new PlayerShip(PlayerIndex.Two, player2.Input);
            ResetActors();
            MODE = Mode.Menu;
        }

        public static T ProbSelect<T>(params Pair<float, T>[] parms) {
            float accum = 0;
            T ret = default(T);
            foreach (var p in parms)
            {
                accum += p.Fst;
                if (RANDOM.NextDouble() <= p.Fst / accum) ret = p.Snd;
            }
            return ret;
        }

        public static Vector2 FromAngle(float theta)
        {
            return new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
        }

        public static void EnemyDeath(Vector2 pos)
        {
            if (RANDOM.NextDouble() <= 1.0f/11)
            {
                Vector2 vel = new Vector2(RandRange(-3, 3), RandRange(-3, 3));
                Sequencer.PlayOnce(pos, Sounds.Select(Sounds.Crash));
                if (MODE == Mode.OnePlayer)
                {
                    Actors.Add(PowerUps.RandomEnemyPowerup(player1.Position, new Vector2()));
                }
                else
                {
                    Actors.Add(PowerUps.RandomEnemyPowerup(pos, vel));
                }
            }

            if (++DeathCount % 175 == 0) {
                Vector2 vel = new Vector2(RandRange(-3, 3), RandRange(-3, 3));
                Sequencer.PlayOnce(pos, "tri1");
                Actors.Add(PowerUps.RandomUpgradePowerup(pos, vel));
            }
        }
    }

}
