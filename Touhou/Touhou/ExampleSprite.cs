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

        //Lists for player bullets and enemy bullets
        List<Bullet> pBullets = new List<Bullet>();
        List<Bullet> eBullets = new List<Bullet>();

        //Player starts at (0,0)
        Vector2 playerPosition = Vector2.Zero;

        //Arrays and lists for storing texture and animation data
        string[] texFiles = {"reimufly","reimumoveleft","reimuleft","reimumoveright","reimuright"};
        List<Texture2D> textures = new List<Texture2D>();
        int[] playerTexFrames = { 4, 3, 4, 3, 4 };
        double[] playerTexSpeeds = { 0.15, 0.07, 0.15, 0.07, 0.15 };
        List<AnimatedTexture> reimuTextures = new List<AnimatedTexture>();
        AnimatedTexture playerTexture;
        Texture2D testReimu;
        Texture2D bulletTexture;

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //Load all textures
            for (int i = 0; i < texFiles.Length; i++)
                textures.Add(Content.Load<Texture2D>(texFiles[i]));
            //Create Reimu animations
            for (int i = 0; i < 5; i++)
                reimuTextures.Add(new AnimatedTexture(textures[i], playerTexFrames[i], playerTexSpeeds[i]));
            //Starting Reimu texture
            playerTexture = reimuTextures[0];
            //Load test Reimu texture
            testReimu = Content.Load<Texture2D>("reimu");
            //Load test bullet texture
            bulletTexture = Content.Load<Texture2D>("bullet1");
        }

        protected override void UnloadContent()
        {

        }
        //State of keyboard
        KeyboardState keystate;
        //Player and bullet data
        Vector2 spriteSpeed;
        float bulletSpeed = 500.0f;
        float fireangle = 0.0f;
        double firedelay = 0.2;


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            //Get keyboard state
            keystate = Keyboard.GetState();
            //Read keyboard input
            spriteSpeed = Vector2.Zero;
            if (firedelay >= 0) firedelay -= gameTime.ElapsedGameTime.TotalSeconds;
            if (keystate.IsKeyDown(Keys.Left)) spriteSpeed.X -= 100;
            if (keystate.IsKeyDown(Keys.Right)) spriteSpeed.X += 100;
            if (keystate.IsKeyDown(Keys.Up)) spriteSpeed.Y -= 100;
            if (keystate.IsKeyDown(Keys.Down)) spriteSpeed.Y += 100;
            if (keystate.IsKeyDown(Keys.Z) && firedelay < 0)
            {
                //Find offset to put bullet at center of sprite
                int offsetX = (testReimu.Width - bulletTexture.Width) / 2;
                int offsetY = (testReimu.Height - bulletTexture.Height) / 2;
                //Shoot bullets at fixed rate when Z is pressed
                pBullets.Add(new Bullet(bulletTexture, 
                    new Vector2(playerPosition.X + offsetX, playerPosition.Y + offsetY),
                    fireangle, bulletSpeed));
                firedelay += 0.2;
            }
            //Angle testing controls
            if (keystate.IsKeyDown(Keys.X)) fireangle -= 1.0f;
            if (keystate.IsKeyDown(Keys.C)) fireangle += 1.0f;

            this.UpdateSprite(gameTime);

            base.Update(gameTime);
        }

        double dt;


        void UpdateSprite(GameTime gameTime)
        {
            //Get elapsed time
            dt = gameTime.ElapsedGameTime.TotalSeconds;
            // Move the sprite by speed, scaled by elapsed time.
            playerPosition += spriteSpeed * (float)dt;
            //Set screen boundaries
            int width = graphics.GraphicsDevice.Viewport.Width;
            int height = graphics.GraphicsDevice.Viewport.Height;
            int MaxX = width - testReimu.Width;
            int MinX = 0;
            int MaxY = height - testReimu.Height;
            int MinY = 0;
            // Check for edges.
            playerPosition.X = MathHelper.Clamp(playerPosition.X, MinX, MaxX);
            playerPosition.Y = MathHelper.Clamp(playerPosition.Y, MinY, MaxY);
        }


        protected override void Draw(GameTime gameTime)
        {
            //Window data
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            int width = graphics.GraphicsDevice.Viewport.Width;
            int height = graphics.GraphicsDevice.Viewport.Height;

            // Begin drawing sprites
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            for (int i = 0; i < pBullets.Count; i++)
            {
                //Move and draw bullets
                Bullet b = pBullets[i];
                b.move(dt);
                spriteBatch.Draw(bulletTexture, b.pos, Color.White);
                //Remove off-screen bullets
                if (b.pos.X < 0 || b.pos.X > width || b.pos.Y < 0 || b.pos.Y > height)
                {
                    pBullets.RemoveAt(i); i--;
                }
            }
            //Determine animations based on movement and current animation
            if (spriteSpeed.X < 0)
            {
                if (playerTexture != reimuTextures[1] && playerTexture != reimuTextures[2])
                    playerTexture = reimuTextures[1];
                if (playerTexture == reimuTextures[1] && playerTexture.willFinish(dt))
                    playerTexture = reimuTextures[2];
            }
            if (spriteSpeed.X > 0)
            {
                if (playerTexture != reimuTextures[3] && playerTexture != reimuTextures[4])
                    playerTexture = reimuTextures[3];
                if (playerTexture.willFinish(dt) && playerTexture == reimuTextures[3])
                    playerTexture = reimuTextures[4];
            }
            if (spriteSpeed.X == 0) playerTexture = reimuTextures[0];
            
            //Draw player
            spriteBatch.Draw(playerTexture.img, playerPosition, playerTexture.getFrame(dt), Color.White);
            //End drawing sprites
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
            //Constructor given angular direction
            public Bullet(Texture2D t, Vector2 p, float a, float s)
            {
                img = t; pos = p; angle = a-90.0f; speed = s;
                radians = MathHelper.ToRadians(angle);
                dir = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
            }
            //Constructor given vector direction
            public Bullet(Texture2D t, Vector2 p, Vector2 d)
            {
                img = t; pos = p; dir = d;
                angle = MathHelper.ToDegrees((float)Math.Atan2(dir.Y, dir.X))+90.0f;
                speed = (float)Math.Sqrt(d.X * d.X + d.Y * d.Y);
            }
            //Move the bullet using class data
            public void move(double dt)
            {
                pos.X += dir.X * speed * (float)dt;
                pos.Y += dir.Y * speed * (float)dt;
            }

        }



        public class AnimatedTexture
        {
            public Texture2D img;
            public int frames;
            public int frame = 0;
            public double speed;
            public double delay;
            //Constructor taking texture file, number of frames, and frame rate
            public AnimatedTexture(Texture2D t, int f, double s)
            {
                img = t; frames = f; delay = speed = s;
            }
            //Gets a rectangle representing the frame to be drawn. Needs to be called each update
            public Rectangle getFrame(double dt)
            {
                delay -= dt;
                if (delay < 0.0)
                {
                    delay += speed; frame++;
                    if (frame >= frames) frame = 0;
                }
                return new Rectangle(img.Width * frame / frames, 0, img.Width / frames, img.Height);
            }
            //Detects if an animation is about to finish.
            public bool willFinish(double dt)
            {
                bool willfinish = (delay - dt < 0 && frame + 1 == frames);
                if (willfinish) { frame = 0; delay += speed; }
                return willfinish;
            }
        }
    }
}