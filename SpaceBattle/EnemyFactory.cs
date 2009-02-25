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
        public EnemyFactory(Action<Vector2> draw, float timeout, Func<Vector2, PlayerShip, Enemy> spawner)
        {
            this.draw = draw;
            spawn = spawner;
            this.timeout = timeout;
            timer = 0;
        }

        Action<Vector2> draw;
        Func<Vector2, PlayerShip, Enemy> spawn;
        float timer;
        float timeout;

        public void Draw(Vector2 position) { draw(position); }

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
