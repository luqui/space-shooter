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
            var result = System.Windows.Forms.MessageBox.Show("Two player mode?", "Mode selection", System.Windows.Forms.MessageBoxButtons.YesNo);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                Util.MODE = Util.Mode.TwoPlayer;
            }
            else
            {
                Util.MODE = Util.Mode.OnePlayer;
            }
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
            Util.Scheduler = new Scheduler();

            Util.player1 = new PlayerShip(PlayerIndex.One);
            Util.player2 = new PlayerShip(PlayerIndex.Two);

            Util.Actors = new ActorList();
            Util.Actors.Add(Util.player1);
            if (Util.MODE == Util.Mode.TwoPlayer) { Util.Actors.Add(Util.player2); }
            Util.Reset();
        }

        protected override void UnloadContent()
        {
        }

        float emptyTimer = 0.0f;
        float scoreTimer = 10.0f;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
             || GamePad.GetState(PlayerIndex.Two).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Util.Actors.Update(dt);
            Util.Scheduler.Update(dt);

            if (Util.MODE == Util.Mode.OnePlayer)
            {
                emptyTimer -= dt;
                while (emptyTimer < 0)
                {
                    emptyTimer += 0.25f;
                    Util.player1.Equip("Empty", 1);
                }

                scoreTimer -= dt;
                while (!Util.player1.Dead && scoreTimer < 0)
                {
                    scoreTimer += 0.5f;
                    Util.SCORE--;
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteBlendMode.Additive, SpriteSortMode.Immediate, SaveStateMode.SaveState, transform);
            Util.Actors.Draw();
            if (Util.MODE == Util.Mode.OnePlayer)
            {
                Util.DrawText(new Vector2(0, Util.FIELDHEIGHT / 2-1), Util.SCORE.ToString());
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
