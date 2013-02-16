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
    public class Level
    {

        Texture2D playerTexture;
        Texture2D bulletTexture;

        Effects.AnimatedTexture playerAnimation;

        Song music;
        SoundEffect soundShoot;

        int width;
        int height;

        Vector2 playerPosition = Vector2.Zero;
        Vector2 playerVelocity = Vector2.Zero;

        List<Bullet> playerBullets = new List<Bullet>();
        List<Bullet> enemyBullets = new List<Bullet>();

        float playerSpeed = 170.0f;
        float playerFireWait = 0.0f;
        float playerFireDelay = 0.1f;

        // Delta time, the amount of time passed between game frames.
        float dt;

        SpriteBatch spriteBatch;

        public Level(Game game)
        {
            // Load images and sprites
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            playerTexture = game.Content.Load<Texture2D>("reimufly");
            bulletTexture = game.Content.Load<Texture2D>("bullet1");

            playerAnimation = new Effects.AnimatedTexture(playerTexture, playerPosition, 4, 0.2);

            // Load other game media
            music = game.Content.Load<Song>("A Soul As Red As Ground Cherry");
            soundShoot = game.Content.Load<SoundEffect>("playershoot");

            // Start playing level music
            // MediaPlayer.Play(music);

            width = game.GraphicsDevice.Viewport.Width;
            height = game.GraphicsDevice.Viewport.Height;
        }
        
        public void addBullet(Bullet newBullet, BulletSet bulletSet)
        {
            if (bulletSet == BulletSet.Enemy)
                enemyBullets.Add(newBullet);
            else if (bulletSet == BulletSet.Player)
                playerBullets.Add(newBullet);
        }

        public void Update(GameTime gameTime, KeyboardState keystate)
        {
            dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Move the player if the arrow keys are pressed
            playerVelocity = Vector2.Zero;
            if (keystate.IsKeyDown(Keys.Left))
                playerVelocity.X += -playerSpeed;
            if (keystate.IsKeyDown(Keys.Right))
                playerVelocity.X += playerSpeed;
            if (keystate.IsKeyDown(Keys.Up)) 
                playerVelocity.Y += -playerSpeed;
            if (keystate.IsKeyDown(Keys.Down))
                playerVelocity.Y += playerSpeed;

            //Move the player sprite based on its speed
            playerPosition += playerVelocity * dt;
            playerAnimation.position = playerPosition;

            playerAnimation.Update(dt);

            // Make sure that the player stays on the screen
            playerPosition.X = MathHelper.Clamp(playerPosition.X, 0, width - playerTexture.Width);
            playerPosition.Y = MathHelper.Clamp(playerPosition.Y, 0, height- playerTexture.Height);

            //Decrease the player firing delay
            if (playerFireWait > 0.0f)
                playerFireWait -= dt;

            //Check to see if the player will fire a bullet
            if (keystate.IsKeyDown(Keys.Z))
            {
                if (playerFireWait <= 0.0f)
                {
                    playerFireWait = playerFireDelay;
                    // Fire a new player bullet
                    Vector2 bulletPosition;
                    bulletPosition.X = playerPosition.X + (playerTexture.Width - bulletTexture.Width) / 2;
                    bulletPosition.Y = playerPosition.Y + (playerTexture.Height - bulletTexture.Height) / 2;
                    Bullet bullet = new Bullet(bulletTexture, bulletPosition, 0.0f, 700.0f);
                    addBullet(bullet, BulletSet.Player);
                    soundShoot.Play();
                }
            }

            //Update each bullet on the screen.
            for (int i = 0; i < playerBullets.Count; i++)
            {
                Bullet bullet = playerBullets[i];
                bullet.Update(dt);

                // Remove any off-screen bullets
                if (bullet.position.X < 0 || bullet.position.X > width
                    || bullet.position.Y < 0 || bullet.position.Y > height)
                {
                    playerBullets.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < enemyBullets.Count; i++)
            {
                Bullet bullet = enemyBullets[i];
                bullet.Update(dt);

                // Remove any off-screen bullets
                if (bullet.position.X < 0 || bullet.position.X > width
                    || bullet.position.Y < 0 || bullet.position.Y > height)
                {
                    enemyBullets.RemoveAt(i);
                    i--;
                }
            }
        }

        public void Draw()
        {
            // Draw the player sprite
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            //spriteBatch.Draw(playerTexture, playerPosition, Color.White);

            playerAnimation.Draw(spriteBatch);

            //Draw each bullet on the screen
            for (int i = 0; i < playerBullets.Count; i++)
            {
                Bullet bullet = playerBullets[i];
                bullet.Draw(spriteBatch);
            }
            for (int i = 0; i < enemyBullets.Count; i++)
            {
                Bullet bullet = playerBullets[i];
                bullet.Draw(spriteBatch);
            }

            spriteBatch.End();
        }
    }

    public enum BulletSet
    {
        Player,
        Enemy,
    }

    public class Bullet
    {
        public Texture2D image;
        public Vector2 position;
        float speed;
        float angle;
        Vector2 velocity;

        // Create a new bullet with an angle and a speed.
        public Bullet(Texture2D image, Vector2 position, float angle, float speed)
        {
            this.image = image;
            this.position = position;
            this.speed = speed;
            this.Angle = angle;
        }

        // Create a new bullet with a position and velocity.
        public Bullet(Texture2D image, Vector2 position, Vector2 velocity)
        {
            this.image = image;
            this.position = position;
            this.Velocity = velocity;
        }

        public void Update(float dt)
        {
            //Move the Bullet in the direction of the velocity vector
            position.X += velocity.X * dt;
            position.Y += velocity.Y * dt;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(image, position, Color.White);
        }

        public Vector2 Velocity
        {
            get
            {
                return this.velocity;
            }
            set
            {
                this.velocity = Velocity;
                // Calculate the traveling angle of the velocity.
                this.angle = MathHelper.ToDegrees((float)Math.Atan2(velocity.Y, velocity.X)) + 90.0f;
                // Use the Pythagorean theorum to calculate the speed in the given velocity.
                this.speed = (float)Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Y, 2));
            }
        }

        public float Angle
        {
            get
            {
                return angle;
            }
            set
            {
                this.angle = Angle;
                float radians;
                radians = MathHelper.ToRadians(this.angle - 90.0f);

                this.velocity = new Vector2(
                    (float)Math.Cos(radians) * this.speed,
                    (float)Math.Sin(radians) * this.speed
                );
            }
        }

        public float Speed
        {
            get
            {
                return this.speed;
            }
            set
            {
                // Set new speed to bullet
                this.speed = Speed;
                // Set angle equal to itself to velocity values
                this.Angle = this.angle;
            }
        }
    }
}
