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
        Action<Vector2> draw;
        Vector2 position;
        public override Vector2 Position { get { return position; } }
        public override float Radius { get { return 0.75f; } }
        bool dead = false;
        public override bool Dead { get { return dead; } }
        Action<PlayerShip> action;

        public PowerUp(Action<Vector2> draw, Vector2 position, Action<PlayerShip> action)
        {
            this.draw = draw;
            this.position = position;
            this.action = action;
        }

        public override void Draw()
        {
            Util.DrawSprite(Textures.RandomPowerup, position, 0, 0.75f);
            draw(position);
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

        public override void Die()
        {
            dead = true;
        }
    }

    static class PowerUps
    {
        public static PowerUp RandomPowerup(Vector2 position)
        {
            List<ComponentFactory> factories = new List<ComponentFactory>();
            factories.AddRange(Components.Behaviors.Cast<ComponentFactory>().Where(b => b.Name != "Empty"));
            factories.AddRange(Components.Seekers.Cast<ComponentFactory>().Where(b => b.Name != "Empty"));
            factories.AddRange(Components.Damages.Cast<ComponentFactory>().Where(b => b.Name != "Empty"));

            ComponentFactory f = factories[Util.RANDOM.Next(factories.Count)];
            int amount = Util.RANDOM.Next(10)+5;
            Action<Vector2> draw = v =>
            {
                f.Draw(v);
                Util.DrawText(v + new Vector2(0.75f, 0), amount.ToString());
            };
            return new PowerUp(draw, position, ship =>
            {
                ship.Equip(f.Name, amount);
            });
        }
    };
}
