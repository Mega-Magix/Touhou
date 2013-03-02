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

    public enum BulletSet
    {
        Player,
        Enemy,
    }

}