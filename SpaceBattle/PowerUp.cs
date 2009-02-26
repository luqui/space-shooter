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
            return new PowerUp(Textures.RandomPowerup, position, ship => { });
        }
    };
}
