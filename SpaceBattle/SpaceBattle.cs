using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace SpaceBattle
{
    public class SpaceBattle : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Matrix transform;

        public SpaceBattle()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
#if !DEBUG
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
#endif
        }

        protected override void Initialize()
        {
            base.Initialize();
            Util.Device = GraphicsDevice;
            float w = GraphicsDevice.Viewport.Width;
            float h = GraphicsDevice.Viewport.Height;
            transform = Matrix.Identity;
            transform *= Matrix.CreateScale(2/Util.FIELDWIDTH, 2/Util.FIELDHEIGHT, 1.0f);
            transform *= Matrix.CreateTranslation(1f, 1f, 0.0f);
            transform *= Matrix.CreateScale(w / 2, h / 2, 1.0f);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Util.Batch = spriteBatch;
            Textures.LoadContent(Content);
            Util.Sequencer = new Sequencer();
            Util.Sequencer.Start();

            Util.player1 = new PlayerShip(PlayerIndex.One);
            Util.player2 = new PlayerShip(PlayerIndex.Two);

            Util.Actors = new ActorList();
            Util.Actors.Add(Util.player1);
            Util.Actors.Add(Util.player2);
            Util.Reset();
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Util.Actors.Update(dt);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteBlendMode.Additive, SpriteSortMode.Immediate, SaveStateMode.SaveState, transform);
            Util.Actors.Draw();
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
