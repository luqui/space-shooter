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
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SpaceBattle : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        PlayerShip player1;
        PlayerShip player2;
        Matrix transform;

        public SpaceBattle()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
            
            Util.Device = GraphicsDevice;
            float w = GraphicsDevice.Viewport.Width;
            float h = GraphicsDevice.Viewport.Height;
            transform = Matrix.Identity;
            transform *= Matrix.CreateScale(1/8.0f, 1/6.0f, 1.0f);
            transform *= Matrix.CreateTranslation(1f, 1f, 0.0f);
            transform *= Matrix.CreateScale(w / 2, h / 2, 1.0f);
            //transform *= Matrix.CreateScale(2, 2, 1);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Util.Batch = spriteBatch;
            Util.Actors = new List<Actor>();
            player1 = new PlayerShip(PlayerIndex.One);
            player2 = new PlayerShip(PlayerIndex.Two);
            player1.LoadContent(Content);
            player2.LoadContent(Content);
            Bullet.LoadContent(Content);
            FollowerEnemy.LoadContent(Content);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            player1.Update(dt);
            player2.Update(dt);
            foreach (var b in Util.Actors) { b.Update(dt); }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteBlendMode.Additive, SpriteSortMode.Immediate, SaveStateMode.SaveState, transform);
            player1.Draw();
            player2.Draw();
            foreach (var b in Util.Actors) { b.Draw(); }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
