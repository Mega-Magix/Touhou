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
using IronPython.Hosting;

namespace Touhou
{

    public class Touhou : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        KeyboardState keystate;

        Battle.Level level;

        double primaryAspectRatio;

        double scalingRatio;

        RenderTarget2D target;
        SpriteBatch targetBatch;
        Rectangle targetRectangle;

        Rectangle screenRectangle;

        public int viewportWidth;
        public int viewportHeight;

        public int gameWidth = 640;
        public int gameHeight = 480;

        private bool fullscreen;

        public Touhou(bool fullscreen, double aspectRatio)
        {
            DisplayMode displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;

            primaryAspectRatio = aspectRatio;

            this.IsFixedTimeStep = false;
            this.fullscreen = fullscreen;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        public Touhou() : this(false, 1.0d) { }

        protected override void Initialize()
        {
            base.Initialize();

            if (fullscreen == true)
            {
                List<int> optimalHeights = new List<int>();

                // Find the best full screen resolution to use to maintain a consistant aspect ratio
                GraphicsAdapter e = graphics.GraphicsDevice.Adapter;
                foreach (DisplayMode c in e.SupportedDisplayModes)
                {
                    int width = c.Width;
                    int height = c.Height;
                    double aspectRatio = (double)width / (double)height;
                    if (aspectRatio == primaryAspectRatio)
                    {
                        optimalHeights.Add(height);
                    }
                }

                // Our preferred height at a consistant aspect ratio would be 720.
                // Let's try and get as close to that as possible
                int minimalValue = -1;
                int heightValue = 0;
                int minimalIndex = -1;
                for (int i = 0; i < optimalHeights.Count; i++)
                {
                    int diff = Math.Abs(optimalHeights[i] - 480);
                    if (minimalValue < 0)
                    {
                        minimalValue = diff;
                        minimalIndex = i;
                        heightValue = optimalHeights[i];
                    }
                    else
                    {
                        if (diff < minimalValue)
                        {
                            minimalValue = diff;
                            minimalIndex = i;
                            heightValue = optimalHeights[i];
                        }
                    }
                }

                graphics.PreferredBackBufferWidth = (int)(heightValue * primaryAspectRatio);
                graphics.PreferredBackBufferHeight = heightValue;
                graphics.IsFullScreen = true;
                graphics.SynchronizeWithVerticalRetrace = false;

                graphics.ApplyChanges();

                // Our new height will MATCH the screen resolution now
                // Our width will be at that screen resolution height adjusted to the aspect ratio
                viewportHeight = heightValue;
                viewportWidth = (int)((4.0d / 3.0d) * (double)heightValue);
                int screenWidth = graphics.PreferredBackBufferWidth;
                int screenHeight = graphics.PreferredBackBufferHeight;
                int viewportX = (screenWidth - viewportWidth) / 2;
                int viewportY = (screenHeight - viewportHeight) / 2;

                graphics.ApplyChanges();

                scalingRatio = (double)heightValue / (double)gameHeight;

                targetRectangle = new Rectangle(viewportX, viewportY, viewportWidth, viewportHeight);
                screenRectangle = new Rectangle(0, 0, screenWidth, screenHeight);
            }
            else
            {
                graphics.PreferredBackBufferWidth = gameWidth;
                graphics.PreferredBackBufferHeight = gameHeight;
                graphics.ApplyChanges();
                targetRectangle = new Rectangle(0, 0, gameWidth, gameHeight);
                screenRectangle = new Rectangle(0, 0, gameWidth, gameHeight);
            }

            target = new RenderTarget2D(GraphicsDevice, gameWidth, gameHeight);
            targetBatch = new SpriteBatch(GraphicsDevice);
            level = new Battle.Level(this);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            keystate = Keyboard.GetState();

            level.Update(gameTime, keystate);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(target);

            level.Draw();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            targetBatch.Begin();
            targetBatch.Draw(target, targetRectangle, Color.White);
            targetBatch.End();

            base.Draw(gameTime);
        }
    }

    public class SoundManager
    {
        SoundEffectInstance instance1;
        SoundEffectInstance instance2;
        int current;
        public bool isPlaying;
        public SoundManager(SoundEffect s)
        {
            instance1 = s.CreateInstance();
            instance2 = s.CreateInstance();
        }
        public void Play()
        {
            if (!isPlaying)
            {
                isPlaying = true;
                if (current == 1)
                { current = 2; instance1.Stop(); instance2.Play(); }
                else
                { current = 1; instance2.Stop(); instance1.Play(); }
            }
        }
    }
}
