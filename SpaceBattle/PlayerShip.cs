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
        public void PositionSetter(Vector2 position) {this.position = position;}
        public override bool Dead { get { return false; } }
        public override float Radius { get { return 0.30f; } }
        Vector2 velocity;
        public Vector2 Velocity { get { return velocity; } }
        public int Lives { get { return lives; } }

        PlayerIndex player;
        Texture2D texture;
        Texture2D crosshair;

        ComponentRing<BehaviorComponent> behaviors = Components.MakeBehaviorRing();
        ComponentRing<SeekerComponent> seekers = Components.MakeSeekerRing();
        ComponentRing<DamageComponent> damages = Components.MakeDamageRing();
        GamePadState lastState;
        Vector2 velocityMemory;

        int lives;
        float bulletTimeout = 0.0f;
        float enemyTimeout = 0.0f;
        float shotrate = 0.1f;
        int numshots = 1;

        float immunity = 5.0f;

        const float ENEMYTIME = 0.1f;
        const float SPEED = 5.0f;
        const float BULLETSPEED = 18.0f;
        const float SPAWNTHRESH = 10.0f;
        const float MINDISTANCE = 0.20f;
        const float RINGTIMER = 60.0f;
        const float TRIGGERLENGTH = 10.0f;

        int rings = 1;
        float ringTimer = 0f;

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
            var input = GamePad.GetState(player);

            if (input.Buttons.Back == ButtonState.Pressed) { Util.MODE = Util.Mode.Menu; Util.Destroy();  return; }
            if (lives <= 0) return;

            velocity = SPEED * input.ThumbSticks.Left;
            if (velocity.LengthSquared() > 0) velocityMemory = velocity;
            position += dt * velocity;

            Func<Func<GamePadButtons, ButtonState>, bool> pressed = f => 
                f(lastState.Buttons) == ButtonState.Released && f(input.Buttons) == ButtonState.Pressed;

            Vector2 dir = input.ThumbSticks.Right;

            bool resetAmmo = false;
            if (pressed(i => i.Y)) { resetAmmo = true; behaviors.Next(); }
            if (pressed(i => i.X)) { resetAmmo = true; seekers.Next(); }
            if (pressed(i => i.A)) { resetAmmo = true; damages.Next(); }
            if (rings > 0 && (pressed(i => i.RightShoulder) || pressed(i => i.LeftShoulder)))
            {
                rings--;
                Util.Actors.Add(new Ring(position + TRIGGERLENGTH * dir));
                if (rings == 0) ringTimer = RINGTIMER;
            }

            if (ringTimer > 0 && ringTimer - dt <= 0)
            {
                Util.Sequencer.PlayOnce(position, "tri2");
                rings++;
            }
            ringTimer -= dt;

            immunity -= dt;
            bulletTimeout -= dt;
            enemyTimeout -= dt;
            int shots = 0;

            while (bulletTimeout <= 0) { shots++; bulletTimeout += dt; }

            if (dir.LengthSquared() < MINDISTANCE * MINDISTANCE)
            {
                dir = velocityMemory;
                if (dir != Vector2.Zero) //normalizing a 0 vector results in NAN.
                    dir.Normalize(); 
                dir *= 0.01f;
            }

            bool enemyShoot = Util.MODE == Util.Mode.OnePlayer ? input.Triggers.Right > 0.5f : input.Triggers.Left > 0.5f;
            bool bulletShoot = input.Triggers.Right > 0.5f;

            var pos = Util.MODE == Util.Mode.OnePlayer ? position - TRIGGERLENGTH * dir : position + TRIGGERLENGTH * dir;
            var other = Util.MODE == Util.Mode.OnePlayer ? this : Util.GetPlayer(Util.OtherPlayer(player));

            if (enemyShoot && enemyTimeout <= 0)
            {
                if ((behaviors.Ammo > 0 || seekers.Ammo > 0 || damages.Ammo > 0))
                {
                    if (Util.MODE == Util.Mode.OnePlayer && !Util.OnScreen(pos)) // if spawning offscreen in single player, wrap.
                    {
                        pos = Util.WrapToScreen(pos);
                    }
                    Enemy e = new Enemy(pos, other, behaviors.Spawn(), seekers.Spawn(), damages.Spawn());
                    Util.Actors.Add(e);
                    enemyTimeout = ENEMYTIME;
                    resetAmmo = true;
                }
            }

            float offset = 1.0f;
            for (int shot = 0; shot < shots; shot++)
            {
                bulletTimeout += shotrate;
                if (bulletShoot)
                {
                    float thetastep = 5 * (float)Math.PI / 180;
                    float theta = (float)Math.Atan2(dir.Y, dir.X) - (numshots-1) * thetastep / 2;
                    for (int i = 0; i < numshots; i++)
                    {
                        Vector2 dir2 = new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
                        Vector4 color = player == PlayerIndex.One ? new Vector4(0, 1, 0, 1) : new Vector4(0.3f, 0.6f, 1, 1);
                        Util.Actors.Add(new Bullet(position + offset * dir2, BULLETSPEED * dir2, color));
                        theta += thetastep;
                    }
                    offset += 0.3f;
                }
            }

            if (resetAmmo) ResetAmmo();
            lastState = input;

            if (position.X < -Util.FIELDWIDTH / 2) position.X = -Util.FIELDWIDTH / 2;
            if (position.X >= Util.FIELDWIDTH / 2) position.X = Util.FIELDWIDTH / 2 - 0.01f;
            if (position.Y < -Util.FIELDHEIGHT / 2) position.Y = -Util.FIELDHEIGHT / 2;
            if (position.Y >= Util.FIELDHEIGHT / 2) position.Y = Util.FIELDHEIGHT / 2 - 0.01f;
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
            if (lives <= 0) return;

            float rot = (float)Math.Atan2(velocityMemory.Y, velocityMemory.X);
            Util.DrawSprite(texture, position, rot - (float)Math.PI/2, 1);

            Vector2 dir = GamePad.GetState(player).ThumbSticks.Right;
            if (dir.LengthSquared() >= MINDISTANCE*MINDISTANCE)
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
                for (int i = 0; i < rings; i++)
                {
                    Util.DrawSprite(Textures.RingIcon, r + new Vector2(i / 2.0f + 3, 0), 0, 0.5f);
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
                for (int i = 0; i < rings; i++)
                {
                    Util.DrawSprite(Textures.RingIcon, r + new Vector2(-i / 2.0f - 3, 0), 0, 0.5f);
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
                Util.Actors.Add(new ProxyEnemy(e));
            }
        }

        public override void Die()
        {
            if (immunity > 0) return;
            lives--;
            Util.ResetActors();
            Util.Sequencer.PlayOnce(new Vector2(0,0), Sounds.Select(Sounds.Gong));
            Vector3 color = player == PlayerIndex.One ? new Vector3(1, 0, 0) : new Vector3(0, 0.3f, 1.0f);
            Util.Explosions.AddExplosion(position, color, 300, 25, 5, 0.5f);
            immunity = 5.0f;
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

        public void AddRing()
        {
            rings++;
        }
    }
}
