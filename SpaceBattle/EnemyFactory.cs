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
        public EnemyFactory(Action<Vector2> draw, int store, float refresh, float timeout, Func<Vector2, PlayerShip, Enemy> spawner)
        {
            this.draw = draw;
            fire_timer = 0;
            fire_timeout = timeout;
            refresh_timer = refresh;
            refresh_timeout = refresh;
            this.store = 0;
            this.maxstore = store;
            spawn = spawner;
        }

        Action<Vector2> draw;
        Func<Vector2, PlayerShip, Enemy> spawn;
        float fire_timer;
        float fire_timeout;
        float refresh_timer;
        float refresh_timeout;
        int store;
        int maxstore;

        public void Draw(Vector2 position) { 
            draw(position);
            Util.DrawText(position + new Vector2(1,0), store.ToString());
        }

        public Enemy Spawn(Vector2 pos, PlayerShip target)
        {
            if (store > 0 && fire_timer <= 0)
            {
                Enemy e = spawn(pos, target);
                if (e != null)
                {
                    fire_timer = fire_timeout;
                    store--;
                }
                return e;
            }
            return null;
        }

        public void Update(float dt)
        {
            fire_timer -= dt;
            refresh_timer -= dt;
            if (refresh_timer <= 0)
            {
                store++;
                refresh_timer = refresh_timeout;
                if (store > maxstore) store = maxstore;
            }
        }
    }
}
