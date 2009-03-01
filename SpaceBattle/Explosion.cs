using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SpaceBattle
{
    class Explosion
    {
        struct Particle
        {
            public Vector2 pos;
            public Vector2 vel;
            public Vector3 color;
            public float life;
            public float size;
            public float maxlife;
        }

        LinkedList<LinkedList<Particle>> particles;
        LinkedListNode<LinkedList<Particle>> idx;
        int total = 0;
        public static int CAP = 1500;

        public Explosion()
        {
            particles = new LinkedList<LinkedList<Particle>>();
            for (int i = 0; i < 30; i++)
            {
                particles.AddLast(new LinkedList<Particle>());
            }
            idx = particles.First;
        }

        public void AddExplosion(Vector2 pos, Vector3 color, int num, float maxvel, float maxlife, float maxsize)
        {
            for (int i = 0; i < num; i++)
            {
                Particle p;
                p.pos = pos;
                float theta = Util.RandRange(0, 2 * (float)Math.PI);
                float r = Util.RandRange(0, maxvel);
                p.vel = r * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
                p.life = Util.RandRange(0, maxlife);
                p.size = Util.RandRange(0, maxsize);
                p.color = color;
                p.maxlife = maxlife;
                idx.Value.AddLast(p);
                total++;
                idx = idx.Next;
                if (idx == null) idx = particles.First;
            }
        }

        public void Update(float dt)
        {
            bool subtracted = false;
            // fast pruning
            while (total > CAP)
            {
                subtracted = true;
                total -= particles.First.Value.Count;
                particles.RemoveFirst();
                particles.AddLast(new LinkedList<Particle>());
            }

            if (subtracted)
                idx = particles.First;

            foreach (var bin in particles)
            {
                var node = bin.First;
                while (node != null)
                {
                    Particle p = node.Value;
                    p.life -= dt;
                    p.pos += dt * p.vel;
                    node.Value = p;
                    if (p.life <= 0)
                    {
                        var next = node.Next;
                        bin.Remove(node);
                        total--;
                        node = next;
                    }
                    else
                    {
                        node = node.Next;
                    }
                }
            }
        }

        public void Draw()
        {
            foreach (var bin in particles)
            {
                foreach (var n in bin)
                {
                    Util.DrawSprite(Textures.Particle, n.pos, 0, n.size, new Vector4(n.color.X, n.color.Y, n.color.Z, n.life / n.maxlife));
                }
            }
        }


    }
}
