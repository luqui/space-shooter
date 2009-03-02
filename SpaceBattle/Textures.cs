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
            EmptyEnemy2 = Content.Load<Texture2D>("EmptyEnemy2");
            EmptyEnemy3 = Content.Load<Texture2D>("EmptyEnemy3");
            RandomPowerup = Content.Load<Texture2D>("RandomPowerup");
            StrongEnemy = Content.Load<Texture2D>("StrongEnemy");
            TwirlyEnemy = Content.Load<Texture2D>("TwirlyEnemy");
            Particle = Content.Load<Texture2D>("Particle");
            MineEnemy = Content.Load<Texture2D>("MineEnemy");
            FastEnemy = Content.Load<Texture2D>("FastEnemy");
            DodgeEnemy = Content.Load<Texture2D>("DodgeEnemy");
            ProjectorEnemy = Content.Load<Texture2D>("ProjectorEnemy");
            RatePowerup = Content.Load<Texture2D>("RatePowerup");
            NumPowerup = Content.Load<Texture2D>("NumPowerup");
            RingEnemy = Content.Load<Texture2D>("RingEnemy");
            RingIcon = Content.Load<Texture2D>("RingIcon");
            StatusBackground = Content.Load<Texture2D>("StatusBackground");

            Font = Content.Load<SpriteFont>("Font");
        }
        public static Texture2D FollowerEnemy;
        public static Texture2D SplittyEnemy;
        public static Texture2D Bullet;
        public static Texture2D RedShip;
        public static Texture2D BlueShip;
        public static Texture2D RedCrosshair;
        public static Texture2D BlueCrosshair;
        public static Texture2D EmptyEnemy;
        public static Texture2D EmptyEnemy2;
        public static Texture2D EmptyEnemy3;
        public static Texture2D RandomPowerup;
        public static Texture2D StrongEnemy;
        public static Texture2D TwirlyEnemy;
        public static Texture2D Particle;
        public static Texture2D MineEnemy;
        public static Texture2D FastEnemy;
        public static Texture2D DodgeEnemy;
        public static Texture2D ProjectorEnemy;
        public static Texture2D RatePowerup;
        public static Texture2D NumPowerup;
        public static Texture2D RingEnemy;
        public static Texture2D RingIcon;
        public static Texture2D StatusBackground;

        public static SpriteFont Font;
    }
}
