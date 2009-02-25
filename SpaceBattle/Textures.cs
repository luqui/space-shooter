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
            SplittyEnemy = Content.Load<Texture2D>("SplittyEnemy");
            Bullet = Content.Load<Texture2D>("Bullet");
            RedShip = Content.Load<Texture2D>("RedShip");
            BlueShip = Content.Load<Texture2D>("BlueShip");
            RedCrosshair = Content.Load<Texture2D>("RedCrosshair");
            BlueCrosshair = Content.Load<Texture2D>("BlueCrosshair");
            EmptyEnemy = Content.Load<Texture2D>("EmptyEnemy");
            RandomPowerup = Content.Load<Texture2D>("RandomPowerup");
            StrongEnemy = Content.Load<Texture2D>("StrongEnemy");
            TwirlyEnemy = Content.Load<Texture2D>("TwirlyEnemy");
        }
        public static Texture2D FollowerEnemy;
        public static Texture2D SplittyEnemy;
        public static Texture2D Bullet;
        public static Texture2D RedShip;
        public static Texture2D BlueShip;
        public static Texture2D RedCrosshair;
        public static Texture2D BlueCrosshair;
        public static Texture2D EmptyEnemy;
        public static Texture2D RandomPowerup;
        public static Texture2D StrongEnemy;
        public static Texture2D TwirlyEnemy;
    }
}
