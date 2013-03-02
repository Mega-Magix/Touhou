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

        public Effects.AnimatedTexture animation;
        Texture2D bulletTexture;

        // Setup player animation sets
        List<int> playerAnimationFly = new List<int> { 0, 1, 2, 3 };
        List<int> playerAnimationLeft = new List<int> { 7, 8, 9, 10 };
        List<int> playerAnimationLeftAccel = new List<int> { 4, 5, 6 };
        List<int> playerAnimationRight = new List<int> { 17, 16, 15, 14 };
        List<int> playerAnimationRightAccel = new List<int> { 13, 12, 11 };

        Vector2 position = Vector2.Zero;
        Vector2 velocity = Vector2.Zero;

        float speed = 170.0f;
        float fireWait = 0.0f;
        float fireDelay = 0.1f;

        float oldDX;
        float oldDY;

        SoundEffect soundShoot;

        public Player(Game game, Level level)
        {

            this.level = level;

            Texture2D playerAnimationTexture;

            playerAnimationTexture = game.Content.Load<Texture2D>("reimu");
            animation = new Effects.AnimatedTexture(playerAnimationTexture, position, 18, 0.1);
            // Setup final animation sets
            animation.animationSets = new Dictionary<string, List<int>>()
	        {
	            {"Fly", playerAnimationFly},
                {"Left Accel", playerAnimationLeftAccel},
                {"Left", playerAnimationLeft},
                {"Right Accel", playerAnimationRightAccel},
                {"Right", playerAnimationRight},
	        };
            animation.animationSet = "Fly";

            soundShoot = game.Content.Load<SoundEffect>("playershoot");

            bulletTexture = game.Content.Load<Texture2D>("bullet1");

        }

        public void Update(float dt, KeyboardState keystate)
        {

            oldDX = velocity.X;
            oldDY = velocity.Y;

            // Move the player if the arrow keys are pressed
            velocity = Vector2.Zero;
            if (keystate.IsKeyDown(Keys.Left))
            {
                velocity.X += -speed;
            }
            if (keystate.IsKeyDown(Keys.Right))
            {
                velocity.X += speed;
            }
            if (keystate.IsKeyDown(Keys.Up))
            {
                velocity.Y += -speed;
            }
            if (keystate.IsKeyDown(Keys.Down))
            {
                velocity.Y += speed;
            }

            // Animate the player based on velocity
            if (velocity.X < 0.0f)
            {
                // Moving left
                if (animation.animationSet != "Left")
                {
                    animation.animationSet = "Left Accel";
                    animation.nextAnimationSet = "Left";
                }
            }
            else if (velocity.X > 0.0f)
            {
                // Moving right
                if (animation.animationSet != "Right")
                {
                    animation.animationSet = "Right Accel";
                    animation.nextAnimationSet = "Right";
                }
            }
            else
            {
                animation.animationSet = "Fly";
            }

            //Move the player sprite based on its speed
            position += velocity * dt;
            animation.position = position;

            animation.Update(dt);

            // Make sure that the player stays on the screen
            position.X = MathHelper.Clamp(position.X, 0, level.width - animation.width);
            position.Y = MathHelper.Clamp(position.Y, 0, level.height - animation.height);

            //Decrease the player firing delay
            if (fireWait > 0.0f)
                fireWait -= dt;

            //Check to see if the player will fire a bullet
            if (keystate.IsKeyDown(Keys.Z))
            {
                if (fireWait <= 0.0f)
                {
                    fireWait = fireDelay;
                    // Fire a new player bullet
                    Vector2 bulletPosition;
                    bulletPosition.X = position.X + (animation.width - bulletTexture.Width) / 2;
                    bulletPosition.Y = position.Y + (animation.height - bulletTexture.Height) / 2;
                    Bullet bullet = new Bullet(bulletTexture, bulletPosition, 0.0f, 700.0f);
                    level.addBullet(bullet, BulletSet.Player);
                    soundShoot.Play();
                }
            }

        }

    }

}