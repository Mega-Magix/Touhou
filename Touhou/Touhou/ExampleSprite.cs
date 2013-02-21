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

namespace Touhou.ExampleSprite
{
    public class ExampleSprite : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public ExampleSprite()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();

        }

        //Lists for player bullets and enemy bullets
        static List<Bullet> pBullets = new List<Bullet>();
        static List<Bullet> eBullets = new List<Bullet>();
        static List<Enemy> enemies = new List<Enemy>();

        //Arrays and lists for storing texture and sound data
        string[] texFiles = {"reimufly","reimumoveleft","reimuleft",
                            "enemy1fly","enemy1moveright","enemy1right",
                            "enemy2fly","enemy2moveright","enemy2right",
                            "itempower","itempoint",
                            "bullet1","bullet2","testbullet","testbullet2",
                            "explode","explodeblue",
                            "foreground","sky"
                            };
        string[] soundFiles = { "death", "enemyshoot", "explodesound", "playershoot", "item", "damage" };

        static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        static Dictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();

        static List<AnimatedTexture> reimuTextures = new List<AnimatedTexture>();
        static List<AnimatedTexture> enemyTextures = new List<AnimatedTexture>();
        static List<Explosion> explosions = new List<Explosion>();
        static List<Item> items = new List<Item>();
        static List<ScoreText> scoreTexts = new List<ScoreText>();
        static AnimatedTexture playerTexture;
        static SoundManager enemyShootManager;
        static Song bgm;

        static Random random = new Random();

        static Vector2 gameDim = new Vector2(420, 480);
        static Vector2 screenDim;
        static Vector2 foregroundDim;

        static SpriteFont font;
        static SpriteFont fontfps;

        static int fps = 0;

        static int score = 0;
        static int power = 0;

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //Load all textures
            for (int i = 0; i < texFiles.Length; i++)
                textures.Add(texFiles[i], Content.Load<Texture2D>(texFiles[i]));
            //Load all sounds
            for (int i = 0; i < soundFiles.Length; i++)
                sounds.Add(soundFiles[i], Content.Load<SoundEffect>(soundFiles[i]));
            //Load font
            font = Content.Load<SpriteFont>("SpriteFont1");
            fontfps = Content.Load<SpriteFont>("SpriteFont2");
            //Create Reimu animations
            reimuTextures.Add(new AnimatedTexture(textures["reimufly"], 4, 0.2));
            reimuTextures.Add(new AnimatedTexture(textures["reimumoveleft"], 3, 0.1));
            reimuTextures.Add(new AnimatedTexture(textures["reimuleft"], 4, 0.2));
            //Create blue enemy animations
            enemyTextures.Add(new AnimatedTexture(textures["enemy1fly"], 4, 0.2));
            enemyTextures.Add(new AnimatedTexture(textures["enemy1moveright"], 3, 0.1));
            enemyTextures.Add(new AnimatedTexture(textures["enemy1right"], 1, 1));
            //Create red enemy animations
            enemyTextures.Add(new AnimatedTexture(textures["enemy2fly"], 4, 0.2));
            enemyTextures.Add(new AnimatedTexture(textures["enemy2moveright"], 3, 0.1));
            enemyTextures.Add(new AnimatedTexture(textures["enemy2right"], 1, 1));
            //Starting Reimu texture
            playerTexture = reimuTextures[0];
            //Create enemy shot sound manager
            enemyShootManager = new SoundManager(sounds["enemyshoot"]);
            //Load BGM and play it
            bgm = Content.Load<Song>("Fall of Fall");
            MediaPlayer.IsRepeating = true; MediaPlayer.Play(bgm);
            //Set screen boundaries
            screenDim = new Vector2(GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height);
            foregroundDim = screenDim - gameDim;
            playerPosition = gameDim / 2;
        }

        protected override void UnloadContent()
        {

        }

        //State of keyboard
        KeyboardState keystate;
        //Player and bullet data
        static Vector2 spriteSpeed = Vector2.Zero;
        static Vector2 playerPosition;
        public enum PlayerStatus { Alive, Spawning, Dead };
        public enum item { Point, Power };
        public enum bulletType { Normal, Directional, Animated, Homing, Spinning, Laser }
        static PlayerStatus playerStatus = PlayerStatus.Alive;
        static float respawnDelay = 3.0f;
        static float playerSpeed = 100.0f;
        static float firerate1 = 0.1f;
        static float firerate2 = 0.5f;
        static float firedelay1 = 0.0f;
        static float firedelay2 = 0.0f;
        static int fireamount1 = 1;
        static int fireamount2 = 1;

        static double dt;
        static SpriteEffects playerEffect = SpriteEffects.None;





        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            //Get elapsed time
            dt = gameTime.ElapsedGameTime.TotalSeconds;
            //Get framerate
            fps = (int)(1.0 / dt) + 1;

            //Read keyboard input
            this.readInput();
            //Spawn enemies randomly
            this.spawnEnemies();
            //Move and draw sprites
            this.Draw(gameTime);


            base.Update(gameTime);
        }

        void readInput()
        {
            //Get keyboard state
            keystate = Keyboard.GetState();
            //Read keyboard input
            spriteSpeed = Vector2.Zero;
            if (firedelay1 >= 0) firedelay1 -= (float)dt;
            if (firedelay2 >= 0) firedelay2 -= (float)dt;
            if (keystate.IsKeyDown(Keys.Left)) spriteSpeed.X -= playerSpeed;
            if (keystate.IsKeyDown(Keys.Right)) spriteSpeed.X += playerSpeed;
            if (keystate.IsKeyDown(Keys.Up)) spriteSpeed.Y -= playerSpeed;
            if (keystate.IsKeyDown(Keys.Down)) spriteSpeed.Y += playerSpeed;
            if (keystate.IsKeyDown(Keys.Z) && firedelay1 < 0 && playerStatus != PlayerStatus.Dead)
            {
                //Shoot bullets at fixed rate when Z is pressed (and only when the player is not dead)
                pBullets.Add(new Bullet(textures["bullet1"],playerPosition,
                    0.0f, 750.0f, Color.White, bulletType.Directional));
                sounds["playershoot"].Play();
                firedelay1 += firerate1;
            }
            if (keystate.IsKeyDown(Keys.Z) && firedelay2 < 0 && playerStatus != PlayerStatus.Dead)
            {
                //Shoot bullets at fixed rate when Z is pressed (and only when the player is not dead)
                pBullets.Add(new Bullet(textures["bullet2"], playerPosition,
                    30.0f, 500.0f, Color.White, bulletType.Homing));
                pBullets.Add(new Bullet(textures["bullet2"], playerPosition,
                    -30.0f, 500.0f, Color.White, bulletType.Homing));
                firedelay2 += firerate2;
            }
        }

        public void drawPlayer()
        {
            //Respawn the player in the center of the screen and give 5 sec. of invulnerability
            if (playerStatus == PlayerStatus.Dead)
            {
                respawnDelay -= (float)dt;
                if (respawnDelay <= 0)
                {
                    respawnDelay = 5.0f;
                    playerPosition = gameDim / 2;  playerStatus = PlayerStatus.Spawning;
                }
            }
            else
            {
                //Player is alive after invulnerability period runs out
                if (playerStatus == PlayerStatus.Spawning)
                {
                    respawnDelay -= (float)dt;
                    if (respawnDelay <= 0)
                    {
                        respawnDelay = 3.0f;
                        playerStatus = PlayerStatus.Alive;
                    }
                }
                //Move the player
                playerPosition += spriteSpeed * (float)dt;
                //Keep the player on screen
                playerPosition.X = MathHelper.Clamp(playerPosition.X, 0, gameDim.X );
                playerPosition.Y = MathHelper.Clamp(playerPosition.Y, 0, gameDim.Y );
                //Determine animations based on movement and current animation
                if (playerTexture == reimuTextures[0])
                    playerTexture = reimuTextures[1];
                if (playerTexture == reimuTextures[1] && playerTexture.willFinish(dt))
                    playerTexture = reimuTextures[2];
                if (spriteSpeed.X < 0 && playerEffect == SpriteEffects.FlipHorizontally)
                { playerTexture = reimuTextures[1]; playerEffect = SpriteEffects.None; }
                if (spriteSpeed.X > 0 && playerEffect == SpriteEffects.None)
                { playerTexture = reimuTextures[1]; playerEffect = SpriteEffects.FlipHorizontally; }
                if (spriteSpeed.X == 0) playerTexture = reimuTextures[0];
                //Draw player
                float[] trans = {0.5f, 1.0f};
                if (playerStatus == PlayerStatus.Spawning)
                    //Make player flash if spawning
                    spriteBatch.Draw(playerTexture.img, playerPosition - playerTexture.dim / 2,
                    playerTexture.getFrame(), Color.White * trans[(int)(respawnDelay * 2 % 2.0f)], 0.0f, Vector2.Zero, 1.0f, playerEffect, 0.5f);
                else
                    //Draw player normally if alive
                    spriteBatch.Draw(playerTexture.img, playerPosition - playerTexture.dim / 2, 
                    playerTexture.getFrame(), Color.White, 0.0f, Vector2.Zero, 1.0f, playerEffect, 0.5f);
            }

        }

        public void drawForeground()
        {
            for (int i = (int)gameDim.X; i < screenDim.X; i += textures["foreground"].Width)
            {
                for (int j = 0; j < screenDim.Y; j += textures["foreground"].Height)
                {
                    spriteBatch.Draw(textures["foreground"], new Vector2(i, j), textures["foreground"].Bounds,
                        Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.01f);
                }
            }
        }

        double spawnDelay1 = 0.0;
        double spawnDelay2 = 15.0;
        public void spawnEnemies()
        {
            spawnDelay1 -= dt;
            spawnDelay2 -= dt;
            if (spawnDelay1 <= 0)
            {
                spawnDelay1 += 0.5; enemies.Add(new Enemy(new AnimatedTexture(enemyTextures[3]),
                   new Vector2(random.Next(0, (int)gameDim.X), -30), 180.0f, 50.0f, Color.Blue, 3, 1
                   ));
            }
            if (spawnDelay2 <= 0)
            {
                spawnDelay2 += 15.0;
                int n = random.Next(50, 150);
                for (int i = 0; i < 3; i++)
                {
                    enemies.Add(new Enemy(new AnimatedTexture(enemyTextures[0]),
                       new Vector2(n+100*i, -30), 180.0f, 50.0f, Color.Blue, 6, 2));
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            //Window data
            int width = graphics.GraphicsDevice.Viewport.Width;
            int height = graphics.GraphicsDevice.Viewport.Height;
            //Reset sound
            enemyShootManager.reset();
            // Begin drawing sprites
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            //Draw bg
            spriteBatch.Draw(textures["sky"], Vector2.Zero, textures["sky"].Bounds,
                Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            //Move and draw player bullets
            for (int i = 0; i < pBullets.Count; i++)
            {
                Bullet b = pBullets[i];
                if (b.type == bulletType.Normal)
                    spriteBatch.Draw(b.img, b.pos - b.dim / 2, b.img.Bounds,
                    Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.4f);
                else
                    spriteBatch.Draw(b.img, b.pos - b.dim / 2, b.img.Bounds,
                    Color.White, MathHelper.ToRadians(b.angle+90), Vector2.Zero, 1.0f, SpriteEffects.None, 0.4f);
                //Remove bullet if off-screen
                if (!b.update("enemies"))
                { pBullets.RemoveAt(i); i--; continue; }
            }

            //Move and draw the player
            this.drawPlayer();

            //Move and draw enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy e = enemies[i];
                spriteBatch.Draw(e.img.img, e.pos - e.dim / 2, e.img.getFrame(),
                Color.White, 0.0f, Vector2.Zero, 1.0f, e.effect, 0.3f);
                if (!e.update())
                { enemies.RemoveAt(i); i--; continue; }
            }
            //Move and draw enemy bullets
            for (int i = 0; i < eBullets.Count; i++)
            {
                Bullet b = eBullets[i];
                if (b.type == bulletType.Normal)
                    spriteBatch.Draw(b.img, b.pos - b.dim / 2, b.img.Bounds,
                    Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
                else
                    spriteBatch.Draw(b.img, b.pos - b.dim / 2, b.img.Bounds,
                    Color.White, MathHelper.ToRadians(b.angle + 90), Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
                //Remove bullet if off-screen
                if (!b.update("player"))
                { eBullets.RemoveAt(i); i--; continue; }
            }
            //Move and draw items
            for (int i = 0; i < items.Count; i++)
            {
                Item x = items[i];
                spriteBatch.Draw(x.img, x.pos - x.dim / 2, x.img.Bounds, Color.White, 0.0f,
                    Vector2.Zero, 1.0f, SpriteEffects.None, 0.1f);
                //Remove item if off-screen
                if (!x.update())
                { items.RemoveAt(i); i--; continue; }
            }
            //Move and draw score texts
            for (int i = 0; i < scoreTexts.Count; i++)
            {
                ScoreText t = scoreTexts[i];
                spriteBatch.DrawString(font, t.amount.ToString(), t.pos, t.color);
                //Remove text after 1 sec.
                if (!t.update())
                { scoreTexts.RemoveAt(i); i--; continue; }
            }
            //Update and draw explosions
            for (int i = 0; i < explosions.Count; i++)
            {
                Explosion e = explosions[i];
                e.expandAmount += (float)dt * 2 * e.expansionRate;
                e.alphaAmount -= (float)dt * 2 / e.expansionRate;
                if (e.alphaAmount <= 0)
                { explosions.RemoveAt(i); i--; continue; }
                spriteBatch.Draw(e.img, e.pos - e.dim * e.expandAmount / 2, e.img.Bounds, Color.White * e.alphaAmount,
                    0.0f, Vector2.Zero, e.expandAmount, SpriteEffects.None, 0.05f);
            }

            //Draw foreground
            drawForeground();

            //Draw score and power
            spriteBatch.DrawString(fontfps, "Score: " + score.ToString(), new Vector2(500, 50), Color.White);
            spriteBatch.DrawString(fontfps, "Power: " + power.ToString(), new Vector2(500, 80), Color.White);

            //Draw framerate
            spriteBatch.DrawString(fontfps, fps.ToString(), Vector2.Zero, Color.White);

            //End drawing sprites
            spriteBatch.End();
            base.Draw(gameTime);
        }






        public class Item
        {
            item type;
            public Texture2D img;
            public Vector2 dim;
            public Vector2 pos;
            float speed = -1.5f;
            public Item(item t, Texture2D i, Vector2 p)
            {
                type = t;
                pos = p;
                img = i;
                dim = new Vector2(img.Width, img.Height);
            }
            public bool update()
            {
                //Cause the item to fall
                speed += (float)dt;
                if (speed >= 1.5f) speed = 1.5f;
                pos.Y += speed;
                //Return false if item off-screen
                if (pos.Y > gameDim.Y + 10)
                    return false;
                //Check for item collisions with player (but only if the player is not dead)
                if (Math.Abs(pos.X - playerPosition.X) < playerTexture.dim.X - 10 &&
                    Math.Abs(pos.Y - playerPosition.Y) < playerTexture.dim.Y - 20 &&
                    playerStatus != PlayerStatus.Dead)
                //Destroy item upon collision with player and create text
                {
                    sounds["item"].Play();
                    if (type == item.Power)
                    {
                        scoreTexts.Add(new ScoreText(10, pos, Color.White));
                        power++;
                    }
                    else
                    {
                        if (playerPosition.Y < 150)
                            scoreTexts.Add(new ScoreText(100000, pos, Color.Yellow));
                        else scoreTexts.Add(new ScoreText(100000 - (int)playerPosition.Y * 100, pos, Color.White));
                    }
                    //Return false to indicate item destruction
                    return false;
                }
                return true;
            }
        }

        public class SoundManager
        {
            SoundEffectInstance instance1;
            SoundEffectInstance instance2;
            int current;
            bool isPlaying;
            public SoundManager(SoundEffect s)
            {
                instance1 = s.CreateInstance();
                instance2 = s.CreateInstance();
            }
            public void play()
            {
                if (!isPlaying)
                {
                    isPlaying = true;
                    if (current == 1)
                    { current = 2;  instance1.Stop(); instance2.Play(); }
                    else
                    { current = 1;  instance2.Stop(); instance1.Play(); }
                }
            }
            public void reset()
            {
                isPlaying = false;
            }
        }

        public class Bullet
        {
            public Texture2D img;
            public Vector2 dim;
            public Vector2 pos;
            public Vector2 dir;
            public float speed;
            public float angle;
            public float radians;
            public Color color;
            public float hitRadius;
            public String collision;
            public bulletType type;
            //Constructor given angular direction
            public Bullet(Texture2D t, Vector2 p, float a, float s, Color c, bulletType ty)
            {
                img = t; pos = p; angle = a - 90.0f; speed = s; type = ty;
                radians = MathHelper.ToRadians(angle);
                dir = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
                dim = new Vector2(img.Width, img.Height);
                color = c;
            }
            //Constructor given vector direction
            public Bullet(Texture2D t, Vector2 p, Vector2 d, Color c, bulletType ty)
            {
                img = t; pos = p; dir = d; type = ty;
                angle = MathHelper.ToDegrees((float)Math.Atan2(dir.Y, dir.X))+90.0f;
                speed = (float)Math.Sqrt(d.X * d.X + d.Y * d.Y);
            }
            //Update method to move the bullet and check for collisions
            public bool update(String c)
            {
                //Move the bullet
                pos += dir * speed * (float)dt;
                if (type == bulletType.Homing)
                {
                    float minDist = 10000.0f;
                    float minAngle = 0.0f;
                    for (int i = 0; i < enemies.Count; i++)
                    {
                        float dist = (float)Math.Sqrt((enemies[i].pos.X - pos.X) * (enemies[i].pos.X - pos.X) + 
                            (enemies[i].pos.Y - pos.Y) * (enemies[i].pos.Y - pos.Y));
                        if (dist < minDist)
                        {
                            minAngle = MathHelper.ToDegrees((float)Math.Atan2((enemies[i].pos.Y - pos.Y),
                                (enemies[i].pos.X - pos.X)));
                            minDist = dist;
                        }
                    }
                    if (minDist != 10000.0f)
                    {
                        if (Math.Abs(minAngle - angle) < 10)
                            angle = minAngle;
                        else angle -= 10 * Math.Sign(angle - minAngle);
                        radians = MathHelper.ToRadians(angle);
                        dir = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
                    }
                }

                //Return false if bullet off-screen
                if (pos.X < -50 || pos.X > gameDim.X + 50 || pos.Y < -50 || pos.Y > gameDim.Y + 50)
                    return false;
                if (c == "player")
                {
                    //Check for bullet collisions with player
                    if (Math.Sqrt((playerPosition.X - pos.X) * (playerPosition.X - pos.X) +
                        (playerPosition.Y - pos.Y) * (playerPosition.Y - pos.Y)) < 10.0 &&
                        playerStatus != PlayerStatus.Dead)
                    {
                        //If the player is alive, kill the player and create large explosion
                        if (playerStatus == PlayerStatus.Alive)
                        {
                            playerStatus = PlayerStatus.Dead;
                            sounds["death"].Play();
                            power = 0;
                            explosions.Add(new Explosion(textures["explode"], playerPosition,
                                3.0f, Color.White));
                        }
                        //Return false to indicate bullet destruction
                        return false;
                    }
                }
                else
                {
                    //Check for bullet collisions with enemies
                    for (int i = 0; i < enemies.Count; i++)
                    {
                        if (Math.Abs(enemies[i].pos.X - pos.X) < enemies[i].dim.X &&
                            Math.Abs(enemies[i].pos.Y - pos.Y) < enemies[i].dim.Y)
                        {
                            //Destroy bullet and damage enemy
                            enemies[i].health--;
                            sounds["damage"].Play();
                            if (enemies[i].health <= 0)
                            {
                                //Destroy enemy and create explosion and item
                                explosions.Add(new Explosion(textures["explodeblue"], enemies[i].pos,
                                    1.0f, enemies[i].color));
                                score += 1000;
                                if (random.Next(2) == 0)
                                    items.Add(new Item(item.Point, textures["itempoint"], enemies[i].pos));
                                else
                                    items.Add(new Item(item.Power, textures["itempower"], enemies[i].pos));
                                sounds["explodesound"].Play();
                                enemies.RemoveAt(i); i--;
                            }
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        public class Enemy
        {
            public AnimatedTexture img;
            public Vector2 dim;
            public Vector2 pos;
            public Vector2 dir;
            public float speed;
            public float angle;
            public float radians;
            public Color color;
            public int health;
            public SpriteEffects effect = SpriteEffects.None;
            public int script;
            public float sDelay = 0.0f;
            public float sRate = 1.0f;
            //Constructor given angular direction
            public Enemy(AnimatedTexture t, Vector2 p, float a, float s, Color c, int h, int scr)
            {
                img = t; pos = p; angle = a - 90.0f; speed = s; health = h; script = scr;
                radians = MathHelper.ToRadians(angle);
                dir = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
                dim = img.dim; color = c;
            }
            //Constructor given vector direction
            public Enemy(AnimatedTexture t, Vector2 p, Vector2 d, Color c, int h, int scr)
            {
                img = t; pos = p; dir = d; health = h; script = scr;
                angle = MathHelper.ToDegrees((float)Math.Atan2(dir.Y, dir.X)) + 90.0f;
                speed = (float)Math.Sqrt(d.X * d.X + d.Y * d.Y);
            }
            //Update method to move the enemy and check for collisions
            public bool update()
            {
                //Move the enemy
                pos += dir * speed * (float)dt;
                //Have the enemy shoot
                sDelay -= (float)dt;
                if (sDelay <= 0)
                {
                    eBullets.AddRange(this.shoot()); enemyShootManager.play(); sDelay += sRate;
                }
                //Return false if enemy off-screen
                if (pos.X < -50 || pos.X > gameDim.X + 50 || pos.Y < -50 || pos.Y > gameDim.Y + 50)
                    return false;
                //Check for enemy collisions with player (but only if the player is not dead)
                if (Math.Abs(pos.X - playerPosition.X) < dim.X - 10 &&
                    Math.Abs(pos.Y - playerPosition.Y) < dim.Y - 10 &&
                    playerStatus != PlayerStatus.Dead)
                //Destroy enemy upon collision with player and create explosion
                {
                    explosions.Add(new Explosion(textures["explodeblue"], pos,
                        1.0f, color));
                    sounds["explodesound"].Play();
                    //If the player is alive, kill the player and create large explosion
                    if (playerStatus == PlayerStatus.Alive)
                    {
                        playerStatus = PlayerStatus.Dead;
                        sounds["death"].Play();
                        power = 0;
                        explosions.Add(new Explosion(textures["explode"], playerPosition,
                            3.0f, Color.White));
                    }
                    //Return false to indicate enemy destruction
                    return false;
                }
                return true;
            }
            public List<Bullet> shoot()
            {
                switch (script)
                {
                    case 1: return shoot1(pos);
                    case 2: return shoot2(pos);
                }
                return null;
            }
        }

        public class Explosion
        {
            public Texture2D img;
            public Vector2 dim;
            public Vector2 pos;
            public float expansionRate;
            public float expandAmount = 0.5f;
            public float alphaAmount = 1.0f;
            public Color color;
            //Constructor taking texture file, position, expansion rate, and color.
            public Explosion(Texture2D i, Vector2 p, float e, Color c)
            {
                img = i; pos = p; dim = new Vector2(i.Width, i.Height); expansionRate = e; color = c;
            }
        }

        public class ScoreText
        {
            public int amount;
            public Vector2 pos;
            public Color color;
            public float time = 1.5f;
            public ScoreText(int s, Vector2 p, Color c)
            {
                amount = s; pos = p; color = c;
                score += amount;

            }
            public bool update()
            {
                pos.Y -= (float)dt * 10;
                time -= (float)dt;
                if (time < 0) return false;
                return true;
            }
        }

        public class AnimatedTexture
        {
            public Texture2D img;
            public Vector2 dim;
            public int frames;
            public int frame = 0;
            public double speed;
            public double delay;
            //Constructor taking texture file, number of frames, frame rate, and optional effects
            public AnimatedTexture(Texture2D t, int f, double s)
            {
                img = t; frames = f; delay = speed = s;
                dim = new Vector2(img.Width / frames, img.Height);
            }
            //Constructor making a new instance as a copy. Use this to make several instances of
            //AnimatedTexture to animate seperately.
            public AnimatedTexture(AnimatedTexture a)
            {
                img = a.img; frames = a.frames; delay = a.delay; speed = a.speed; delay = speed;
                dim = a.dim;
            }
            //Gets a rectangle representing the frame to be drawn. Needs to be called each update
            public Rectangle getFrame()
            {
                delay -= dt;
                if (delay < 0.0)
                {
                    delay += speed; frame++;
                    if (frame >= frames) frame = 0;
                }
                return new Rectangle(img.Width * frame / frames, 0, img.Width / frames, img.Height);
            }
            //Detects if an animation is about to finish.
            public bool willFinish(double dt)
            {
                bool willfinish = (delay - dt < 0 && frame + 1 == frames);
                if (willfinish) { frame = 0; delay += speed; }
                return willfinish;
            }
        }


        public static List<Bullet> shoot1(Vector2 pos)
        {
            List<Bullet> l = new List<Bullet>();
            l.Add(new Bullet(textures["testbullet"], pos, random.Next(0, 360),
                100.0f, Color.White, bulletType.Normal));
            return l;
        }

        public static List<Bullet> shoot2(Vector2 pos)
        {
            List<Bullet> l = new List<Bullet>();
            float a = -MathHelper.ToDegrees((float)Math.Atan2(pos.X - playerPosition.X,
                pos.Y - playerPosition.Y));
            for (int i = -60; i <= 60; i += 30)
            {
                l.Add(new Bullet(textures["testbullet2"], pos, a + i,
                    100, Color.White, bulletType.Directional));
            }
            return l;
        }
    
    
    }
}