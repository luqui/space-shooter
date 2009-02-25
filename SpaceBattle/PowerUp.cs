using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SpaceBattle
{
    class PowerUp : Actor
    {
        Texture2D texture;
        Vector2 position;
        public override Vector2 Position { get { return position; } }
        public override float Radius { get { return 0.75f; } }
        bool dead = false;
        public override bool Dead { get { return dead; } }
        Action<PlayerShip> action;

        public PowerUp(Texture2D texture, Vector2 position, Action<PlayerShip> action)
        {
            this.texture = texture;
            this.position = position;
            this.action = action;
        }

        public override void Draw()
        {
            Util.DrawSprite(texture, position, 0, 0.75f);
        }

        public override void Collision(Actor other)
        {
            PlayerShip ship = other as PlayerShip;
            if (ship != null)
            {
                dead = true;
                action(ship);
            }
        }
    }

    static class PowerUps
    {

        public static PowerUp RandomPowerup(Vector2 position)
        {
            switch (Util.RANDOM.Next(1))
            {
                case 0: return new PowerUp(Textures.FollowerPowerup, position, ship =>
                    ship.Equip(new EnemyFactory(Textures.FollowerPowerup, 0.2f, (pos, target) =>
                        Guard(8.0f, target, new Enemy(pos, target, new SlinkTowardBehavior(), null, null)))));

                default: return null;
            }
        }

        static Enemy Guard(float radius, Actor target, Enemy e)
        {
            if ((e.Position - target.Position).Length() > radius)
            {
                return e;
            }
            else
            {
                return null;
            }
        }
    };
}
