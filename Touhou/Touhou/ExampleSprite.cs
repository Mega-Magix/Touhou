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

namespace Touhou.ExampleSprite
{
    public class ExampleSprite : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public ExampleSprite()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();

        }

        Texture2D playerTexture;
        Texture2D bulletTexture;
        List<Bullet> pBullets = new List<Bullet>();
        List<Bullet> eBullets = new List<Bullet>();

        Vector2 playerPosition = Vector2.Zero;

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            playerTexture = Content.Load<Texture2D>("reimu");
            bulletTexture = Content.Load<Texture2D>("bullet1");
        }

        protected override void UnloadContent()
        {

        }

        KeyboardState keystate;
        Vector2 spriteSpeed;
        float bulletSpeed = 500.0f;
        float fireangle = 0.0f;

        double firedelay = 0.2;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            keystate = Keyboard.GetState();
            spriteSpeed = Vector2.Zero;
            if (firedelay >= 0) firedelay -= gameTime.ElapsedGameTime.TotalSeconds;
            if (keystate.IsKeyDown(Keys.Left)) spriteSpeed.X = -100;
            if (keystate.IsKeyDown(Keys.Right)) spriteSpeed.X = 100;
            if (keystate.IsKeyDown(Keys.Up)) spriteSpeed.Y = -100;
            if (keystate.IsKeyDown(Keys.Down)) spriteSpeed.Y = 100;
            if (keystate.IsKeyDown(Keys.Z) && firedelay < 0)
            {
                pBullets.Add(new Bullet(bulletTexture, playerPosition, fireangle, bulletSpeed));
                firedelay += 0.2;
            }
            if (keystate.IsKeyDown(Keys.X)) fireangle -= 1.0f;
            if (keystate.IsKeyDown(Keys.C)) fireangle += 1.0f;

            this.UpdateSprite(gameTime);

            base.Update(gameTime);
        }

        double dt;

        void UpdateSprite(GameTime gameTime)
        {
            dt = gameTime.ElapsedGameTime.TotalSeconds;
            // Move the sprite by speed, scaled by elapsed time.
            playerPosition +=
                spriteSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            int width = graphics.GraphicsDevice.Viewport.Width;
            int height = graphics.GraphicsDevice.Viewport.Height;
            int MaxX = width - playerTexture.Width;
            int MinX = 0;
            int MaxY = height - playerTexture.Height;
            int MinY = 0;

            // Check for edges.
            playerPosition.X = MathHelper.Clamp(playerPosition.X, MinX, MaxX);
            playerPosition.Y = MathHelper.Clamp(playerPosition.Y, MinY, MaxY);
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            int width = graphics.GraphicsDevice.Viewport.Width;
            int height = graphics.GraphicsDevice.Viewport.Height;

            // Draw the sprite.
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            for (int i = 0; i < pBullets.Count; i++)
            {
                Bullet b = pBullets[i];
                b.move(gameTime.ElapsedGameTime.TotalSeconds);
                spriteBatch.Draw(bulletTexture, b.pos, Color.White);
                if (b.pos.X < 0 || b.pos.X > width || b.pos.Y < 0 || b.pos.Y > height)
                {
                    pBullets.RemoveAt(i); i--;
                }
            }
            spriteBatch.Draw(playerTexture, playerPosition, Color.White);
            spriteBatch.End();
            base.Draw(gameTime);
        }
        public class Bullet
        {
            public Texture2D img;
            public Vector2 pos;
            public Vector2 dir;
            public float speed;
            public float angle;
            public float radians;
            public Bullet(Texture2D t, Vector2 p, float a, float s)
            {
                img = t;
                pos = p;
                angle = a-90.0f;
                speed = s;
                radians = MathHelper.ToRadians(angle);
                dir = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
            }
            public Bullet(Texture2D t, Vector2 p, Vector2 d)
            {
                img = t;
                pos = p;
                dir = d;
                angle = MathHelper.ToDegrees((float)Math.Atan2(dir.Y, dir.X))+90.0f;
                speed = (float)Math.Sqrt(d.X * d.X + d.Y * d.Y);
            }
            public void move(double dt)
            {
                pos.X += dir.X * speed * (float)dt;
                pos.Y += dir.Y * speed * (float)dt;
            }

        }
    }
}