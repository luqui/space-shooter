using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpaceBattle
{
    class PlayerShip : Actor
    {
        Vector2 position;
        public override Vector2 Position { get { return position; } }
        public override bool Dead { get { return false; } }
        public override float Radius { get { return 0.50f; } }
        Vector2 velocity;

        PlayerIndex player;
        Texture2D texture;
        Texture2D crosshair;

        int[] ammo;

        int lives;

        float bulletTimeout = 0.0f;

        const float BULLETTIME = 0.1f;
        const float SPEED = 5.0f;
        const float BULLETSPEED = 18.0f;
        const float TRIGGERLENGTH = 10.0f;
        const float SPAWNTHRESH = 10.0f;

        public PlayerShip(PlayerIndex player) { 
            this.player = player;
            lives = 5;
            ammo = new int[12];

            if (player == PlayerIndex.One)
            {
                texture = Textures.RedShip;
                crosshair = Textures.RedCrosshair;
            }
            else if (player == PlayerIndex.Two)
            {
                texture = Textures.BlueShip;
                crosshair = Textures.BlueCrosshair;
            }
        }

        public override void Update(float dt)
        {
            velocity = SPEED * GamePad.GetState(player).ThumbSticks.Left;
            position += dt * velocity;

            var input = GamePad.GetState(player);

            bulletTimeout -= dt;
            Vector2 dir = input.ThumbSticks.Right;
            if (dir.LengthSquared() > 0.125f)
            {
                var pos = position + TRIGGERLENGTH * dir;
                var other = Util.GetPlayer(Util.OtherPlayer(player));

                if (input.Triggers.Right > 0.5f && bulletTimeout <= 0) {
                    Util.Actors.Add(new Bullet(position + dir, BULLETSPEED * dir));
                    bulletTimeout = BULLETTIME;
                }
            }

        }

        public override void Draw()
        {
            float rot = (float)Math.Atan2(velocity.Y, velocity.X);
            Util.DrawSprite(texture, position, rot - (float)Math.PI/2, 1);

            Vector2 dir = GamePad.GetState(player).ThumbSticks.Right;
            if (dir.Length() >= 0.125f)
            {
                Util.DrawSprite(crosshair, position + TRIGGERLENGTH * dir, 0, 0.5f);
            }

            if (player == PlayerIndex.One)
            {
                Vector2 r = new Vector2(-Util.FIELDWIDTH / 2 + 1, Util.FIELDHEIGHT / 2 - 1);
                for (int i = 0; i < lives; i++)
                {
                    Util.DrawSprite(texture, r + new Vector2(i / 2.0f, 0), 0, 0.5f);
                }
           }
            else if (player == PlayerIndex.Two)
            {
                Vector2 r = new Vector2(Util.FIELDWIDTH / 2 - 1, Util.FIELDHEIGHT / 2 - 1);
                for (int i = 0; i < lives; i++)
                {
                    Util.DrawSprite(texture, r + new Vector2(-i / 2.0f, 0), 0, 0.5f);
                }
            }
        }

        public override void Collision(Actor other)
        {
            Bullet b = other as Bullet;
            if (b != null) { b.SetDead(); }

            if (other is Enemy)
            {
                lives--;
                Util.Reset();
            }
        }
    }
}
