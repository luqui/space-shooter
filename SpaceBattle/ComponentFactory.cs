using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SpaceBattle
{
    abstract class ComponentFactory
    {
        public abstract string Name { get; }
        public abstract void Draw(Vector2 pos);
    }

    class ComponentFactory<T> : ComponentFactory where T : Component
    {
        public readonly Func<T> Spawn;
        public readonly Action<Vector2> draw;
        public readonly string name;

        public override string Name { get { return name; } }
        public override void Draw(Vector2 pos) { draw(pos); }

        public ComponentFactory(string name, Func<T> spawn, Action<Vector2> draw)
        {
            this.name = name;
            Spawn = spawn;
            this.draw = draw;
        }
    }

    class ComponentRing<T> where T : Component
    {
        class Entry
        {
            public Entry(ComponentFactory<T> f, int a) { factory = f; ammo = a; }
            public ComponentFactory<T> factory;
            public int ammo;
        };

        List<Entry> buf;
        int index;

        public int Index
        {
            get { return index; }
            set
            {
                if (value < 0 || value >= buf.Count) throw new Exception("Index out of range!");
                index = value;
            }
        }

        public ComponentRing(IEnumerable<ComponentFactory<T>> contents)
        {
            buf = new List<Entry>(contents.Select(f => new Entry(f, 10)));
            index = 0;
        }

        public bool Empty()
        {
            return buf.All(b => b.ammo == 0);
        }

        public void Next()
        {
            index++;
            while (index >= buf.Count) index -= buf.Count;
        }

        public void Prev()
        {
            index--;
            while (index < 0) index += buf.Count;
        }

        public T Spawn()
        {
            if (buf[index].ammo > 0)
            {
                buf[index].ammo--;
                return buf[index].factory.Spawn();
            }
            else
            {
                return null;
            }
        }

        public int Ammo { get { return buf[index].ammo; } }

        public void Draw(Vector2 pos, Vector2 stride)
        {
            Vector2 offset = pos;
            for (int i = 0; i < buf.Count; i++)
            {
                int j = (i + index) % buf.Count;
                if (buf[j].ammo == 0) continue;
                buf[j].factory.Draw(offset);
                Util.DrawText(offset + new Vector2(1,0), buf[j].ammo.ToString());
                offset += stride;
            }
        }

        public void Add(string id, int amount)
        {
            foreach (var s in buf)
            {
                if (s.factory.Name == id)
                {
                    s.ammo += amount;
                    if (s.ammo > 99) s.ammo = 99;
                    break;
                }
            }
        }
    }
}
