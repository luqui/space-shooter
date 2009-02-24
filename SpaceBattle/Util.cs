using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceBattle
{
    static class Util
    {
        public static GraphicsDevice Device;
        public static SpriteBatch Batch;
        public static List<Actor> Actors;

        public static void DrawSprite(Texture2D tex, Vector2 pos, float rot, float scale) {
            float sc = 1.0f / tex.Width;
            pos.Y = -pos.Y;
            Batch.Draw(tex, pos, null, Color.White, -rot, new Vector2(tex.Width / 2, tex.Height / 2), sc*scale, SpriteEffects.None, 0);
        }
    }
}
