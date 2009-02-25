using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SpaceBattle
{
    static class Textures
    {
        public static void LoadContent(ContentManager Content) {
            FollowerEnemy = Content.Load<Texture2D>("FollowerEnemy");
            FollowerPowerup = Content.Load<Texture2D>("FollowerPowerup");
            SplittyPowerup = Content.Load<Texture2D>("SplittyPowerup");
            Bullet = Content.Load<Texture2D>("Bullet");
            RedShip = Content.Load<Texture2D>("RedShip");
            BlueShip = Content.Load<Texture2D>("BlueShip");
            RedCrosshair = Content.Load<Texture2D>("RedCrosshair");
            BlueCrosshair = Content.Load<Texture2D>("BlueCrosshair");
        }
        public static Texture2D FollowerEnemy;
        public static Texture2D FollowerPowerup;
        public static Texture2D SplittyPowerup;
        public static Texture2D Bullet;
        public static Texture2D RedShip;
        public static Texture2D BlueShip;
        public static Texture2D RedCrosshair;
        public static Texture2D BlueCrosshair;
    }
}
