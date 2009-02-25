using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SpaceBattle
{
    class Enemy : Actor
    {
        public Enemy(Vector2 pos, Actor targ,
                     BehaviorComponent b, SeekerComponent s, DamageComponent d)
        {
            position = pos;
            target = targ;
            Behavior = b;
            Seeker = s;
            Damage = d;
            if (Behavior != null) Behavior.Reassign(this);
            if (Seeker != null) Seeker.Reassign(this);
            if (Damage != null) Damage.Reassign(this);
        }

        // only for use in components:
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 accel;
        public bool dead = false;
        public Actor target = null;

        public BehaviorComponent Behavior = null;
        public SeekerComponent Seeker = null;
        public DamageComponent Damage = null;

        public Enemy Clone()
        {
            return new Enemy(position, target, 
                Behavior == null ? null : Behavior.Clone(), 
                Seeker == null ? null : Seeker.Clone(), 
                Damage == null ? null : Damage.Clone());
        }

        public override void Draw()
        {
            if (Behavior == null && Seeker == null && Damage == null)
            {
                Util.DrawSprite(Textures.EmptyEnemy, position, 0, 1.0f);
            }
            else
            {
                if (Behavior != null) Behavior.Draw();
                if (Seeker != null) Seeker.Draw();
                if (Damage != null) Damage.Draw();
            }
        }

        public override void Update(float dt)
        {
            if (Behavior != null) Behavior.Update(dt);
            if (Seeker != null) Seeker.Update(dt); 
            velocity += dt * accel;
            position += dt * velocity;
            accel = new Vector2();
        }

        public override bool Dead { get { return dead; } }
        public override Vector2 Position { get { return position; } }
        public override float Radius { get { return 0.75f; } }

        public override void Collision(Actor other)
        {
            Bullet b = other as Bullet;
            if (b != null) { 
                if (Damage != null) Damage.OnHit(b);
                else
                {
                    dead = true;
                    b.SetDead();
                }
            }
        }

        public void Absorb(Enemy other)
        {
            if ((Behavior == null || other.Behavior == null)
             && (Seeker == null || other.Seeker == null)
             && (Damage == null || other.Damage == null))
            {
                Behavior = Join(Behavior, other.Behavior);
                Seeker = Join(Seeker, other.Seeker);
                Damage = Join(Damage, other.Damage);
                other.dead = true;
            }
        }

        static T Join<T>(T x, T y) where T:class
        {
            if (x == null) { return y; }
            if (y == null) { return x; }
            return null;
        }

    }

    abstract class Component
    {
        protected Enemy self;
        public virtual void Reassign(Enemy newself)
        {
            self = newself;
        }
        public abstract void Draw();
    }

    abstract class BehaviorComponent : Component
    {
        public abstract void Update(float dt);
        public abstract BehaviorComponent Clone();
    }

    abstract class SeekerComponent : Component
    {
        public abstract void Update(float dt);
        public abstract SeekerComponent Clone();
    }

    class SlinkTowardSeeker : SeekerComponent
    {
        public override void Draw()
        {
            Util.DrawSprite(Textures.FollowerEnemy, self.position, 0, 1.0f);
        }
        public override void Update(float dt)
        {
            var desired = self.target.Position - self.position;
            desired.Normalize();
            self.accel += desired - self.velocity;
        }
        public override SeekerComponent Clone()
        {
            return new SlinkTowardSeeker();
        }
    }

    abstract class DamageComponent : Component
    {
        public abstract void OnHit(Bullet bullet);
        public abstract DamageComponent Clone();
    }

    class SplittyDamage : DamageComponent
    {
        public override void Draw()
        {
            Util.DrawSprite(Textures.SplittyEnemy, self.position, 0, 1.0f);
        }
        public override void OnHit(Bullet bullet)
        {
            self.dead = true;
            bullet.SetDead();
            Enemy child1 = self.Clone(); child1.Damage = null;
            Enemy child2 = self.Clone(); child2.Damage = null;
            Enemy child3 = self.Clone(); child3.Damage = null;
            child1.position.X += Util.RandRange(-1, 1);
            child1.position.Y += Util.RandRange(-1, 1);
            child2.position.X += Util.RandRange(-1, 1);
            child2.position.Y += Util.RandRange(-1, 1);
            child3.position.X += Util.RandRange(-1, 1);
            child3.position.Y += Util.RandRange(-1, 1);
            Util.Actors.Add(child1);
            Util.Actors.Add(child2);
            Util.Actors.Add(child3);
        }
        public override DamageComponent Clone()
        {
            return new SplittyDamage();
        }
    }
}
