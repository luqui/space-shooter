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
        public Enemy(Vector2 pos, PlayerShip targ,
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

        public const float FADEINTIME = 2.0f;
        public float dtUpgrade = 45;
        float upgrade = 0;

        // only for use in components:
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 accel;
        public bool dead = false;
        public PlayerShip target = null;

        public BehaviorComponent Behavior = null;
        public SeekerComponent Seeker = null;
        public DamageComponent Damage = null;
        public float fadeIn = FADEINTIME;

        public List<SoundID> soundids = new List<SoundID>();

        public Enemy Clone()
        {
            Enemy e = new Enemy(position, target, 
                Behavior == null ? null : Behavior.Clone(), 
                Seeker == null ? null : Seeker.Clone(), 
                Damage == null ? null : Damage.Clone());
            return e;
        }

        public override void Start()
        {
            if (Behavior != null) { Behavior.Start(); }
            if (Seeker != null) { Seeker.Start(); }
            if (Damage != null) { Damage.Start(); }
            if (Behavior == null && Seeker == null && Damage == null) { Util.BLANKS++; }
        }

        public override void Finish()
        {
            foreach (var id in soundids) { Util.Sequencer.Dequeue(id); }
            if (Behavior != null) { Behavior.Finish(); }
            if (Seeker != null) { Seeker.Finish(); }
            if (Damage != null) { Damage.Finish(); }
            if (Behavior == null && Seeker == null && Damage == null) { Util.BLANKS--; }
        }

        public override void Draw()
        {
            float prealpha = Util.AlphaHack;
            if (fadeIn > 0) { Util.AlphaHack = 1 - fadeIn / FADEINTIME; }
            if (Behavior == null && Seeker == null && Damage == null)
            {
                if (upgrade > dtUpgrade - 5)
                {
                    Util.DrawSprite(Textures.EmptyEnemy3, position + new Vector2(Util.RandRange(-0.05f, 0.05f), Util.RandRange(-0.05f, 0.05f)), 0, 1.0f);
                }
                else if (upgrade > dtUpgrade - 10)
                {
                    Util.DrawSprite(Textures.EmptyEnemy2, position, 0, 1.0f);
                }
                else
                {
                    Util.DrawSprite(Textures.EmptyEnemy, position, 0, 1.0f);
                }
            }
            else
            {
                if (Behavior != null) Behavior.Draw();
                if (Seeker != null) Seeker.Draw();
                if (Damage != null) Damage.Draw();
            }
            Util.AlphaHack = prealpha;
        }

        public override void Update(float dt)
        {
            if (fadeIn > 0 && fadeIn - dt <= 0)
            {
                Util.Explosions.AddExplosion(position, new Vector3(0, 0.5f, 1), 25, 5, 0.5f, 0.3f);
            }
            
            fadeIn -= dt;
            if (fadeIn > 0) return;
            if (Behavior != null) Behavior.Update(dt);
            if (Seeker != null) Seeker.Update(dt); 
            velocity += dt * accel;
            position += dt * velocity;
            accel = new Vector2();

            upgrade += dt;
            if (upgrade >= dtUpgrade)
            {
                upgrade -= dtUpgrade;
                List<ComponentFactory> factories = new List<ComponentFactory>();
                factories.AddRange(Components.Behaviors.Cast<ComponentFactory>().Where(b => b.Name != "Empty"));
                factories.AddRange(Components.Seekers.Cast<ComponentFactory>().Where(b => b.Name != "Empty"));
                factories.AddRange(Components.Damages.Cast<ComponentFactory>().Where(b => b.Name != "Empty"));
                ComponentFactory f = factories[Util.RANDOM.Next(factories.Count)];
                if (Behavior == null && f is ComponentFactory<BehaviorComponent>) { Behavior = ((ComponentFactory<BehaviorComponent>)f).Spawn(); Behavior.Reassign(this); }
                if (Seeker == null && f is ComponentFactory<SeekerComponent>) { Seeker = ((ComponentFactory<SeekerComponent>)f).Spawn(); Seeker.Reassign(this); }
                if (Damage == null && f is ComponentFactory<DamageComponent>) { Damage = ((ComponentFactory<DamageComponent>)f).Spawn(); Damage.Reassign(this); }
            }
        }

        public override bool Dead { get { return dead; } }
        public override Vector2 Position { get { return position; } }
        public override float Radius { get { return 0.3f; } }

        public override void Collision(Actor other)
        {
            if (dead) return;
            if (Damage != null)
            {
                if (fadeIn > 0) return;
                Damage.OnHit(other);
            }
            else {
                if (fadeIn > 1) return;
                Bullet b = other as Bullet;
                if (b != null && !b.Dead) {
                    b.Die();
                    Die();
                }
            }
        }

        public override void Die()
        {
            if (dead) return;
            if (Behavior != null || Seeker != null || Damage != null) Util.SCORE++;
            if (Damage != null)
            {
                Damage.Die();
            }
            else
            {
                dead = true;
                Util.RandomExplosion(position);
                Util.EnemyDeath(position);
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
}
