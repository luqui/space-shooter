using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceBattle
{
    class ProxyEnemy : Actor
    {
        public ProxyEnemy(Enemy e) { enemy = e; }

        Enemy enemy;
        float timer = FADEOUT;
        const float FADEOUT = 3.0f;

        public override Microsoft.Xna.Framework.Vector2 Position
        {
            get { return enemy.Position; }
        }

        public override bool Dead
        {
            get { return timer <= 0;  }
        }

        public override float Radius
        {
            get { return enemy.Radius; }
        }

        public override void Die()
        {
            timer = 0;
        }

        public override void Update(float dt)
        {
            timer -= dt;
        }

        public override void Draw()
        {
            float prealpha = Util.AlphaHack;
            Util.AlphaHack *= timer / FADEOUT;
            enemy.Draw();
            Util.AlphaHack = prealpha;
        }
    }
}
