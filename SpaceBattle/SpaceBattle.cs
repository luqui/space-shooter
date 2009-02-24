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

        const float POWERUPTIMEOUT = 30.0f;
        float powerupTimer = 10.0f;

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
            transform *= Matrix.CreateScale(2/Util.FIELDWIDTH, 2/Util.FIELDHEIGHT, 1.0f);
            transform *= Matrix.CreateTranslation(1f, 1f, 0.0f);
            transform *= Matrix.CreateScale(w / 2, h / 2, 1.0f);
            //transform *= Matrix.CreateScale(2, 2, 1);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Util.Batch = spriteBatch;
            Util.Actors = new ActorList();
            Util.player1 = new PlayerShip(PlayerIndex.One);
            Util.player2 = new PlayerShip(PlayerIndex.Two);
            Util.player1.LoadContent(Content);
            Util.player2.LoadContent(Content);
            PowerUps.LoadContent(Content);
            Util.Actors.Add(Util.player1);
            Util.Actors.Add(Util.player2);
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
            Util.Actors.Update(dt);

            powerupTimer -= dt;
            if (powerupTimer <= 0)
            {
                Util.Actors.Add(PowerUps.RandomPowerup(Util.RandomPosition()));
                powerupTimer = POWERUPTIMEOUT;
            }

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
