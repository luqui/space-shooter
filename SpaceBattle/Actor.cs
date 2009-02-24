using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SpaceBattle
{
    abstract class Actor
    {
        public abstract Vector2 Position { get; }

        public virtual void Update(float dt) { }
        public virtual void Draw() { }
    }
}
