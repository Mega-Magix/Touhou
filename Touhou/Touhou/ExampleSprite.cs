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
        List<Enemy> enemies = new List<Enemy>();

        //Arrays and lists for storing texture and animation data
        string[] texFiles = {"reimufly","reimumoveleft","reimuleft",
                            "enemy1fly","enemy1moveright","enemy1right",
                            "enemy2fly","enemy2moveright","enemy2right"};
        List<Texture2D> textures = new List<Texture2D>();
        int[] playerTexFrames = { 4, 3, 4 };
        double[] playerTexSpeeds = { 0.2, 0.1, 0.2 };
        int[] enemyTexFrames = { 4, 3, 1 };
        double[] enemyTexSpeeds = { 0.2, 0.1, 1 };
        List<AnimatedTexture> reimuTextures = new List<AnimatedTexture>();
        List<AnimatedTexture> enemyTextures = new List<AnimatedTexture>();
        List<Explosion> explosions = new List<Explosion>();
        AnimatedTexture playerTexture;
        Texture2D bulletTexture;
        SoundEffect playerShoot;
        Texture2D playerExplode;
        Texture2D enemyExplode;
        SoundEffect enemySound;
        SoundEffect deathSound;
        Song bgm;

        Random random = new Random();

        static int width; static int height;

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //Load all textures
            for (int i = 0; i < texFiles.Length; i++)
                textures.Add(Content.Load<Texture2D>(texFiles[i]));
            //Create Reimu animations
            for (int i = 0; i < 3; i++)
                reimuTextures.Add(new AnimatedTexture(textures[i], playerTexFrames[i], playerTexSpeeds[i]));
            //Create enemy animations
            for (int i = 0; i < 3; i++)
                enemyTextures.Add(new AnimatedTexture(textures[i+3], enemyTexFrames[i], enemyTexSpeeds[i]));
            //Starting Reimu texture
            playerTexture = reimuTextures[0];
            //Load player explosion texture
            playerExplode = Content.Load<Texture2D>("explode");
            //Load player death sound
            deathSound = Content.Load<SoundEffect>("death");
            //Load enemy explosion texture
            enemyExplode = Content.Load<Texture2D>("explodeblue");
            //Load enemy explosion sound
            enemySound = Content.Load<SoundEffect>("explodesound");
            //Load test bullet texture
            bulletTexture = Content.Load<Texture2D>("bullet1");
            //Load test bullet sound
            playerShoot = Content.Load<SoundEffect>("playershoot");
            //Load BGM and play it
            bgm = Content.Load<Song>("A Soul As Red As Ground Cherry");
            MediaPlayer.Play(bgm);
            //Set screen boundaries
            width = graphics.GraphicsDevice.Viewport.Width;
            height = graphics.GraphicsDevice.Viewport.Height;
            playerPosition = new Vector2(width, height) / 2;
        }

        protected override void UnloadContent()
        {

        }
        //State of keyboard
        KeyboardState keystate;
        //Player and bullet data
        Vector2 spriteSpeed = Vector2.Zero;
        Vector2 playerPosition;
        String playerStatus = "alive";
        float respawnDelay = 3.0f;
        float playerSpeed = 100.0f;
        float bulletSpeed = 750.0f;
        float fireangle = 0.0f;
        float firerate = 0.1f;
        float firedelay = 0.0f;

        double dt;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            //Get elapsed time
            dt = gameTime.ElapsedGameTime.TotalSeconds;

            //Read keyboard input
            this.readInput();
            //Spawn enemies randomly
            this.spawnEnemies(dt);
            //Move and draw sprites
            this.Draw(gameTime);


            base.Update(gameTime);
        }

        void readInput()
        {
            //Get keyboard state
            keystate = Keyboard.GetState();
            //Read keyboard input
            spriteSpeed = Vector2.Zero;
            if (firedelay >= 0) firedelay -= (float)dt;
            if (keystate.IsKeyDown(Keys.Left)) spriteSpeed.X -= playerSpeed;
            if (keystate.IsKeyDown(Keys.Right)) spriteSpeed.X += playerSpeed;
            if (keystate.IsKeyDown(Keys.Up)) spriteSpeed.Y -= playerSpeed;
            if (keystate.IsKeyDown(Keys.Down)) spriteSpeed.Y += playerSpeed;
            if (keystate.IsKeyDown(Keys.Z) && firedelay < 0 && playerStatus != "dead")
            {
                //Shoot bullets at fixed rate when Z is pressed (and only when the player is not dead)
                pBullets.Add(new Bullet(bulletTexture,playerPosition,fireangle, bulletSpeed, Color.White));
                playerShoot.Play();
                firedelay += firerate;
            }
            //Angle testing controls
            if (keystate.IsKeyDown(Keys.X)) fireangle -= 1.0f;
            if (keystate.IsKeyDown(Keys.C)) fireangle += 1.0f;
        }

        SpriteEffects playerEffect = SpriteEffects.None;

        public void drawPlayer()
        {
            //Respawn the player in the center of the screen and give 5 sec. of invulnerability
            if (playerStatus == "dead")
            {
                respawnDelay -= (float)dt;
                if (respawnDelay <= 0)
                {
                    respawnDelay = 5.0f;
                    playerPosition = new Vector2(width, height) / 2;  playerStatus = "spawning";
                }
            }
            else
            {
                //Player is alive after invulnerability period runs out
                if (playerStatus == "spawning")
                {
                    respawnDelay -= (float)dt;
                    if (respawnDelay <= 0)
                    {
                        respawnDelay = 3.0f;
                        playerStatus = "alive";
                    }
                }
                //Move the player
                playerPosition += spriteSpeed * (float)dt;
                //Keep the player on screen
                playerPosition.X = MathHelper.Clamp(playerPosition.X, playerTexture.dim.X / 2, width - playerTexture.dim.X / 2);
                playerPosition.Y = MathHelper.Clamp(playerPosition.Y, playerTexture.dim.Y / 2, height - playerTexture.dim.Y / 2);
                //Determine animations based on movement and current animation
                if (playerTexture == reimuTextures[0])
                    playerTexture = reimuTextures[1];
                if (playerTexture == reimuTextures[1] && playerTexture.willFinish(dt))
                    playerTexture = reimuTextures[2];
                if (spriteSpeed.X < 0 && playerEffect == SpriteEffects.FlipHorizontally)
                { playerTexture = reimuTextures[1]; playerEffect = SpriteEffects.None; }
                if (spriteSpeed.X > 0 && playerEffect == SpriteEffects.None)
                { playerTexture = reimuTextures[1]; playerEffect = SpriteEffects.FlipHorizontally; }
                if (spriteSpeed.X == 0) playerTexture = reimuTextures[0];
                //Draw player
                float[] trans = {0.5f, 1.0f};
                if (playerStatus == "spawning")
                    //Make player flash if spawning
                    spriteBatch.Draw(playerTexture.img, playerPosition - playerTexture.dim / 2,
                    playerTexture.getFrame(dt), Color.White * trans[(int)(respawnDelay * 2 % 2.0f)], 0.0f, Vector2.Zero, 1.0f, playerEffect, 0.5f);
                else
                    //Daw player normally if alive
                    spriteBatch.Draw(playerTexture.img, playerPosition - playerTexture.dim / 2, 
                    playerTexture.getFrame(dt), Color.White, 0.0f, Vector2.Zero, 1.0f, playerEffect, 0.5f);
            }

        }

        double spawnDelay = 0.0;

        public void spawnEnemies(double dt)
        {
            spawnDelay -= dt;
            if (spawnDelay <= 0)
            {
                spawnDelay += 0.5; enemies.Add(new Enemy(new AnimatedTexture(enemyTextures[0]),
                   new Vector2(random.Next(0, width), -30), 180.0f, 50.0f, Color.Blue));
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            //Window data
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            int width = graphics.GraphicsDevice.Viewport.Width;
            int height = graphics.GraphicsDevice.Viewport.Height;

            // Begin drawing sprites
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            //Move and draw player bullets
            for (int i = 0; i < pBullets.Count; i++)
            {
                Bullet b = pBullets[i];
                spriteBatch.Draw(b.img, b.pos - b.dim / 2, b.color);
                //Remove bullet if off-screen
                if (!b.update(dt))
                { pBullets.RemoveAt(i); i--; continue; }
                //Check for bullet collisions with enemies
                for (int j = 0; j < enemies.Count; j++)
                {
                    if (Math.Abs(enemies[j].pos.X - b.pos.X) < enemies[j].dim.X &&
                        Math.Abs(enemies[j].pos.Y - b.pos.Y) < enemies[j].dim.Y)
                    //Destroy enemy and bullet upon collision and create explosion
                    {
                        explosions.Add(new Explosion(enemyExplode, enemies[j].pos,
                            1.0f, enemies[j].color));
                        enemySound.Play();
                        pBullets.RemoveAt(i); i--;
                        enemies.RemoveAt(j); j--;
                        break;
                    }
                }
            }
            //Move and draw the player
            this.drawPlayer();
            //Move and draw enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy e = enemies[i];
                spriteBatch.Draw(e.img.img, e.pos - e.dim / 2, e.img.getFrame(dt),
                Color.White, 0.0f, Vector2.Zero, 1.0f, e.effect, 0.0f);
                if (!e.update(dt))
                { enemies.RemoveAt(i); i--; continue; }
                //Check for enemy collisions with player (but only if the player is not dead)
                if (Math.Abs(e.pos.X - playerPosition.X) < e.dim.X - 10 &&
                    Math.Abs(e.pos.Y - playerPosition.Y) < e.dim.Y - 10 &&
                    playerStatus != "dead")
                //Destroy enemy upon collision with player and create explosion
                {
                    explosions.Add(new Explosion(enemyExplode, e.pos,
                        1.0f, e.color));
                    enemySound.Play();
                    enemies.RemoveAt(i); i--;
                    //If the player is alive, kill the player and create large explosion
                    if (playerStatus == "alive")
                    {
                        playerStatus = "dead";
                        deathSound.Play();
                        explosions.Add(new Explosion(playerExplode, playerPosition,
                            3.0f, Color.White));
                    }
                }
            }
            //Move and draw enemy bullets
            for (int i = 0; i < eBullets.Count; i++)
            {
                Bullet b = eBullets[i];
                //Remove enemy if off-screen
                if (!b.update(dt))
                { eBullets.RemoveAt(i); i--; continue; }
            }
            //Update and draw explosions
            for (int i = 0; i < explosions.Count; i++)
            {
                Explosion e = explosions[i];
                e.expandAmount += (float)dt * 2 * e.expansionRate;
                e.alphaAmount -= (float)dt * 2 / e.expansionRate;
                if (e.alphaAmount <= 0)
                { explosions.RemoveAt(i); i--; continue; }
                spriteBatch.Draw(e.img, e.pos - e.dim * e.expandAmount / 2, e.img.Bounds, Color.White * e.alphaAmount,
                    0.0f, Vector2.Zero, e.expandAmount, SpriteEffects.None, 0.0f);
            }

            //End drawing sprites
            spriteBatch.End();
            base.Draw(gameTime);
        }



        public class Bullet
        {
            public Texture2D img;
            public Vector2 dim;
            public Vector2 pos;
            public Vector2 dir;
            public float speed;
            public float angle;
            public float radians;
            public Color color;
            public float hitRadius;
            //Constructor given angular direction
            public Bullet(Texture2D t, Vector2 p, float a, float s, Color c)
            {
                img = t; pos = p; angle = a-90.0f; speed = s;
                radians = MathHelper.ToRadians(angle);
                dir = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
                dim = new Vector2(img.Width, img.Height);
                color = c;
            }
            //Constructor given vector direction
            public Bullet(Texture2D t, Vector2 p, Vector2 d)
            {
                img = t; pos = p; dir = d;
                angle = MathHelper.ToDegrees((float)Math.Atan2(dir.Y, dir.X))+90.0f;
                speed = (float)Math.Sqrt(d.X * d.X + d.Y * d.Y);
            }
            //Update method to move the bullet
            public bool update(double dt)
            {
                //Move the bullet
                pos += dir * speed * (float)dt;
                //Return false if bullet off-screen
                if (pos.X < -50 || pos.X > width + 50 || pos.Y < -50 || pos.Y > height + 50)
                    return false;
                return true;
            }

        }

        public class Enemy
        {
            public AnimatedTexture img;
            public Vector2 dim;
            public Vector2 pos;
            public Vector2 dir;
            public float speed;
            public float angle;
            public float radians;
            public Color color;
            public SpriteEffects effect = SpriteEffects.None;
            //Constructor given angular direction
            public Enemy(AnimatedTexture t, Vector2 p, float a, float s, Color c)
            {
                img = t; pos = p; angle = a - 90.0f; speed = s;
                radians = MathHelper.ToRadians(angle);
                dir = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
                dim = img.dim; color = c;
            }
            //Constructor given vector direction
            public Enemy(AnimatedTexture t, Vector2 p, Vector2 d)
            {
                img = t; pos = p; dir = d;
                angle = MathHelper.ToDegrees((float)Math.Atan2(dir.Y, dir.X)) + 90.0f;
                speed = (float)Math.Sqrt(d.X * d.X + d.Y * d.Y);
            }
            //Update method to move the enemy
            public bool update(double dt)
            {
                //Move the enemy
                pos += dir * speed * (float)dt;
                //Return false if enemy off-screen
                if (pos.X < -50 || pos.X > width + 50 || pos.Y < -50 || pos.Y > height + 50)
                    return false;
                return true;
            }
        }

        public class Explosion
        {
            public Texture2D img;
            public Vector2 dim;
            public Vector2 pos;
            public float expansionRate;
            public float expandAmount = 0.5f;
            public float alphaAmount = 1.0f;
            public Color color;
            //Constructor taking texture file, position, expansion rate, and color.
            public Explosion(Texture2D i, Vector2 p, float e, Color c)
            {
                img = i; pos = p; dim = new Vector2(i.Width, i.Height); expansionRate = e; color = c;
            }
        }

        public struct PlayerStatus
        {
            public static PlayerStatus Alive; public static PlayerStatus Dead; public static PlayerStatus Spawning;
        }

        public class AnimatedTexture
        {
            public Texture2D img;
            public Vector2 dim;
            public int frames;
            public int frame = 0;
            public double speed;
            public double delay;
            //Constructor taking texture file, number of frames, frame rate, and optional effects
            public AnimatedTexture(Texture2D t, int f, double s)
            {
                img = t; frames = f; delay = speed = s;
                dim = new Vector2(img.Width / frames, img.Height);
            }
            //Constructor making a new instance as a copy. Use this to make several instances of
            //AnimatedTexture to animate seperately.
            public AnimatedTexture(AnimatedTexture a)
            {
                img = a.img; frames = a.frames; delay = a.delay; speed = a.speed; delay = speed;
                dim = a.dim;
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