using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SpaceBattle
{
    class EnemyFactory
    {
        public EnemyFactory(float timeout, Func<Vector2, PlayerShip, Enemy> spawner)
        {
            spawn = spawner;
            this.timeout = timeout;
            timer = 0;
        }

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

    abstract class Enemy : Actor
    { }
}
