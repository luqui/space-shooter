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

        ComponentRing<BehaviorComponent> behaviors = Components.MakeBehaviorRing();
        ComponentRing<SeekerComponent> seekers = Components.MakeSeekerRing();
        ComponentRing<DamageComponent> damages = Components.MakeDamageRing();
        GamePadState lastState;

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

            lastState = new GamePadState();

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
            Func<Func<GamePadButtons, ButtonState>, bool> pressed = f => 
                f(lastState.Buttons) == ButtonState.Released && f(input.Buttons) == ButtonState.Pressed;

            if (pressed(i => i.Y)) { behaviors.Next(); }
            if (pressed(i => i.X)) { seekers.Next(); }
            if (pressed(i => i.A)) { damages.Next(); }

            bulletTimeout -= dt;
            Vector2 dir = input.ThumbSticks.Right;
            if (dir.LengthSquared() > 0.125f)
            {
                var pos = position + TRIGGERLENGTH * dir;
                var other = Util.GetPlayer(Util.OtherPlayer(player));

                if (input.Triggers.Left > 0.5f && bulletTimeout <= 0)
                {
                    if (behaviors.Ammo > 0 && seekers.Ammo > 0 && damages.Ammo > 0)
                    {
                        Enemy e = new Enemy(pos, other, behaviors.Spawn(), seekers.Spawn(), damages.Spawn());
                        Util.Actors.Add(e);
                        bulletTimeout = BULLETTIME;
                    }
                }
                else if (input.Triggers.Right > 0.5f && bulletTimeout <= 0)
                {
                    Util.Actors.Add(new Bullet(position + (1.2f * dir / dir.Length()), BULLETSPEED * dir));
                    bulletTimeout = BULLETTIME;
                }
            }

            lastState = input;
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
                behaviors.Draw(r + new Vector2(0, -1), new Vector2(2, 0));
                seekers.Draw(r + new Vector2(0, -2), new Vector2(2, 0));
                damages.Draw(r + new Vector2(0, -3), new Vector2(2, 0));
           }
            else if (player == PlayerIndex.Two)
            {
                Vector2 r = new Vector2(Util.FIELDWIDTH / 2 - 1, Util.FIELDHEIGHT / 2 - 1);
                for (int i = 0; i < lives; i++)
                {
                    Util.DrawSprite(texture, r + new Vector2(-i / 2.0f, 0), 0, 0.5f);
                }
                behaviors.Draw(r + new Vector2(-1, -1), new Vector2(-2, 0));
                seekers.Draw(r + new Vector2(-1, -2), new Vector2(-2, 0));
                damages.Draw(r + new Vector2(-1, -3), new Vector2(-2, 0));
            }
        }

        public override void Collision(Actor other)
        {
            Bullet b = other as Bullet;
            if (b != null) { 
                b.SetDead();
                Util.GetPlayer(Util.OtherPlayer(player)).Equip("Empty", 1);
            }

            if (other is Enemy)
            {
                lives--;
                Util.Reset();
                Vector3 color = player == PlayerIndex.One ? new Vector3(1, 0, 0) : new Vector3(0, 0.3f, 1.0f);
                Util.Actors.Add(new Explosion(position, color, 300, 25, 5, 0.5f));
            }
        }

        public void Equip(string e, int amount)
        {
            behaviors.Add(e, amount);
            seekers.Add(e, amount);
            damages.Add(e, amount);
        }
    }
}
