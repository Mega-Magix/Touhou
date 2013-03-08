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

        // Collision radius
        public int radius = 5;

        public Effect.AnimatedTexture animation;
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
        float fireDelay = 0.08f;

        float oldDX;
        float oldDY;

        SoundEffect soundShoot;

        float killWait = 0.0f;
        const float killDelay = 1.0f;
        float respawnWait = 0.0f;
        const float respawnDelay = 3.0f;
        float flashWait = 0.0f;
        const float flashDelay = .25f;
        const float playerRespawnFlashAlpha = .5f;

        // ISSUE Player is not positioning at the beginning
        public Player(Game game, Level level)
        {

            this.level = level;

            Texture2D playerAnimationTexture;

            playerAnimationTexture = game.Content.Load<Texture2D>("reimu");
            animation = new Effect.AnimatedTexture(playerAnimationTexture, position, 18, 0.1);
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

            X = level.width / 2;
            Y = level.height / 2;

        }

        public float GetCenterX()
        {
            return position.X + animation.width / 2;
        }
        public void SetCenterX(float posx)
        {
            position.X = posx - animation.width / 2;
        }
        public float GetCenterY()
        {
            return position.Y + animation.height / 2;
        }
        public void SetCenterY(float posy)
        {
            position.Y = posy - animation.height / 2;
        }

        public float X
        {
            get
            {
                return GetCenterX();
            }
            set
            {
                SetCenterX(value);
            }
        }
        public float Y
        {
            get
            {
                return GetCenterY();
            }
            set
            {
                SetCenterY(value);
            }
        }

        public void SetTransparency(float alpha)
        {
            animation.alpha = alpha;
        }
        public void ToggleTransparency()
        {
            if (animation.alpha == 1.0f)
                SetTransparency(playerRespawnFlashAlpha);
            else
                SetTransparency(1.0f);
        }

        public bool IsKilled() { return (this.killWait > 0.0f); }
        public bool IsResponding() { return (this.respawnWait > 0.0f); }

        public void Kill()
        {
            // You can't already be killed or respawning
            // and kill the player
            if (!IsKilled() && !IsResponding())
            {
                killWait = killDelay;
            }
        }
        private void Respawn()
        {
            killWait = 0.0f;
            respawnWait = respawnDelay;
            SetTransparency(playerRespawnFlashAlpha);
            // ISSUE X and Y is not setting on respawn
            flashWait = flashDelay;

            X = level.width / 2;
            Y = level.height / 2;
        }

        public void Shoot()
        {
            // Fire a new player bullet
            Vector2 bulletPosition;
            bulletPosition.X = GetCenterX();
            bulletPosition.Y = GetCenterY();
            Bullet bullet = new Bullet(bulletTexture, bulletPosition, 0.0f, 1300.0f, 6);
            level.AddBullet(bullet, BulletSet.Player);
            soundShoot.Play();
        }

        public void Update(float dt, KeyboardState keystate)
        {
            // If player is killed
            if (killWait > 0.0f)
            {
                killWait -= dt;
                if (killWait <= 0.0f)
                    Respawn();
            }
            // If player is respawning
            if (respawnWait > 0.0f)
            {
                // PLayer is between respawn flashes
                if (flashWait > 0.0f)
                    flashWait -= dt;
                else
                {
                    ToggleTransparency();
                    flashWait = flashDelay;
                }
                respawnWait -= dt;
                if (respawnWait <= 0.0f)
                    // Player just finished respawning
                    SetTransparency(1.0f);
            }

            if (!IsKilled())
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
                        Shoot();
                    }
                }

            }

        }

    }

}