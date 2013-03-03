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

namespace Touhou
{

    public class Touhou : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        KeyboardState keystate;

        Battle.Level level;

        double primaryAspectRatio;

        public Touhou()
        {

            DisplayMode displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;

            primaryAspectRatio = (double)displayMode.Width / (double)displayMode.Height;

            this.IsFixedTimeStep = false;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();

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
                int diff = Math.Abs(optimalHeights[i] - 720);
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

            GraphicsDevice.Viewport = new Viewport(
                (GraphicsDevice.Viewport.Width - 640) / 2,
                (GraphicsDevice.Viewport.Height- 720) / 2,
                640, 720);

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
            GraphicsDevice.Clear(Color.Black);

            level.Draw();

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
        public void play()
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
