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
        Vector2 bulletSpeed = new Vector2(0.0f,-10.0f);

        double firedelay = 0.2;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            keystate = Keyboard.GetState();
            spriteSpeed = Vector2.Zero;
            if (firedelay >= 0) { firedelay -= gameTime.ElapsedGameTime.TotalSeconds; }
            if (keystate.IsKeyDown(Keys.Left)) {spriteSpeed.X = -100;}
            if (keystate.IsKeyDown(Keys.Right)) {spriteSpeed.X = 100;}
            if (keystate.IsKeyDown(Keys.Up)) {spriteSpeed.Y = -100;}
            if (keystate.IsKeyDown(Keys.Down)) {spriteSpeed.Y = 100;}
            if (keystate.IsKeyDown(Keys.Z) && firedelay < 0)
            {
                pBullets.Add(new Bullet(bulletTexture, playerPosition, bulletSpeed));
                firedelay += 0.2;
            }

            this.UpdateSprite(gameTime);

            base.Update(gameTime);
        }

        void UpdateSprite(GameTime gameTime)
        {
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
            if (playerPosition.X > MaxX) {playerPosition.X = MaxX;}
            else if (playerPosition.X < MinX) {playerPosition.X = MinX;}
            if (playerPosition.Y > MaxY) {playerPosition.Y = MaxY;}
            else if (playerPosition.Y < MinY) {playerPosition.Y = MinY;}
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);
            int width = graphics.GraphicsDevice.Viewport.Width;
            int height = graphics.GraphicsDevice.Viewport.Height;

            // Draw the sprite.
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            for (int i = 0; i < pBullets.Count; i++)
            {
                Bullet b = pBullets[i];
                b.pos.X += b.dir.X; b.pos.Y += b.dir.Y;
                spriteBatch.Draw(bulletTexture, b.pos, Color.White);
                if (b.pos.X < 0 || b.pos.X > width || b.pos.Y < 0 || b.pos.Y > height) {b = null;}
            }
            spriteBatch.Draw(playerTexture, playerPosition, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
    public class Bullet
    {
        public Texture2D img;
        public Vector2 pos;
        public Vector2 dir;
        public Bullet(Texture2D t, Vector2 p, Vector2 d)
        {
            img = t;
            pos = p;
            dir = d;

        }
    }
}