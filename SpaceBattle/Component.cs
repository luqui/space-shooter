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
            bool startme = self == null;
            self = newself;
            if (startme) Start();
        }
        public abstract void Draw();
        public virtual void Start() { }
        public virtual void Finish() { }
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
        public abstract void Die();
        public abstract DamageComponent Clone();
    }

    static class Components
    {
        public static List<ComponentFactory<BehaviorComponent>> Behaviors = 
            new List<ComponentFactory<BehaviorComponent>> {
                new ComponentFactory<BehaviorComponent>("Empty", () => null, pos => Util.DrawSprite(Textures.EmptyEnemy, pos, 0, 1.0f)),
                new ComponentFactory<BehaviorComponent>("Twirly", () => new TwirlyBehavior(), pos => Util.DrawSprite(Textures.TwirlyEnemy, pos, 0, 1.0f)),
                new ComponentFactory<BehaviorComponent>("Dodge", () => new DodgeBehavior(), pos => Util.DrawSprite(Textures.DodgeEnemy, pos, 0, 1.0f))
            };

        public static List<ComponentFactory<SeekerComponent>> Seekers =
            new List<ComponentFactory<SeekerComponent>> {
                new ComponentFactory<SeekerComponent>("Empty", () => null, pos => Util.DrawSprite(Textures.EmptyEnemy, pos, 0, 1.0f)),
                new ComponentFactory<SeekerComponent>("SlinkToward", () => new SlinkTowardSeeker(), pos => Util.DrawSprite(Textures.FollowerEnemy, pos, 0, 2.0f)),
                new ComponentFactory<SeekerComponent>("FastSeeker", () => new FastSeeker(), pos => Util.DrawSprite(Textures.FastEnemy, pos, 0, 2.0f)),
                new ComponentFactory<SeekerComponent>("ProjectorSeeker", () => new ProjectorSeeker(), pos => Util.DrawSprite(Textures.ProjectorEnemy, pos, 0, 2.0f))
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
        public override void Start()
        {
            self.soundids.Add(Sounds.StartSound(() => self.position, Sounds.ShortRattle));
        }
    }

    class DodgeBehavior : BehaviorComponent
    {
        const float SPEED = 3.0f;
        public override void Draw()
        {
            Util.DrawSprite(Textures.DodgeEnemy, self.position, 0, 1.0f);
        }
        public override void Update(float dt)
        {
            foreach (var actor in Util.Actors.ActorsNear(self.position, 2.5f))
            {
                if (actor == self) continue;
                Vector2 diffdir = self.position - actor.Position;
                if (diffdir.LengthSquared() == 0)
                    diffdir = new Vector2(Util.RandRange(-1, 1), Util.RandRange(-1, 1));
                diffdir.Normalize();
                if (actor == self.target) diffdir = -diffdir;
                self.position += SPEED * dt * diffdir;
            }
            if (self.position.X - 2.5f < -Util.FIELDWIDTH / 2) { self.position.X += SPEED * dt; }
            if (self.position.X + 2.5f > Util.FIELDWIDTH / 2) { self.position.X -= SPEED * dt; }
            if (self.position.Y - 2.5f < -Util.FIELDHEIGHT / 2) { self.position.Y += SPEED * dt; }
            if (self.position.Y + 2.5f > Util.FIELDHEIGHT / 2) { self.position.Y -= SPEED * dt; }
        }
        public override BehaviorComponent Clone()
        {
            return new DodgeBehavior();
        }
        public override void Start()
        {
            self.soundids.Add(Sounds.StartSound(() => self.position, Sounds.LightCym));
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
        public override void Start()
        {
            self.soundids.Add(Sounds.StartSound(() => self.position, Sounds.Bongos));
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
        public override void Start()
        {
            self.soundids.Add(Sounds.StartSound(() => self.position, Sounds.Kettle));
        }
    }

    class ProjectorSeeker : SeekerComponent
    {
        float countdown = 3.0f;
        public override void Draw()
        {
            Util.DrawSprite(Textures.ProjectorEnemy, self.position, 0, 1.0f);
        }
        public override void Update(float dt)
        {
            countdown -= dt;
            if (countdown <= 0)
            {
                countdown += 3.0f;
                Vector2 target = self.position;
                Enemy newenemy = self.Clone();
                newenemy.position = target;
                newenemy.Seeker = new SlinkTowardSeeker();
                newenemy.Seeker.Reassign(newenemy);
                Util.Actors.Add(newenemy);
            }
        }
        public override SeekerComponent Clone()
        {
            return new ProjectorSeeker();
        }
        public override void Start()
        {
            self.soundids.Add(Sounds.StartSound(() => self.position, Sounds.Tabla));
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
            if (bullet == null || bullet.Dead) return;
            bullet.Die();
            self.Die();
        }
        public override void Die()
        {
            self.dead = true;
            Enemy child1 = self.Clone(); child1.Damage = null;
            Enemy child2 = self.Clone(); child2.Damage = null;
            Enemy child3 = self.Clone(); child3.Damage = null;
            child1.position.X += Util.RandRange(-1, 1);
            child1.position.Y += Util.RandRange(-1, 1);
            child1.fadeIn = 0;
            child2.position.X += Util.RandRange(-1, 1);
            child2.position.Y += Util.RandRange(-1, 1);
            child2.fadeIn = 0;
            child3.position.X += Util.RandRange(-1, 1);
            child3.position.Y += Util.RandRange(-1, 1);
            child3.fadeIn = 0;
            Util.Actors.Add(child1);
            Util.Actors.Add(child2);
            Util.Actors.Add(child3);
            Util.RandomExplosion(self.position);
            Util.EnemyDeath(self.position);
        }
        public override DamageComponent Clone()
        {
            return new SplittyDamage();
        }
        public override void Start()
        {
            self.soundids.Add(Sounds.StartSound(() => self.position, Sounds.Clave));
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
            if (bullet == null || bullet.Dead) return;

            bullet.Die();
            hitPoints--;
            if (hitPoints <= 0)
            {
                self.Die();
            }
        }
        public override void Die()
        {
            self.dead = true;
            Util.EnemyDeath(self.position);
            Util.RandomExplosion(self.position);
        }
        public override DamageComponent Clone()
        {
            return new ToughDamage();  // keep damage?
        }
        public override void Start()
        {
            self.soundids.Add(Sounds.StartSound(() => self.position, Sounds.Conga));
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
            if (enemy != null && enemy.Damage is MineDamage) return;
            self.Die();
        }
        public override void Die()
        {
            self.dead = true;
            Util.EnemyDeath(self.position);
            Util.Explosions.AddExplosion(self.position, new Vector3(1.0f, 0.5f, 0.0f), 300, 8, 2.0f, 0.4f);
            List<Actor> deaths = new List<Actor>(Util.Actors.ActorsNear(self.position, 2.5f).Where(a => a != self && !(a is PowerUp)));
            Util.Scheduler.Enqueue(0.1f, () =>
            {
                foreach (var a in deaths)
                {
                    a.Die();
                }
            });
        }
        public override DamageComponent Clone()
        {
            return new MineDamage();
        }
        public override void Start()
        {
            self.soundids.Add(Sounds.StartSound(() => self.position, Sounds.Woodblock));
        }
    }
}
