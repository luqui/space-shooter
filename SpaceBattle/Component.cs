using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SpaceBattle
{
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

    abstract class DamageComponent : Component
    {
        public abstract void OnHit(Actor other);
        public abstract DamageComponent Clone();
    }

    static class Components
    {
        public static List<ComponentFactory<BehaviorComponent>> Behaviors = 
            new List<ComponentFactory<BehaviorComponent>> {
                new ComponentFactory<BehaviorComponent>("Empty", () => null, pos => Util.DrawSprite(Textures.EmptyEnemy, pos, 0, 1.0f)),
                new ComponentFactory<BehaviorComponent>("Twirly", () => new TwirlyBehavior(), pos => Util.DrawSprite(Textures.TwirlyEnemy, pos, 0, 1.0f)) 
            };

        public static List<ComponentFactory<SeekerComponent>> Seekers =
            new List<ComponentFactory<SeekerComponent>> {
                new ComponentFactory<SeekerComponent>("Empty", () => null, pos => Util.DrawSprite(Textures.EmptyEnemy, pos, 0, 1.0f)),
                new ComponentFactory<SeekerComponent>("SlinkToward", () => new SlinkTowardSeeker(), pos => Util.DrawSprite(Textures.FollowerEnemy, pos, 0, 1.0f)),
                new ComponentFactory<SeekerComponent>("FastSeeker", () => new FastSeeker(), pos => Util.DrawSprite(Textures.FastEnemy, pos, 0, 1.0f))
            };

        public static List<ComponentFactory<DamageComponent>> Damages =
            new List<ComponentFactory<DamageComponent>> {
                new ComponentFactory<DamageComponent>("Empty", () => null, pos => Util.DrawSprite(Textures.EmptyEnemy, pos, 0, 1.0f)),
                new ComponentFactory<DamageComponent>("Splitty", () => new SplittyDamage(), pos => Util.DrawSprite(Textures.SplittyEnemy, pos, 0, 1.0f)),
                new ComponentFactory<DamageComponent>("Tough", () => new ToughDamage(), pos => Util.DrawSprite(Textures.StrongEnemy, pos, 0, 1.0f)),
                new ComponentFactory<DamageComponent>("Mine", () => new MineDamage(), pos => Util.DrawSprite(Textures.MineEnemy, pos, 0, 1.0f))
            };

        public static ComponentRing<BehaviorComponent> MakeBehaviorRing()
        {
            return new ComponentRing<BehaviorComponent>(Behaviors);
        }

        public static ComponentRing<SeekerComponent> MakeSeekerRing()
        {
            return new ComponentRing<SeekerComponent>(Seekers);
        }

        public static ComponentRing<DamageComponent> MakeDamageRing()
        {
            return new ComponentRing<DamageComponent>(Damages);
        }
    }

    class TwirlyBehavior : BehaviorComponent
    {
        float timer = 0;
        public override void Draw()
        {
            Util.DrawSprite(Textures.TwirlyEnemy, self.position, timer, 1.0f);
        }
        public override void Update(float dt)
        {
            self.position.X -= (float)Math.Cos(4 * timer);
            self.position.Y -= (float)Math.Sin(4 * timer);
            timer += dt;
            self.position.X += (float)Math.Cos(4 * timer);
            self.position.Y += (float)Math.Sin(4 * timer);
        }
        public override BehaviorComponent Clone()
        {
            return new TwirlyBehavior();
        }
    }

    class SlinkTowardSeeker : SeekerComponent
    {
        public override void Draw()
        {
            Util.DrawSprite(Textures.FollowerEnemy, self.position, 0, 1.0f);
        }
        public override void Update(float dt)
        {
            var diff = self.target.Position - self.position;
            var desired = 3 * diff / diff.Length();
            self.accel += desired - self.velocity;
        }
        public override SeekerComponent Clone()
        {
            return new SlinkTowardSeeker();
        }
    }

    class FastSeeker : SeekerComponent
    {
        public override void Draw()
        {
            float rot = (float)(Math.Atan2(self.velocity.Y, self.velocity.X) - Math.PI / 2);
            Util.DrawSprite(Textures.FastEnemy, self.position, rot, 1.0f);
        }
        public override void Update(float dt)
        {
            Vector2 dir = self.velocity;
            Vector2 tdir = self.target.Position - self.position;
            if (dir.LengthSquared() == 0) { dir = tdir; }
            dir.Normalize();
            tdir.Normalize();

            float scale = (float)Math.Abs(Vector2.Dot(dir, tdir));
            if (scale < 0.2f)
            {
                self.accel += tdir - 5 * self.velocity;
            }
            else
            {
                self.accel += 5 * tdir * scale;
            }
        }
        public override SeekerComponent Clone()
        {
            return new FastSeeker();
        }
    }

    class SplittyDamage : DamageComponent
    {
        public override void Draw()
        {
            Util.DrawSprite(Textures.SplittyEnemy, self.position, 0, 1.0f);
        }
        public override void OnHit(Actor other)
        {
            Bullet bullet = other as Bullet;
            if (bullet == null) return;

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
            Util.RandomExplosion(self.position);
        }
        public override DamageComponent Clone()
        {
            return new SplittyDamage();
        }
    }

    class ToughDamage : DamageComponent
    {
        int hitPoints = 4;

        public override void Draw()
        {
            float size = 0.25f * hitPoints + 0.5f;
            Util.DrawSprite(Textures.StrongEnemy, self.Position, 0, size);
        }
        public override void OnHit(Actor other)
        {
            Bullet bullet = other as Bullet;
            if (bullet == null) return;

            bullet.SetDead();
            hitPoints--;
            if (hitPoints <= 0)
            {
                self.dead = true;
                Util.RandomExplosion(self.position);
            }
        }
        public override DamageComponent Clone()
        {
            return new ToughDamage();  // keep damage?
        }
    }

    class MineDamage : DamageComponent
    {
        public override void Draw()
        {
            Util.DrawSprite(Textures.MineEnemy, self.position, 0, 1.0f);
        }
        public override void OnHit(Actor other)
        {
            Enemy enemy = other as Enemy;
            if (enemy != null)
            {
                if (enemy.Damage is MineDamage) return;
                enemy.dead = true;
                Util.RandomExplosion(enemy.position);
            }

            Bullet bullet = other as Bullet;
            if (bullet != null)
            {
                bullet.SetDead();
            }

            self.dead = true;
            Util.RandomExplosion(self.position);
        }
        public override DamageComponent Clone()
        {
            return new MineDamage();
        }
    }
}
