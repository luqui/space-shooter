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
        Menu menu;

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
            Util.WorldToScreen = transform;

            Util.Sequencer = new Sequencer();
            Util.Sequencer.Start();
            Util.Scheduler = new Scheduler();
            Util.Explosions = new Explosion();

            Util.player1 = new PlayerShip(PlayerIndex.One, new XBoxInput(PlayerIndex.One));
            Util.player2 = new PlayerShip(PlayerIndex.Two, new XBoxInput(PlayerIndex.Two));
            
            Util.Actors = new ActorList(8,6);

            Util.MODE = Util.Mode.Menu;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Util.Batch = spriteBatch;
            Textures.LoadContent(Content);
            menu = new Menu(Content.Load<SpriteFont>("MenuFont"));
        }

        protected override void UnloadContent()
        {
        }

        float emptyTimer = 0.0f;
        float scoreTimer = 0.0f;

        protected override void Update(GameTime gameTime)
        {
            //no matter what, escape == exit.
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Util.MODE = Util.Mode.Exit;

            float dt = Util.Paused ? 0 : (float)gameTime.ElapsedGameTime.TotalSeconds;
            Util.Actors.Update(dt);
            Util.Scheduler.Update(dt);
            Util.Explosions.Update(dt);

            switch(Util.MODE)
            {
                case Util.Mode.OnePlayer:
                    emptyTimer -= dt;
                    while (emptyTimer < 0)
                    {
                        emptyTimer += 0.18f;
                        Util.player1.Equip("Empty", 1);
                    }

                    scoreTimer -= dt;
                    while (Util.player1.Lives > 0 && scoreTimer < 0)
                    {
                        scoreTimer += 0.5f;
                        Util.SCORE--;
                    }
                    break;
                case Util.Mode.TwoPlayer:
                    emptyTimer -= dt;
                    while (emptyTimer < 0)
                    {
                        emptyTimer += 0.36f;
                        Util.player1.Equip("Empty", 1);
                        Util.player2.Equip("Empty", 1);
                    }
                    break;
                case Util.Mode.Menu:
                    menu.Update(new GamePadState[] {GamePad.GetState(PlayerIndex.One),GamePad.GetState(PlayerIndex.Two),GamePad.GetState(PlayerIndex.Three),GamePad.GetState(PlayerIndex.Four)});
                    break;
                case Util.Mode.Exit:
                    Exit();
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            switch (Util.MODE)
            {
                case Util.Mode.Menu:
                    spriteBatch.Begin();
                    var vp = graphics.GraphicsDevice.Viewport;
                    menu.Draw(spriteBatch, new Rectangle(vp.X,vp.Y,vp.Width,vp.Height));
                    spriteBatch.End();
                    break;
                case Util.Mode.OnePlayer:
                case Util.Mode.TwoPlayer:
                    spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState, transform);
                    Util.Actors.Draw();
                    Util.Explosions.Draw();
                    Util.player1.DrawStatus();
                    if (Util.MODE == Util.Mode.OnePlayer)
                    {
                        Util.DrawText(new Vector2(0, Util.FIELDHEIGHT / 2 - 1), Util.SCORE.ToString());
                    }
                    else
                    {
                        Util.player2.DrawStatus();
                    }
                    spriteBatch.End();
                    break;
            }
            base.Draw(gameTime);
        }
    }
}
