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

        float newEnemyWait = 0;
        float newEnemyDelay = 1; 

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

            addEnemy(new Enemy(this.game, this, 100, 14, 0, 50));
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
            Bullet bullet;
            for (int i = 0; i < playerBullets.Count; i++)
            {
                bullet = playerBullets[i];
                bullet.Update(dt);

                // Remove any off-screen bullets
                if (bullet.position.X < 0 || bullet.position.X > width
                    || bullet.position.Y < 0 || bullet.position.Y > height)
                {
                    playerBullets.RemoveAt(i);
                    i--;
                }

                else
                {
                    // Check any bullets for collision
                    if (bullet.radius == 1)
                    {
                        // If radius is one, do a simple point to circle collision
                        for (int ii = 0; ii < enemies.Count; ii++)
                        {
                            enemy = enemies[ii];
                            // Find the distance from the bullet point to the center of the enemy circle
                            double distance = Math.Sqrt(
                                (bullet.position.X - enemy.getCenterX()) * (bullet.position.X - enemy.getCenterX()) +
                                (bullet.position.Y - enemy.getCenterY()) * (bullet.position.Y - enemy.getCenterY())
                                );
                            if (distance <= enemy.radius)
                            {
                                playerBullets.RemoveAt(i);
                                i--;
                                enemy.Damage();
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < enemyBullets.Count; i++)
            {
                bullet = enemyBullets[i];
                bullet.Update(dt);

                // Remove any off-screen bullets
                if (bullet.position.X < 0 || bullet.position.X > width
                    || bullet.position.Y < 0 || bullet.position.Y > height)
                {
                    enemyBullets.RemoveAt(i);
                    i--;
                }

                // Check any bullets for collision
            }

            newEnemyWait -= dt;
            if (newEnemyWait <= newEnemyDelay)
            {
                newEnemyWait += newEnemyDelay;
                addEnemy(new Enemy(this.game, this, 150, 14, 0, 50));
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
