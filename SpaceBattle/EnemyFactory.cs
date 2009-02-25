using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceBattle
{
    class EnemyFactory
    {
        public EnemyFactory(Texture2D texture, float timeout, Func<Vector2, PlayerShip, Enemy> spawner)
        {
            this.texture = texture;
            spawn = spawner;
            this.timeout = timeout;
            timer = 0;
        }

        public Texture2D Texture { get { return texture; } }
        Texture2D texture;
        Func<Vector2, PlayerShip, Enemy> spawn;
        float timer;
        float timeout;

        public Enemy Spawn(Vector2 pos, PlayerShip target)
        {
            if (timer <= 0)
            {
                Enemy e = spawn(pos, target);
                if (e != null) { timer = timeout; }
                return e;
            }
            return null;
        }

        public void Update(float dt)
        {
            timer -= dt;
        }
    }
}
