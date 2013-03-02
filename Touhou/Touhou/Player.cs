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


namespace Touhou.Battle
{

    public class Player
    {
        public Level level;

        public Effects.AnimatedTexture playerAnimation;
        Texture2D bulletTexture;

        // Setup player animation sets
        List<int> playerAnimationFly = new List<int> { 0, 1, 2, 3 };
        List<int> playerAnimationLeft = new List<int> { 7, 8, 9, 10 };
        List<int> playerAnimationLeftAccel = new List<int> { 4, 5, 6 };
        List<int> playerAnimationRight = new List<int> { 17, 16, 15, 14 };
        List<int> playerAnimationRightAccel = new List<int> { 13, 12, 11 };

        Vector2 playerPosition = Vector2.Zero;
        Vector2 playerVelocity = Vector2.Zero;

        float playerSpeed = 170.0f;
        float playerFireWait = 0.0f;
        float playerFireDelay = 0.1f;

        float oldDX;
        float oldDY;

        SoundEffect soundShoot;

        public Player(Game game, Level level)
        {

            this.level = level;

            Texture2D playerAnimationTexture;

            playerAnimationTexture = game.Content.Load<Texture2D>("reimu");
            playerAnimation = new Effects.AnimatedTexture(playerAnimationTexture, playerPosition, 18, 0.1);
            // Setup final animation sets
            playerAnimation.animationSets = new Dictionary<string, List<int>>()
	        {
	            {"Fly", playerAnimationFly},
                {"Left Accel", playerAnimationLeftAccel},
                {"Left", playerAnimationLeft},
                {"Right Accel", playerAnimationRightAccel},
                {"Right", playerAnimationRight},
	        };
            playerAnimation.animationSet = "Fly";

            soundShoot = game.Content.Load<SoundEffect>("playershoot");

            bulletTexture = game.Content.Load<Texture2D>("bullet1");

        }

        public void Update(float dt, KeyboardState keystate)
        {

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
            playerPosition.X = MathHelper.Clamp(playerPosition.X, 0, level.width - playerAnimation.width);
            playerPosition.Y = MathHelper.Clamp(playerPosition.Y, 0, level.height - playerAnimation.height);

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
                    level.addBullet(bullet, BulletSet.Player);
                    soundShoot.Play();
                }
            }

        }

    }

}