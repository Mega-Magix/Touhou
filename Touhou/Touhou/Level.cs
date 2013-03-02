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
    public class Level
    {
        public Game game;

        Player player;
        List<Enemy> enemies = new List<Enemy>();

        Song music;

        public int width;
        public int height;

        List<Bullet> playerBullets = new List<Bullet>();
        List<Bullet> enemyBullets = new List<Bullet>();

        // Delta time, the amount of time passed between game frames.
        float dt;

        SpriteBatch spriteBatch;

        public Level(Game game)
        {
            this.game = game;

            // Load images and sprites
            spriteBatch = new SpriteBatch(game.GraphicsDevice);

            // Create the level player
            player = new Player(game, this);

            // Load other game media
            music = game.Content.Load<Song>("A Soul As Red As Ground Cherry");

            // Start playing level music
            // MediaPlayer.Play(music);

            width = game.GraphicsDevice.Viewport.Width;
            height = game.GraphicsDevice.Viewport.Height;

            addEnemy(new Enemy(this.game, this, 100, 0, 50));
        }
        
        public void addBullet(Bullet newBullet, BulletSet bulletSet)
        {
            if (bulletSet == BulletSet.Enemy)
                enemyBullets.Add(newBullet);
            else if (bulletSet == BulletSet.Player)
                playerBullets.Add(newBullet);
        }
        public void addEnemy(Enemy enemy)
        {
            enemies.Add(enemy);
        }

        public void Update(GameTime gameTime, KeyboardState keystate)
        {
            dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            player.Update(dt, keystate);

            // Update each enemy on the screen
            Enemy enemy;
            for (int i = 0; i < enemies.Count; i++)
            {
                enemy = enemies[i];
                enemy.Update(dt);
                if (enemy.destroyed == true)
                {
                    enemies.RemoveAt(i);
                    i--;
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

            player.animation.Draw(spriteBatch);

            //Draw each enemy on the screen
            Enemy enemy;
            for (int i = 0; i < enemies.Count; i++)
            {
                enemy = enemies[i];
                enemy.animation.Draw(spriteBatch);
            }
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
}
