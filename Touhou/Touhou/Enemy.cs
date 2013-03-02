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
    public class Enemy
    {
        Vector2 position = Vector2.Zero;
        Vector2 velocity = Vector2.Zero;

        public Effects.AnimatedTexture animation;

        public Game game;
        public Level level;

        public Boolean destroyed = false;

        public Enemy(Game game, Level level, int x, int speedX, int speedY)
        {
            this.game = game;
            this.level = level;

            Texture2D animationTexture;
            animationTexture = game.Content.Load<Texture2D>("enemy1fly");

            position.X = x;
            position.Y = -animationTexture.Height;
            velocity.X = speedX;
            velocity.Y = speedY;

            animation = new Effects.AnimatedTexture(animationTexture, position, 4, 0.2);
        }

        public void Destroy()
        {
            this.destroyed = true;
        }

        public void Update(float dt)
        {
            position += velocity * dt;
            animation.position = position;
            animation.Update(dt);

            if (position.Y >= level.height)
                Destroy();
        }
    }
}