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

        Texture2D bulletTexture;

        Effects.AnimatedTexture playerAnimation;

        List<int> playerAnimationFly = new List<int>();
        List<int> playerAnimationLeft = new List<int>();
        List<int> playerAnimationLeftAccel = new List<int>();
        List<int> playerAnimationRight = new List<int>();
        List<int> playerAnimationRightAccel = new List<int>();

        Song music;
        SoundEffect soundShoot;

        int width;
        int height;

        float oldDX;
        float oldDY;

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

            Texture2D playerAnimationTexture;
            playerAnimationTexture = game.Content.Load<Texture2D>("reimu");

            bulletTexture = game.Content.Load<Texture2D>("bullet1");

            playerAnimation = new Effects.AnimatedTexture(playerAnimationTexture, playerPosition, 18, 0.1);

            // Create Reimu animation sets
            int i = 0;
            for (i = 0; i < 4; i++)
                this.playerAnimationFly.Add(i);
            for (i = 4; i <= 6; i++)
                this.playerAnimationLeftAccel.Add(i);
            for (i = 7; i <= 10; i++)
                this.playerAnimationLeft.Add(i);
            for (i = 13; i >= 11; i--)
                this.playerAnimationRightAccel.Add(i);
            for (i = 17; i >= 14; i--)
                this.playerAnimationRight.Add(i);

            playerAnimation.animationSets.Add("Fly", playerAnimationFly);
            playerAnimation.animationSets.Add("Left Accel", playerAnimationLeftAccel);
            playerAnimation.animationSets.Add("Left", playerAnimationLeft);
            playerAnimation.animationSets.Add("Right Accel", playerAnimationRightAccel);
            playerAnimation.animationSets.Add("Right", playerAnimationRight);

            playerAnimation.animationSet = "Fly";

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

            oldDX = playerVelocity.X;
            oldDY = playerVelocity.Y;

            // Move the player if the arrow keys are pressed
            playerVelocity = Vector2.Zero;
            if (keystate.IsKeyDown(Keys.Left))
            {
                playerVelocity.X += -playerSpeed;
            }
            if (keystate.IsKeyDown(Keys.Right))
            {
                playerVelocity.X += playerSpeed;
            }
            if (keystate.IsKeyDown(Keys.Up))
            {
                playerVelocity.Y += -playerSpeed;
            }
            if (keystate.IsKeyDown(Keys.Down))
            {
                playerVelocity.Y += playerSpeed;
            }

            // Animate the player based on velocity
            if (playerVelocity.X < 0.0f)
            {
                // Moving left
                if (playerAnimation.animationSet != "Left")
                {
                    playerAnimation.animationSet = "Left Accel";
                    playerAnimation.nextAnimationSet = "Left";
                }
            }
            else if (playerVelocity.X > 0.0f)
            {
                // Moving right
                if (playerAnimation.animationSet != "Right")
                {
                    playerAnimation.animationSet = "Right Accel";
                    playerAnimation.nextAnimationSet = "Right";
                }
            }
            else
            {
                playerAnimation.animationSet = "Fly";
            }

            //Move the player sprite based on its speed
            playerPosition += playerVelocity * dt;
            playerAnimation.position = playerPosition;

            playerAnimation.Update(dt);

            // Make sure that the player stays on the screen
            playerPosition.X = MathHelper.Clamp(playerPosition.X, 0, width - playerAnimation.width);
            playerPosition.Y = MathHelper.Clamp(playerPosition.Y, 0, height- playerAnimation.height);

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
                    bulletPosition.X = playerPosition.X + (playerAnimation.width - bulletTexture.Width) / 2;
                    bulletPosition.Y = playerPosition.Y + (playerAnimation.height - bulletTexture.Height) / 2;
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
