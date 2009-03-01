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
        Vector2 velocity;
        public override Vector2 Position { get { return position; } }
        public override float Radius { get { return 0.75f; } }
        bool dead = false;
        float timeout;
        public override bool Dead { get { return dead || timeout <= 0; } }
        Action<PlayerShip> action;

        public PowerUp(Action<Vector2> draw, float timeout, Vector2 position, Vector2 velocity, Action<PlayerShip> action)
        {
            this.draw = draw;
            this.position = position;
            this.action = action;
            this.velocity = velocity;
            this.timeout = timeout;
        }

        public override void Update(float dt)
        {
            if (position.X + dt * velocity.X > Util.FIELDWIDTH / 2
              || position.X + dt * velocity.X < -Util.FIELDWIDTH / 2) { velocity.X = -velocity.X; }
            if (position.Y + dt * velocity.Y > Util.FIELDHEIGHT / 2
              || position.Y + dt * velocity.Y < -Util.FIELDHEIGHT / 2) { velocity.Y = -velocity.Y; }
            position += dt * velocity;
            timeout -= dt;
        }

        public override void Draw()
        {
            if (timeout < 3 && (int)(4 * timeout) % 2 == 0) return;
            float prealpha = Util.AlphaHack;
            Util.AlphaHack *= 0.5f;
            Util.DrawSprite(Textures.RandomPowerup, position, 0, 0.75f);
            draw(position);
            Util.AlphaHack = prealpha;
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
        public static PowerUp RandomPowerup(Vector2 position, Vector2 velocity)
        {
            List<ComponentFactory> factories = new List<ComponentFactory>();
            factories.AddRange(Components.Behaviors.Cast<ComponentFactory>().Where(b => b.Name != "Empty"));
            factories.AddRange(Components.Seekers.Cast<ComponentFactory>().Where(b => b.Name != "Empty"));
            factories.AddRange(Components.Damages.Cast<ComponentFactory>().Where(b => b.Name != "Empty"));

            ComponentFactory f = factories[Util.RANDOM.Next(factories.Count)];
            int amount = Util.RANDOM.Next(10) + 5;
            Action<Vector2> draw = v =>
            {
                f.Draw(v);
                Util.DrawText(v + new Vector2(0.75f, 0), amount.ToString());
            };
            return new PowerUp(draw, float.PositiveInfinity, position, velocity, ship =>
            {
                ship.Equip(f.Name, amount);
            });
        }
    };
}
