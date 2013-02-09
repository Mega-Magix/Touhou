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
        Vector2 playerPosition = Vector2.Zero;
        Vector2 playerSpeed = Vector2.Zero;

        float playerVelocity = 100.0F;

        SpriteBatch spriteBatch;

        public Level(Game game)
        {
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            playerTexture = game.Content.Load<Texture2D>("reimu");
        }

        public void Update(GameTime gameTime, KeyboardState keystate)
        {
            // Move the player if the arrow keys are pressed
            playerSpeed = Vector2.Zero;
            if (keystate.IsKeyDown(Keys.Left))
                playerSpeed.X = -playerVelocity;
            if (keystate.IsKeyDown(Keys.Right))
                playerSpeed.X = playerVelocity;
            if (keystate.IsKeyDown(Keys.Up)) 
                playerSpeed.Y = -playerVelocity;
            if (keystate.IsKeyDown(Keys.Down))
                playerSpeed.Y = playerVelocity;

            //Move the player sprite based on its speed
            playerPosition += playerSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw()
        {
            // Draw the player sprite
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            spriteBatch.Draw(playerTexture, playerPosition, Color.White);
            spriteBatch.End();
        }
    }
}
