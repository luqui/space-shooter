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
        public override bool Dead { get { return lives <= 0; } }
        public override float Radius { get { return 0.50f; } }
        Vector2 velocity;
        public Vector2 Velocity { get { return velocity; } }

        PlayerIndex player;
        Texture2D texture;
        Texture2D crosshair;

        ComponentRing<BehaviorComponent> behaviors = Components.MakeBehaviorRing();
        ComponentRing<SeekerComponent> seekers = Components.MakeSeekerRing();
        ComponentRing<DamageComponent> damages = Components.MakeDamageRing();
        GamePadState lastState;

        int lives;

        int[,] storage;

        float bulletTimeout = 0.0f;
        float enemyTimeout = 0.0f;
        float shotrate = 0.1f;
        int numshots = 1;

        const float ENEMYTIME = 0.1f;
        const float SPEED = 5.0f;
        const float BULLETSPEED = 18.0f;
        const float TRIGGERLENGTH = 10.0f;
        const float SPAWNTHRESH = 10.0f;

        public PlayerShip(PlayerIndex player) { 
            this.player = player;
            lives = 5;
            storage = new int[8, 3];

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

        void ResetAmmo()
        {
            if (!behaviors.Empty())
            {
                while (behaviors.Ammo == 0) { behaviors.Next(); }
            }
            if (!seekers.Empty())
            {
                while (seekers.Ammo == 0) { seekers.Next(); }
            }
            if (!damages.Empty())
            {
                while (damages.Ammo == 0) { damages.Next(); }
            }
        }

        public override void Update(float dt)
        {
            velocity = SPEED * GamePad.GetState(player).ThumbSticks.Left;
            position += dt * velocity;

            var input = GamePad.GetState(player);
            Func<Func<GamePadButtons, ButtonState>, bool> pressed = f => 
                f(lastState.Buttons) == ButtonState.Released && f(input.Buttons) == ButtonState.Pressed;

            bool resetAmmo = false;
            if (pressed(i => i.Y)) { resetAmmo = true; behaviors.Next(); }
            if (pressed(i => i.X)) { resetAmmo = true; seekers.Next(); }
            if (pressed(i => i.A)) { resetAmmo = true; damages.Next(); }

            bulletTimeout -= dt;
            enemyTimeout -= dt;
            int shots = 0;

            while (bulletTimeout <= 0) { shots++; bulletTimeout += dt; }

            Vector2 dir = input.ThumbSticks.Right;
            if (dir.LengthSquared() > 0.125f)
            {
                bool enemyShoot = Util.MODE == Util.Mode.OnePlayer ? true : input.Triggers.Left > 0.5f;
                bool bulletShoot = Util.MODE == Util.Mode.OnePlayer ? true : input.Triggers.Right > 0.5f;

                var pos = Util.MODE == Util.Mode.OnePlayer ? position - TRIGGERLENGTH * dir : position + TRIGGERLENGTH * dir;
                var other = Util.MODE == Util.Mode.OnePlayer ? this : Util.GetPlayer(Util.OtherPlayer(player));

                if (enemyShoot && enemyTimeout <= 0)
                {
                    if (behaviors.Ammo > 0 && seekers.Ammo > 0 && damages.Ammo > 0 && Util.OnScreen(pos))
                    {
                        Enemy e = new Enemy(pos, other, behaviors.Spawn(), seekers.Spawn(), damages.Spawn());
                        Util.Actors.Add(e);
                        enemyTimeout = ENEMYTIME;
                        resetAmmo = true;
                    }
                }

                float offset = 1.2f;
                for (int shot = 0; shot < shots; shot++)
                {
                    bulletTimeout += shotrate;
                    if (bulletShoot)
                    {
                        float thetastep = 5 * (float)Math.PI / 180;
                        float theta = (float)Math.Atan2(dir.Y, dir.X) - numshots * thetastep / 2;
                        for (int i = 0; i < numshots; i++)
                        {
                            Vector2 dir2 = new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
                            Util.Actors.Add(new Bullet(position + offset * dir2, BULLETSPEED * dir2));
                            theta += thetastep;
                        }
                        offset += 0.3f;
                    }
                }
            }


            int dpad = DPadDirection(input);
            if (input.Buttons.RightShoulder == ButtonState.Pressed)
            {
                if (dpad > 0)
                {
                    storage[dpad, 0] = behaviors.Index;
                    storage[dpad, 1] = seekers.Index;
                    storage[dpad, 2] = damages.Index;
                }
            }
            else
            {
                if (dpad > 0)
                {
                    behaviors.Index = storage[dpad, 0];
                    seekers.Index = storage[dpad, 1];
                    damages.Index = storage[dpad, 2];
                    resetAmmo = true;
                }
            }

            if (resetAmmo) ResetAmmo();
            lastState = input;

            if (position.X < -Util.FIELDWIDTH / 2) position.X = -Util.FIELDWIDTH / 2;
            if (position.X > Util.FIELDWIDTH / 2) position.X = Util.FIELDWIDTH / 2;
            if (position.Y < -Util.FIELDHEIGHT / 2) position.Y = -Util.FIELDHEIGHT / 2;
            if (position.Y > Util.FIELDHEIGHT / 2) position.Y = Util.FIELDHEIGHT / 2;
        }

        int DPadDirection(GamePadState input)
        {
            bool u = input.DPad.Up == ButtonState.Pressed;
            bool d = input.DPad.Down == ButtonState.Pressed;
            bool l = input.DPad.Left == ButtonState.Pressed;
            bool r = input.DPad.Right == ButtonState.Pressed;

            // starting from top, clockwise: 0-7
            int dir =
                u && l ? 7 :
                u && r ? 1 :
                d && r ? 3 :
                d && l ? 5 :
                u ? 0 :
                r ? 2 :
                d ? 4 :
                l ? 6 : -1;
            return dir;
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
                b.Die();
                if (Util.MODE == Util.Mode.TwoPlayer)
                {
                    Util.GetPlayer(Util.OtherPlayer(player)).Equip("Empty", 1);
                }
            }

            Enemy e = other as Enemy;
            if (e != null && e.fadeIn <= 0)
            {
                Die();
            }
        }

        public override void Die()
        {
            lives--;
            Util.Reset();
            Util.Sequencer.PlayOnce(Sounds.Select(Sounds.Gong));
            Vector3 color = player == PlayerIndex.One ? new Vector3(1, 0, 0) : new Vector3(0, 0.3f, 1.0f);
            Util.Actors.Add(new Explosion(position, color, 300, 25, 5, 0.5f));
        }

        public void Equip(string e, int amount)
        {
            behaviors.Add(e, amount);
            seekers.Add(e, amount);
            damages.Add(e, amount);
            ResetAmmo();
        }

        public void FasterShots()
        {
            shotrate *= 0.8f;
            Util.Scheduler.Enqueue(30, () => shotrate /= 0.8f);
        }

        public void MoreShots()
        {
            numshots++;
            Util.Scheduler.Enqueue(30, () => numshots--);
        }
    }
}
