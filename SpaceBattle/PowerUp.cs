using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SpaceBattle
{
    class PowerUp : Actor
    {
        Texture2D texture;
        Vector2 position;
        public override Vector2 Position { get { return position; } }
        public override float Radius { get { return 0.75f; } }
        bool dead = false;
        public override bool Dead { get { return dead; } }
        Action<PlayerShip> action;

        public PowerUp(Texture2D texture, Vector2 position, Action<PlayerShip> action)
        {
            this.texture = texture;
            this.position = position;
            this.action = action;
        }

        public override void Draw()
        {
            Util.DrawSprite(texture, position, 0, 0.75f);
        }

        public override void Collision(Actor other)
        {
            PlayerShip ship = other as PlayerShip;
            if (ship != null)
            {
                dead = true;
                action(ship);
            }
        }
    }

    class ComponentFactory<C>
    {
        public ComponentFactory(float cost, Func<C> create, Action<Vector2> draw) {
            Create = create;
            Draw = draw;
            Cost = cost;
        }
        public ComponentFactory(float cost, Func<C> create, Texture2D tex)
        {
            Create = create;
            Draw = pos => Util.DrawSprite(tex, pos, 0, 1.0f);
            Cost = cost;
        }
        public readonly Func<C> Create;
        public readonly Action<Vector2> Draw;
        public readonly float Cost;
    }

    static class PowerUps
    {
        public static PowerUp RandomPowerup(Vector2 position)
        {
            var behavior = Switch<BehaviorComponent>(
                new ComponentFactory<BehaviorComponent>(1, () => new TwirlyBehavior(), Textures.TwirlyEnemy));
            var seeker = Switch(
                new ComponentFactory<SeekerComponent>(2, () => new SlinkTowardSeeker(), Textures.FollowerEnemy));
            var damage = Switch(
                new ComponentFactory<DamageComponent>(2, () => new SplittyDamage(), Textures.SplittyEnemy),
                new ComponentFactory<DamageComponent>(2, () => new ToughDamage(), Textures.StrongEnemy));

            float cost = behavior.Cost + seeker.Cost + damage.Cost;
            float refresh = cost / 4.0f;

            return new PowerUp(Textures.RandomPowerup, position, ship =>
                    ship.Equip(new EnemyFactory(pos => { behavior.Draw(pos); seeker.Draw(pos); damage.Draw(pos); }, 5, refresh, 0.3f, (pos, target) =>
                        Guard(8.0f, target, new Enemy(pos, target, behavior.Create(), seeker.Create(), damage.Create())))));
        }

        static ComponentFactory<T> Switch<T>(params ComponentFactory<T>[] ps) where T:class
        {
            int pick = Util.RANDOM.Next(ps.Count() + 1);
            if (pick == 0) { return new ComponentFactory<T>(0, () => null, v => { }); }
            else { return ps[pick - 1]; }
        }

        static Enemy Guard(float radius, Actor target, Enemy e)
        {
            if ((e.Position - target.Position).Length() > radius)
            {
                return e;
            }
            else
            {
                return null;
            }
        }
    };
}
