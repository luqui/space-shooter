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
            public float life;
            public float size;
        }

        Particle[] particles;

        float maxlife;
        float life;
        Vector3 color;

        public Explosion(Vector2 pos, Vector3 color, int num, float maxvel, float maxlife, float maxsize)
        {
            particles = new Particle[num];
            for (int i = 0; i < num; i++)
            {
                particles[i].pos = pos;
                float theta = Util.RandRange(0, 2 * (float)Math.PI);
                float r = Util.RandRange(0, maxvel);
                particles[i].vel = r * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
                particles[i].life = Util.RandRange(0, maxlife);
                particles[i].size = Util.RandRange(0, maxsize);
            }
            this.maxlife = maxlife;
            life = maxlife;
            this.color = color;
        }

        public void Update(float dt)
        {
            for (int i = 0; i < particles.Count(); i++)
            {
                particles[i].pos += dt * particles[i].vel;
                particles[i].life -= dt;
            }
            life -= dt;
        }

        public void Draw()
        {
            for (int i = 0; i < particles.Count(); i++)
            {
                Util.DrawSprite(Textures.Particle, particles[i].pos, 0, particles[i].size, new Vector4(color.X, color.Y, color.Z, particles[i].life/maxlife));
            }
        }

        public bool Done { get { return life < 0; } }
    }
}
