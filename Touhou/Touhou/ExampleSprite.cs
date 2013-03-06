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
        static GraphicsDeviceManager graphics;
        static GraphicsDevice graphicsDevice;
        static SpriteBatch spriteBatch;

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
        static List<Orb> orbs = new List<Orb>();
        static List<Enemy> enemies = new List<Enemy>();

        //Arrays and lists for storing texture and sound data
        string[] texFiles = {"reimufly","reimumoveleft","reimuleft",
                            "enemy1fly","enemy1moveright","enemy1right",
                            "enemy2fly","enemy2moveright","enemy2right",
                            "itempower","itempoint","itemhighpower","itemstar",
                            "itemfullpower","itembomb","itemplayer",
                            "itempowerarrow","itempointarrow","itemhighpowerarrow","itemstararrow",
                            "itemfullpowerarrow","itembombarrow","itemplayerarrow","spellcardtext",
                            "bullet1","bullet2","bomb1",
                            "explode","focus","powerup","bulletexplode","shoot",
                            "foreground","sky",
                            "textbox","reimu1","reimu2","marisa1","marisa2","marisafly",
                            "bg1","bg2",
                            };
        string[] bulletTexFiles = { "explode", "bulletexplode","shoot",
                                      "B1", "B2", "B3", "B4", "B5", "B6", "B7"};
        string[] soundFiles = { "death","graze",
                                  "enemyshoot1","enemyshoot2","enemyshoot3",
                                  "explodesound", "playershoot", "item", "damage",
                              "powersound", "spellcard", "defeat", "1up"};
        string[] musicFiles = {"A Soul As Red As Ground Cherry",
                                  "Song of the Night Sparrow",
                                  "Fall of Fall",
                                  "Battle"};
        string[] convFiles = { "conv1" };


        static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        static Dictionary<string, SoundManager> sounds = new Dictionary<string, SoundManager>();
        static Dictionary<string, Song> songs = new Dictionary<string,Song>();
        static Dictionary<string, string[]> conversations = new Dictionary<string, string[]>();
        static Dictionary<string, Color> convColors = new Dictionary<string,Color>();
        static Dictionary<string, Dictionary<Color, Texture2D>> bulletTextures = new Dictionary<string, Dictionary<Color, Texture2D>>();
        

        static List<AnimatedTexture> reimuTextures = new List<AnimatedTexture>();
        static List<AnimatedTexture> enemyTextures = new List<AnimatedTexture>();
        static List<Explosion> explosions = new List<Explosion>();
        static Spellcard spellcard;
        static Bomb bomb;
        static List<Background> backgrounds = new List<Background>();
        static List<Item> items = new List<Item>();
        static List<ScoreText> scoreTexts = new List<ScoreText>();
        static PowerText powerText;
        static AnimatedTexture bossTexture;

        static Random random = new Random();

        static Vector2 gameDim = new Vector2(420, 480);
        static Vector2 screenDim;
        static Vector2 foregroundDim;

        static SpriteFont font;
        static SpriteFont fontfps;
        static SpriteFont fontconv;

        static Conversation conversation;

        static int fps = 0;

        public enum PlayerStatus { Alive, Spawning, Dead }
        public enum drawType { Normal, Directional, Animated, AnimatedDirectional, Homing, Spinning, Laser }

        static int score = 0;
        static int power = 0;
        static int point = 0;
        static int graze;
        static int players = 2;
        static int bombs = 3;

        //Boss data
        static Boss boss;
        static Loop[] bossScript1Loops = new Loop[2];
        static Script bossScript1;
        static Loop[] enemyScript1Loops = new Loop[1];
        static Script enemyScript1;
        static Loop[] enemyScript2Loops = new Loop[2];
        static Script enemyScript2;
        static Loop[,] enemyScript3Loops = new Loop[2, 2];
        static Script[] enemyScript3;

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            graphicsDevice = GraphicsDevice;
            //Load all textures
            for (int i = 0; i < texFiles.Length; i++)
                textures.Add(texFiles[i], Content.Load<Texture2D>(texFiles[i]));
            //Load bullet textures into dictionary
            for (int i = 0; i < bulletTexFiles.Length; i++)
            {
                bulletTextures.Add(bulletTexFiles[i], new Dictionary<Color, Texture2D>());
                bulletTextures[bulletTexFiles[i]].Add(Color.Gray, Content.Load<Texture2D>(bulletTexFiles[i]));
            }
            //Load all sounds
            for (int i = 0; i < soundFiles.Length; i++)
                sounds.Add(soundFiles[i], new SoundManager(Content.Load<SoundEffect>(soundFiles[i])));
            //Load all music
            for (int i = 0; i < musicFiles.Length; i++)
                songs.Add(musicFiles[i], Content.Load<Song>(musicFiles[i]));
            //Load all conversations
            for (int i = 0; i < convFiles.Length; i++)
                conversations.Add(convFiles[i], System.IO.File.ReadAllLines("Content/conv1.txt"));
            //Load conversation colors
            convColors.Add("reimu", Color.Crimson); convColors.Add("marisa", Color.Gold);
            //Load fonts
            font = Content.Load<SpriteFont>("SpriteFont1");
            fontfps = Content.Load<SpriteFont>("SpriteFont2");
            fontconv = Content.Load<SpriteFont>("SpriteFont3");
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
            //Load boss data
            bossTexture = new AnimatedTexture(textures["marisafly"], 4, 0.3);
            //Load scripts
            enemyScript1Loops[0] = new Loop(1000, 1.0, 0, 0.0);
            enemyScript1 = new Script(getColoredTexture("B1", Color.Red), Color.Red, 0.0f, 100.0f, enemyScript1Loops);
            enemyScript2Loops[0] = new Loop(10, 0.5, 1, 0.0);
            enemyScript2Loops[1] = new Loop(5, 0.0, 2, 0.0);
            enemyScript2 = new Script(getColoredTexture("B2", Color.Blue), Color.Blue, 0.0f, 100.0f, enemyScript2Loops);
            enemyScript3Loops[0,0] = new Loop(20, 1.0, 3, 0.0);
            enemyScript3Loops[0,1] = new Loop(20, 0.0, 4, 0.0);
            enemyScript3Loops[1,0] = new Loop(500, 0.02, 5, 0.0);
            enemyScript3 = new Script[]{new Script(getColoredTexture("B3", Color.Green), Color.Green, 0.0f, 150.0f, new Loop[] {enemyScript3Loops[0,0], enemyScript3Loops[0,1]}),
                                        new Script(getColoredTexture("B4",Color.Yellow), Color.Yellow, 0.0f, 100.0f, new Loop[] {enemyScript3Loops[1,0]})};
            bossScript1Loops[0] = new Loop(1000, 0.5, 6, 0.0);
            bossScript1Loops[1] = new Loop(20, 0.0, 7, 0.0);
            bossScript1 = new Script(getColoredTexture("B2", Color.Purple), Color.Purple, 0.0f, 100.0f, bossScript1Loops);
            //Starting Reimu texture
            Player.img = reimuTextures[0];
            //Starting background
            backgrounds.Add(new Background(textures["sky"], new Vector2(0,30), 1.0f));
            //Set screen boundaries
            screenDim = new Vector2(GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height);
            foregroundDim = screenDim - gameDim;
            Player.pos = gameDim / 2;
            //Play BGM
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(songs["Fall of Fall"]);
        }

        protected override void UnloadContent()
        {

        }

        //State of keyboard
        static KeyboardState keystate;
       
        

        static double dt;
        static double time = 0.0;
        static SpriteEffects playerEffect = SpriteEffects.None;





        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            //Get elapsed time
            dt = gameTime.ElapsedGameTime.TotalSeconds;
            time += dt;
            //Get framerate
            fps = (int)(1.0 / dt) + 1;
            //Reset sound managers
            for (int i = 0; i < sounds.Count; i++) sounds.ElementAt(i).Value.reset();
            //Read keyboard input
            keystate = Keyboard.GetState();
            Player.readInput(keystate);
            //Spawn 10 waves of enemies
            if (waves < 10) this.spawnEnemies();
            else if (waves == 10)
            {
                waves = 11;  conversation = new Conversation(conversations["conv1"]);
                boss = new Boss(bossTexture, new Vector2(-30, 100), 0.0f, 0.0f, Color.White, 1500,
                    new Script[] {bossScript1}, new string[1]);
                enemies.Add(boss);
            }
            //Move and draw sprites
            this.Draw(gameTime);
            //Move boss
            if (boss != null && Boss.isEntering) boss.enter();


            base.Update(gameTime);
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
            //Draw score, power, lives, and bombs
            spriteBatch.DrawString(fontfps, "Score: " + score.ToString(), new Vector2(500, 50), Color.White);
            spriteBatch.DrawString(fontfps, "Player: " + players.ToString(), new Vector2(500, 100), Color.White);
            spriteBatch.DrawString(fontfps, "Bomb: " + bombs.ToString(), new Vector2(500, 130), Color.White);
            if (power < 128)
                spriteBatch.DrawString(fontfps, "Power: " + power.ToString(), new Vector2(500, 180), Color.White);
            else
                spriteBatch.DrawString(fontfps, "Power: MAX", new Vector2(500, 180), Color.White);
            spriteBatch.DrawString(fontfps, "Point: " + point, new Vector2(500, 230), Color.White);
            spriteBatch.DrawString(fontfps, "Graze: " + graze, new Vector2(500, 260), Color.White);
            


        }

        double spawnDelay1 = 0.0;
        double spawnRate1 = 0.5;
        double spawnDelay2 = 10.0;
        double spawnDelay3 = 3.0;
        double waveTime = 15.0;
        int waves = 10;
        static double fullPowerTime = -1;

        public void spawnEnemies()
        {
            spawnDelay1 -= dt;
            spawnDelay2 -= dt;
            if (waves == 9) spawnDelay3 -= dt;
            waveTime -= dt;
            if (spawnDelay1 <= 0 && waveTime >= 5)
            {
                spawnDelay1 += spawnRate1;
                string[] drops;
                if (random.NextDouble() < 0.5f) drops = new string[] { "itempower" };
                else if (random.NextDouble() < 0.01f) drops = new string[] { "itemhighpower" };
                else drops = new string[] {"itempoint"};
                enemies.Add(new Enemy(new AnimatedTexture(enemyTextures[3]),
                   new Vector2(random.Next(0, (int)gameDim.X), -30), 180.0f, 50.0f, Color.Red, 3, 
                   new Script(enemyScript1), drops));
            }
            if (spawnDelay2 <= 0)
            {
                spawnDelay2 += 10.0;
                string[] drops = new string[10];
                for (int i = 0; i < 10; i++) drops[i] = "itempoint";
                int n = random.Next(50, 150);
                for (int i = 0; i < 3; i++)
                {
                    enemies.Add(new Enemy(new AnimatedTexture(enemyTextures[0]),
                       new Vector2(n + 100 * i, -30), 180.0f, 50.0f, Color.Blue, 6,
                       new Script(enemyScript2), drops));
                }
            }
            if (spawnDelay3 <= 0)
            {
                spawnDelay3 += 1000000.0;
                string[] drops = new string[20];
                for (int i = 0; i < 19; i++) drops[i] = "itempoint";
                drops[19] = "itembomb";
                enemies.Add(new Enemy(new AnimatedTexture(enemyTextures[0]),
                       new Vector2(gameDim.X / 2, 50), 180.0f, 0.0f, Color.Blue, 100,
                       new Script[]{enemyScript3[0], enemyScript3[1]}, drops));
            }
            if (waveTime <= 0)
            {
                waveTime = 15.0; spawnDelay1 = 0.0;  spawnDelay2 = 10.0; spawnRate1 *= 0.8; waves++;
            }
        }

        public static Texture2D getColoredTexture(string t, Color c)
        {
            if (!bulletTextures.ContainsKey(t)) return textures[t];
            if (bulletTextures[t].ContainsKey(c))
                return bulletTextures[t][c];
            else
            {
                Texture2D def = bulletTextures[t][Color.Gray];
                Color[] data = new Color[def.Width * def.Height];
                def.GetData(data);
                int[] rgba = new int[4];
                for (int i = 0; i < data.Length; i++)
                {
                    rgba[0] = (data[i].R - 128 + c.R) * 2;
                    rgba[1] = (data[i].G - 128 + c.G) * 2;
                    rgba[2] = (data[i].B - 128 + c.B) * 2;
                    rgba[3] = data[i].A;
                    rgba[0] = (int)MathHelper.Clamp(rgba[0], 0, 255);
                    rgba[1] = (int)MathHelper.Clamp(rgba[1], 0, 255);
                    rgba[2] = (int)MathHelper.Clamp(rgba[2], 0, 255);
                    rgba[3] = (int)MathHelper.Clamp(rgba[3], 0, 255);
                    data[i].R = (byte)rgba[0]; data[i].G = (byte)rgba[1]; data[i].B = (byte)rgba[2]; data[i].A = (byte)rgba[3];
                }
                Texture2D newTexture = new Texture2D(graphicsDevice, def.Width, def.Height);
                newTexture.SetData(data);
                bulletTextures[t].Add(c, newTexture);
                return bulletTextures[t][c];
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            //Window data
            int width = graphics.GraphicsDevice.Viewport.Width;
            int height = graphics.GraphicsDevice.Viewport.Height;
            // Begin drawing sprites
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            //Draw bg
            if (spellcard != null) 
                for (int i = 0; i < spellcard.backgrounds.Count; i++) spellcard.backgrounds[i].draw();
            else
                for (int i = 0; i < backgrounds.Count; i++) backgrounds[i].draw();
            //Move and draw player bullets
            for (int i = 0; i < pBullets.Count; i++)
            {
                Bullet b = pBullets[i];
                if (!b.update()) { pBullets.RemoveAt(i); i--; continue; }
                b.draw(0.4f);
            }
            //Move and draw the player
            Player.update();
            Player.draw(0.5f);

            //Move and draw enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy e = enemies[i];
                if (!e.update() || e.collide()) { enemies.RemoveAt(i); i--; continue; }
                e.draw(0.3f);
            }
            //Draw boss
            if (boss != null)
            {
                boss.collide(); boss.draw(0.3f);
            }
            //Switch blend state for color blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied);
            //Move and draw enemy bullets
            for (int i = 0; i < eBullets.Count; i++)
            {
                Bullet b = eBullets[i];
                if (!b.update() || b.collide()) { eBullets.RemoveAt(i); i--; continue; }
                b.draw(0.2f);
            }
            //Switch back
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            //Move and draw items
            for (int i = 0; i < items.Count; i++)
            {
                Item x = items[i];
                if (!x.update()) { items.RemoveAt(i); i--; continue; }
                x.draw(0.1f);
            }
            //Move and draw score texts
            for (int i = 0; i < scoreTexts.Count; i++)
            {
                ScoreText t = scoreTexts[i];
                if (!t.update()) { scoreTexts.RemoveAt(i); i--; continue; }
                spriteBatch.DrawString(font, t.amount.ToString(), t.pos, t.color);
            }
            if (powerText != null)
            {
                spriteBatch.Draw(powerText.text, powerText.pos, powerText.text.Bounds, Color.White, 0.0f,
                       Vector2.Zero, 1.0f, SpriteEffects.None, 0.1f);
                if (!powerText.update()) powerText = null;
            }
            //Update and draw orbs
            for (int i = 0; i < orbs.Count; i++)
            {
                Orb b = orbs[i];
                if (!b.update()) { orbs.RemoveAt(i); i--;
                explosions.Add(new Explosion("explode", b.pos, 5.0f, 1.0f, 1.0f, Color.White));
                    sounds["enemyshoot1"].play(); continue;
                }
                b.draw(0.1f);
            }
            //Switch blend state for color blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied);
            //Update and draw explosions
            for (int i = 0; i < explosions.Count; i++)
            {
                Explosion e = explosions[i];
                if (!e.update()) { explosions.RemoveAt(i); i--; continue; }
                e.draw(0.05f);
            }
            //Switch back
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            //Update spellcards
            if (spellcard != null && !spellcard.update()) spellcard = null;
            if (bomb != null && !bomb.update()) bomb = null;

            //Draw conversation
            if (conversation != null) if (!conversation.show()) conversation = null;

            //Draw foreground
            drawForeground();

            //Draw other text
            spriteBatch.DrawString(fontfps, fps.ToString(), new Vector2(0,450), Color.White);
            if (fullPowerTime >= 0)
            {
                fullPowerTime += dt;
                spriteBatch.DrawString(fontfps, "Max Power Mode!!",
                    new Vector2(MathHelper.Clamp((float)fullPowerTime * 500 - 400, -1000, 100), 100.0f),
                    Color.Red);
                if (fullPowerTime >= 3) fullPowerTime = -1;
            }

            //End drawing sprites
            spriteBatch.End();
            base.Draw(gameTime);
        }



        public static class Player
        {
            public static AnimatedTexture img;
            public static string name = "reimu";
            public static Vector2 dir = Vector2.Zero;
            public static Vector2 pos;
            public static PlayerStatus status = PlayerStatus.Alive;
            public static SpriteEffects effect = SpriteEffects.None;
            public static float respawnDelay = 3.0f;
            public static float moveSpeed = 120.0f;
            public static float firerate1 = 0.1f;
            public static float firerate2 = 0.5f;
            public static float firedelay1 = 0.0f;
            public static float firedelay2 = 0.0f;
            public static int fireamount1 = 4;
            public static int fireamount2 = 4;
            public static bool focused = false;
            public static float fAngle = 0.0f;
            public static double bombDelay = -1;
            public static float getAngle(Vector2 p)
            {
                return -MathHelper.ToDegrees((float)Math.Atan2(p.X - pos.X, p.Y - pos.Y));
            }
            public static void kill()
            {
                if (bomb != null) return;
                status = PlayerStatus.Dead;
                sounds["death"].play();
                explosions.Add(new Explosion("explode", pos, 5.0f, 0.33f, 3.0f, Color.White));
                for (int i = 0; i < fireamount1 + 1; i++)
                {
                    items.Add(new Item("itempower", pos + new Vector2(random.Next(-50, 50),
                        random.Next(-50, 50)), -3.0f));
                    items.Add(new Item("itemhighpower", pos + new Vector2(random.Next(-50, 50),
                        random.Next(-50, 50)), -3.0f));
                }
                if (players == 1) items.Add(new Item("itemfullpower", pos, -3.0f));
                for (int i = 0; i < items.Count; i++) items[i].autoCollect = false;
                players--; bombs = 3; fireamount1 = 1; fireamount2 = 0; power /= 2; powerUp();
            }
            public static void update()
            {
                //Respawn the player in the center of the screen and give 5 sec. of invulnerability
                if (status == PlayerStatus.Dead)
                {
                    respawnDelay -= (float)dt;
                    if (respawnDelay <= 2)
                    {
                        for (int i = 0; i < eBullets.Count; i++)
                        {
                            explosions.Add(new Explosion("bulletexplode", eBullets[i].pos,
                                5.0f, 0.0f, 0.2f, eBullets[i].color));
                            eBullets.RemoveAt(i); i--;
                        }
                    }
                    if (respawnDelay <= 0)
                    {
                        respawnDelay = 5.0f;
                        pos = gameDim / 2; status = PlayerStatus.Spawning;
                    }
                }
                else
                {
                    //Player is alive after invulnerability period runs out
                    if (status == PlayerStatus.Spawning)
                    {
                        if (respawnDelay >= 2)
                            for (int i = 0; i < eBullets.Count; i++)
                        {
                            explosions.Add(new Explosion("bulletexplode", eBullets[i].pos,
                                5.0f, 0.0f, 0.2f, eBullets[i].color));
                            eBullets.RemoveAt(i); i--;
                        }
                        respawnDelay -= (float)dt;
                        if (respawnDelay <= 0)
                        {
                            respawnDelay = 3.0f;
                            status = PlayerStatus.Alive;
                        }
                    }
                    //Move the player
                    pos += dir * (float)dt;
                    //Keep the player on screen
                    pos.X = MathHelper.Clamp(pos.X, 0, gameDim.X);
                    pos.Y = MathHelper.Clamp(pos.Y, 0, gameDim.Y);
                    //Determine animations based on movement and current animation
                    if (img == reimuTextures[0])
                        img = reimuTextures[1];
                    if (img == reimuTextures[1] && img.willFinish(dt))
                        img = reimuTextures[2];
                    if (dir.X < 0 && playerEffect == SpriteEffects.FlipHorizontally)
                    { img = reimuTextures[1]; playerEffect = SpriteEffects.None; }
                    if (dir.X > 0 && playerEffect == SpriteEffects.None)
                    { img = reimuTextures[1]; playerEffect = SpriteEffects.FlipHorizontally; }
                    if (dir.X == 0) img = reimuTextures[0];
                    //Spin focus glyph
                    fAngle += 0.02f;
                    //Auto-collect items at full power
                    if (pos.Y < 150 && power == 128)
                        for (int i = 0; i < items.Count; i++) items[i].autoCollect = true;
                    
                }

            }
            public static void draw(float layer)
            {
                float[] transCycle = { 0.5f, 1.0f };
                float trans = 1.0f;
                if (status == PlayerStatus.Spawning) trans = transCycle[(int)(respawnDelay * 2 % 2.0f)];
                if (status == PlayerStatus.Dead) trans = 0.0f;
                spriteBatch.Draw(img.img, pos - img.dim / 2,
                img.getFrame(), Color.White * trans, 0.0f, Vector2.Zero, 1.0f, playerEffect, layer);
                //Draw hitbox if player focused
                if (focused && status != PlayerStatus.Dead)
                    spriteBatch.Draw(textures["focus"], pos, textures["focus"].Bounds,
                        Color.White, fAngle, new Vector2(textures["focus"].Width, textures["focus"].Height) / 2,
                        1.0f, playerEffect, layer - 0.01f);
            }
            public static bool powerUp()
            {
                bool powerup = false;
                if (power >= 128) power = 128;
                if (power >= 8 && fireamount2 <= 0) { fireamount2++; powerup = true; }
                if (power >= 16 && fireamount1 <= 1) { fireamount1++; powerup = true; }
                if (power >= 32 && fireamount2 <= 1) { fireamount2++; powerup = true; }
                if (power >= 48 && fireamount1 <= 2) { fireamount1++; powerup = true; }
                if (power >= 64 && fireamount2 <= 2) { fireamount2++; powerup = true; }
                if (power >= 96 && fireamount1 <= 3) { fireamount1++; powerup = true; }
                if (power >= 128 && fireamount2 <= 3) { fireamount2++; powerup = true; fullPowerTime = 0;
                for (int i = 0; i < eBullets.Count; i++)
                {
                    items.Add(new Item("itemstar", eBullets[i].pos, 0));
                    eBullets.RemoveAt(i); i--;
                }
                }
                return powerup; 
            }
            public static void readInput(KeyboardState keystate)
            {
                //Read keyboard input
                dir = Vector2.Zero;
                if (firedelay1 >= 0) firedelay1 -= (float)dt;
                if (firedelay2 >= 0) firedelay2 -= (float)dt;
                if (keystate.IsKeyDown(Keys.Left)) dir.X -= moveSpeed;
                if (keystate.IsKeyDown(Keys.Right)) dir.X += moveSpeed;
                if (keystate.IsKeyDown(Keys.Up)) dir.Y -= moveSpeed;
                if (keystate.IsKeyDown(Keys.Down)) dir.Y += moveSpeed;
                if (keystate.IsKeyDown(Keys.LeftShift)) { focused = true; dir /= 2; }
                else focused = false;
                if (status != PlayerStatus.Dead && conversation == null && bomb == null)
                {
                    if (keystate.IsKeyDown(Keys.Z) && firedelay1 < 0)
                    {
                        //Shoot regular bullets at fixed rate based on power when Z is pressed
                        switch (fireamount1)
                        {
                            case 1:
                                pBullets.Add(new Bullet(textures["bullet1"], pos,
                                0.0f, 750.0f, Color.Red, drawType.Directional));
                                break;
                            case 2:
                                pBullets.Add(new Bullet(textures["bullet1"], new Vector2(pos.X - 5, pos.Y),
                                0.0f, 750.0f, Color.Red, drawType.Directional));
                                pBullets.Add(new Bullet(textures["bullet1"], new Vector2(pos.X + 5, pos.Y),
                                0.0f, 750.0f, Color.Red, drawType.Directional));
                                break;
                            case 3:
                                pBullets.Add(new Bullet(textures["bullet1"], pos,
                                -10.0f, 750.0f, Color.Red, drawType.Directional));
                                pBullets.Add(new Bullet(textures["bullet1"], pos,
                                0.0f, 750.0f, Color.Red, drawType.Directional));
                                pBullets.Add(new Bullet(textures["bullet1"], pos,
                                10.0f, 750.0f, Color.Red, drawType.Directional));
                                break;
                            case 4:
                                pBullets.Add(new Bullet(textures["bullet1"], new Vector2(pos.X - 5, pos.Y),
                                0.0f, 750.0f, Color.Red, drawType.Directional));
                                pBullets.Add(new Bullet(textures["bullet1"], new Vector2(pos.X + 5, pos.Y),
                                0.0f, 750.0f, Color.Red, drawType.Directional));
                                pBullets.Add(new Bullet(textures["bullet1"], pos,
                                -10.0f, 750.0f, Color.Red, drawType.Directional));
                                pBullets.Add(new Bullet(textures["bullet1"], pos,
                                10.0f, 750.0f, Color.Red, drawType.Directional));
                                break;
                        }

                        sounds["playershoot"].play();
                        firedelay1 += firerate1;
                    }
                    if (keystate.IsKeyDown(Keys.Z) && firedelay2 < 0)
                    {
                        //Shoot homing bullets at fixed rate based on power when Z is pressed
                        if (fireamount2 >= 1)
                        {
                            pBullets.Add(new Bullet(textures["bullet2"], pos,
                            -30.0f, 750.0f, Color.Blue, drawType.Homing));
                            pBullets.Add(new Bullet(textures["bullet2"], pos,
                            30.0f, 750.0f, Color.Blue, drawType.Homing));
                        }
                        if (fireamount2 >= 2)
                        {
                            pBullets.Add(new Bullet(textures["bullet2"], pos,
                            -50.0f, 750.0f, Color.Blue, drawType.Homing));
                            pBullets.Add(new Bullet(textures["bullet2"], pos,
                            50.0f, 750.0f, Color.Blue, drawType.Homing));
                        }
                        if (fireamount2 >= 3)
                        {
                            pBullets.Add(new Bullet(textures["bullet2"], pos,
                            -70.0f, 750.0f, Color.Blue, drawType.Homing));
                            pBullets.Add(new Bullet(textures["bullet2"], pos,
                            70.0f, 750.0f, Color.Blue, drawType.Homing));
                        }
                        if (fireamount2 >= 4)
                        {
                            pBullets.Add(new Bullet(textures["bullet2"], pos,
                            -90.0f, 750.0f, Color.Blue, drawType.Homing));
                            pBullets.Add(new Bullet(textures["bullet2"], pos,
                            90.0f, 750.0f, Color.Blue, drawType.Homing));
                        }
                        firedelay2 += firerate2;
                    }
                    if (keystate.IsKeyDown(Keys.X) && bombs > 0)
                    {
                        bomb = new Bomb("Spirit Sign", "Fantasy Orb", "reimu", 9.0, 0); bombs--;
                    }
                }
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

        public class Sprite
        {
            public Texture2D img;
            public AnimatedTexture imgA;
            public Rectangle rect;
            public Vector2 dim;
            public Vector2 pos;
            public Vector2 dir;
            public float speed;
            public float angle;
            public float radians;
            public Color color;
            public drawType type;
            public Sprite(Texture2D t, Vector2 p, float a, float s, Color c, drawType ty)
            {
                img = t; pos = p; angle = a - 90.0f; speed = s; type = ty;
                radians = MathHelper.ToRadians(angle);
                dir = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
                dim = new Vector2(t.Width, t.Height); color = c;
            }
            public Sprite(Texture2D t, Vector2 p, Vector2 d, Color c, drawType ty)
            {
                img = t; pos = p; dir = d; type = ty;
                angle = MathHelper.ToDegrees((float)Math.Atan2(dir.Y, dir.X))+90.0f;
                speed = (float)Math.Sqrt(d.X * d.X + d.Y * d.Y);
                dim = new Vector2(t.Width, t.Height); color = c;
            }
            public Sprite(AnimatedTexture t, Vector2 p, float a, float s, Color c, drawType ty)
            {
                imgA = t; pos = p; angle = a - 90.0f; speed = s; type = ty;
                radians = MathHelper.ToRadians(angle);
                dir = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
                dim = t.dim; color = c;
            }
            public Sprite(AnimatedTexture t, Vector2 p, Vector2 d, Color c, drawType ty)
            {
                imgA = t; pos = p; dir = d; type = ty;
                angle = MathHelper.ToDegrees((float)Math.Atan2(dir.Y, dir.X)) + 90.0f;
                speed = (float)Math.Sqrt(d.X * d.X + d.Y * d.Y);
                dim = t.dim; color = c;
            }
            virtual public bool update()
            {
                //Move the sprite
                pos += dir * speed * (float)dt;
                //Return false if sprite off-screen
                if (pos.X < -50 || pos.X > gameDim.X + 50 || pos.Y < -50 || pos.Y > gameDim.Y + 50)
                    return false;
                return true;
            }
            virtual public void draw(float layer)
            {
                switch (type)
                {
                    case drawType.Normal:
                        spriteBatch.Draw(img, pos - dim / 2, img.Bounds,
                        Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layer);
                        break;
                    case drawType.Directional:
                        spriteBatch.Draw(img, pos, img.Bounds,
                        Color.White, MathHelper.ToRadians(angle + 90.0f), dim / 2, 1.0f,
                        SpriteEffects.None, layer);
                        break;
                    case drawType.Homing:
                        homeIn();
                        spriteBatch.Draw(img, pos, img.Bounds,
                        Color.White, MathHelper.ToRadians(angle + 90.0f), dim / 2, 1.0f,
                        SpriteEffects.None, layer);
                        break;
                    case drawType.Animated:
                        spriteBatch.Draw(imgA.img, pos - dim / 2, imgA.getFrame(),
                        Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layer);
                        break;
                    case drawType.AnimatedDirectional:
                        spriteBatch.Draw(imgA.img, pos, imgA.getFrame(),
                        Color.White, MathHelper.ToRadians(angle + 90.0f), dim / 2, 1.0f,
                        SpriteEffects.None, layer);
                        break;
                }
            }
            public void homeIn()
            {
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
            }
        }

        public class Bullet : Sprite
        {
            public float hitRadius;
            public bool hasGrazed = false;
            //Constructor given angular direction
            public Bullet(Texture2D t, Vector2 p, float a, float s, Color c, drawType ty)
                : base(t, p, a, s, c, ty) { }
            //Constructor given vector direction
            public Bullet(Texture2D t, Vector2 p, Vector2 d, Color c, drawType ty)
                : base(t, p, d, c, ty) { }
            public Bullet(AnimatedTexture t, Vector2 p, float a, float s, Color c, drawType ty)
                : base(t, p, a, s, c, ty) { }
            //Constructor given vector direction
            public Bullet(AnimatedTexture t, Vector2 p, Vector2 d, Color c, drawType ty)
                : base(t, p, d, c, ty) { }
            //Check for collisions
            public bool collide()
            {   
                //Check for bullet graze
                if (Math.Abs(pos.X - Player.pos.X) < Player.img.dim.X / 2 &&
                    Math.Abs(pos.Y - Player.pos.Y) < Player.img.dim.Y / 2 &&
                    Player.status != PlayerStatus.Dead && !hasGrazed && bomb == null)
                {
                    hasGrazed = true; graze++; sounds["graze"].play();
                }
                if (Math.Sqrt((Player.pos.X - pos.X) * (Player.pos.X - pos.X) +
                    (Player.pos.Y - pos.Y) * (Player.pos.Y - pos.Y)) < 5.0 &&
                    Player.status != PlayerStatus.Dead)
                {
                    explosions.Add(new Explosion("bulletexplode", pos,
                        5.0f, 0.0f, 0.2f, color));
                    //If the player is alive, kill the player and create large explosion
                    if (Player.status == PlayerStatus.Alive) Player.kill();
                    //Return true to indicate collision
                    return true;
                }
                return false;
            }
        }
        public class Enemy : Sprite
        {
            public float health;
            public SpriteEffects effect = SpriteEffects.None;
            public Script[] scripts;
            public string[] drops;
            //Constructor given angular direction
            public Enemy(AnimatedTexture t, Vector2 p, float a, float s, Color c, float h, Script scr, string[] dr)
                : base(t, p, a, s, c, drawType.Animated)
            {
                health = h; scripts = new Script[1]; scripts[0] = scr; drops = dr;
            }
            //Constructor given vector direction
            public Enemy(AnimatedTexture t, Vector2 p, Vector2 d, Color c, float h, Script scr, string[] dr)
                : base(t, p, d, c, drawType.Animated)
            {
                health = h; scripts = new Script[1]; scripts[0] = scr; drops = dr;
            }
            //Constructor given angular direction
            public Enemy(AnimatedTexture t, Vector2 p, float a, float s, Color c, float h, Script[] scr, string[] dr)
                : base(t, p, a, s, c, drawType.Animated)
            {
                health = h; scripts = scr; drops = dr;
            }
            //Constructor given vector direction
            public Enemy(AnimatedTexture t, Vector2 p, Vector2 d, Color c, float h, Script[] scr, string[] dr)
                : base(t, p, d, c, drawType.Animated)
            {
                health = h; scripts = scr; drops = dr;
            }
            //Update method to move the enemy and check for collisions
            override public bool update()
            {
                //Move the sprite
                pos += dir * speed * (float)dt;
                //Have the enemy shoot
                for (int i = 0; i < scripts.Length; i++)
                    scripts[i].run(this.pos);
                //Return false if sprite off-screen
                if (pos.X < -50 || pos.X > gameDim.X + 50 || pos.Y < -50 || pos.Y > gameDim.Y + 50)
                    return false;
                return true;
            }
            public bool collide()
            {
                //Check for enemy collisions with player (but only if the player is alive)
                if (Math.Abs(pos.X - Player.pos.X) < dim.X - 10 &&
                    Math.Abs(pos.Y - Player.pos.Y) < dim.Y - 10 &&
                    Player.status == PlayerStatus.Alive) Player.kill();
                //Check for enemy collisions with player bullets
                for (int i = 0; i < pBullets.Count; i++)
                {
                    if (Math.Abs(pBullets[i].pos.X - pos.X) < dim.X &&
                        Math.Abs(pBullets[i].pos.Y - pos.Y) < dim.Y)
                    {
                        //Destroy bullet and damage enemy
                        pBullets.RemoveAt(i); i--;
                        if (spellcard == null || spellcard != null && spellcard.time > 3) health -= 1.0f;
                        sounds["damage"].play();
                        if (health <= 0)
                        {
                            explode(); return true;
                        }
                    }
                }
                return false;
            }
            virtual public void explode()
            {
                //Destroy enemy and create explosion and item
                explosions.Add(new Explosion("explode",pos, 2.0f, 2.0f, 0.5f, color));
                score += 1000;
                for (int i = 0; i < drops.Length; i++)
                    if (drops[i] == "itemfullpower" || drops[i] == "itembomb" || drops[i] == "itemplayer")
                        items.Add(new Item(drops[i], pos, -1.5f));
                    else
                        items.Add(new Item(drops[i], pos + new Vector2(random.Next(-drops.Length * 10, drops.Length * 10),
                        random.Next(-drops.Length * 5, drops.Length * 5)), -1.5f));
                sounds["explodesound"].play();
            }
        }
        public class Boss : Enemy
        {
            public static bool isEntering = false;
            //Constructor given angular direction
            public Boss(AnimatedTexture t, Vector2 p, float a, float s, Color c, int h, Script[] scr, string[] dr)
                : base(t, p, a, s, c, h, scr, dr)
            {
                imgA = t; pos = p; angle = a - 90.0f; speed = s; health = h; scripts = scr;
                radians = MathHelper.ToRadians(angle);
                dir = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
                dim = imgA.dim; color = c;
            }
            //Constructor given vector direction
            public Boss(AnimatedTexture t, Vector2 p, Vector2 d, Color c, int h, Script[] scr, string[] dr)
                : base(t, p, d, c, h, scr, dr)
            {
                imgA = t; pos = p; dir = d; health = h; scripts = scr;
                angle = MathHelper.ToDegrees((float)Math.Atan2(dir.Y, dir.X)) + 90.0f;
                speed = (float)Math.Sqrt(d.X * d.X + d.Y * d.Y);
                dim = imgA.dim; color = c;
            }
            //Update method to move the boss and check for collisions
            override public bool update()
            {
                //Move the sprite
                pos += dir * speed * (float)dt;
                //Have the enemy shoot
                if (conversation == null && spellcard == null)
                    for (int i = 0; i < scripts.Length; i++) scripts[i].run(boss.pos);
                if (health < 750 && spellcard == null)
                    spellcard = new Spellcard("Star Sign", "Radiant Star Crosses", "marisa", 30,
                    new List<Background> {new Background(textures["bg1"],Vector2.Zero,1.0f),
                        new Background(textures["bg2"],new Vector2(-50,0),0.99f)},
                        new Script[] {bossScript1});
                //Return false if sprite off-screen
                if (pos.X < -50 || pos.X > gameDim.X + 50 || pos.Y < -50 || pos.Y > gameDim.Y + 50)
                    return false;
                return true;
            }
            //Method to make the boss enter the screen
            public void enter()
            {
                pos.X += (gameDim.X / 2- pos.X) / gameDim.X * 10 + 0.5f;
                if (pos.X >= gameDim.X / 2)
                {
                    isEntering = false; pos.X = gameDim.X / 2;
                }
            }
            override public void explode()
            {
                //Destroy boss and create lots of items and huge explosion
                explosions.Add(new Explosion("explode", pos, 10.0f, 0.2f, 5.0f, Color.Gold));
                score += 1000000;
                sounds["defeat"].play();
                for (int j = 0; j < 50; j++)
                {
                    items.Add(new Item("itempoint",
                        boss.pos + new Vector2(random.Next(-50, 50), random.Next(-50, 50)), -1.5f));
                }
                items.Add(new Item("itemplayer", boss.pos, -1.5f));
                boss = null; spellcard = null;
            }
        }
        public class Item : Sprite
        {
            float speed;
            string item;
            public bool autoCollect = false;
            public Item(string i, Vector2 p, float s)
                : base(textures[i], p, Vector2.Zero, Color.White, drawType.Normal)
            {
                img = textures[i];
                item = i; pos = p; speed = s;
                dim = new Vector2(img.Width, img.Height);
                if (i == "itemstar" || bomb != null) autoCollect = true;
            }
            override public bool update()
            {
                if (autoCollect)
                {
                    //Auto collect item
                    radians = MathHelper.ToRadians(Player.getAngle(pos) - 90.0f);
                    pos += new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians)) * 300.0f *(float)dt;
                }
                else
                {
                    //Cause the item to fall
                    speed += (float)dt;
                    if (speed >= 1.5f) speed = 1.5f;
                    pos.Y += speed;
                }
                //Draw arrow if item above screen
                if (pos.Y < 0)
                {
                    Texture2D arrowImg = textures[item + "arrow"];
                    spriteBatch.Draw(arrowImg, new Vector2(pos.X - arrowImg.Width / 2, 0), Color.White);
                }
                //Return false if item off-screen
                if (pos.Y > gameDim.Y + 10)
                    return false;
                //Check for item collisions with player (but only if the player is not dead)
                if (Math.Abs(pos.X - Player.pos.X) < Player.img.dim.X - 10 &&
                    Math.Abs(pos.Y - Player.pos.Y) < Player.img.dim.Y - 20 &&
                    Player.status != PlayerStatus.Dead)
                //Destroy item upon collision with player and create text
                {
                    sounds["item"].play();
                    switch(item)
                    {
                        case "itempower":
                            scoreTexts.Add(new ScoreText(10, pos, Color.White));
                            power++;
                            if (Player.powerUp())
                            {
                                powerText = new PowerText(textures["powerup"], pos, Color.White);
                                sounds["powersound"].play();
                            }
                            break;
                        case "itemhighpower":
                            scoreTexts.Add(new ScoreText(100, pos, Color.White));
                            power += 10;
                            if (Player.powerUp())
                            {
                                powerText = new PowerText(textures["powerup"], pos, Color.White);
                                sounds["powersound"].play();
                            }
                            break;
                        case "itemfullpower":
                            scoreTexts.Add(new ScoreText(1000, pos, Color.White));
                            power = 128;
                            if (Player.powerUp())
                            {
                                powerText = new PowerText(textures["powerup"], pos, Color.White);
                                sounds["powersound"].play();
                            }
                            break;
                        case "itembomb": bombs++; break;
                        case "itemplayer": players++; sounds["1up"].play(); break;
                        case "itempoint":
                            point++;
                            if (Player.pos.Y < 150) scoreTexts.Add(new ScoreText(100000, pos, Color.Yellow));
                            else scoreTexts.Add(new ScoreText(100000 - (int)Player.pos.Y * 100, pos, Color.White));
                        break;
                        case "itemstar": scoreTexts.Add(new ScoreText(1000+graze*10, pos, Color.White)); break;
                    }
                    //Return false to indicate item destruction
                    return false;
                }
                return true;
            }
        }
        public class Orb : Sprite
        {
            float damage;
            double totalTime;
            double time = 0;
            Color c;
            public Orb(Texture2D t, Vector2 p, float a, float s, Color c, drawType ty, float d, float ti)
                : base(t, p, a, s, c, ty)
            {
                damage = d; totalTime = ti; this.c = c;
            }
            public bool update()
            {
                //Move the sprite
                pos += dir * speed * (float)dt;
                time += dt;
                if (time > totalTime) return false;
                c.R = (byte)(time * -15); c.B = (byte)(time * 15);
                float minDist = 10000.0f;
                float minAngle = 0.0f;
                for (int i = 0; i < enemies.Count; i++)
                {
                    float dist = (float)Math.Sqrt((enemies[i].pos.X - pos.X) * (enemies[i].pos.X - pos.X) +
                        (enemies[i].pos.Y - pos.Y) * (enemies[i].pos.Y - pos.Y));
                    if (dist < 30)
                    {
                        enemies[i].health -= damage * (float)dt * 60.0f; sounds["damage"].play();
                    }
                    if (enemies[i].health < 0)
                    {
                        enemies[i].explode(); enemies.RemoveAt(i); i--; continue;
                    }
                    if (dist < minDist)
                    {
                        minAngle = MathHelper.ToDegrees((float)Math.Atan2((enemies[i].pos.Y - pos.Y),
                            (enemies[i].pos.X - pos.X)));
                        minDist = dist;
                    }
                }
                for (int i = 0; i < eBullets.Count; i++)
                {
                    Bullet b = eBullets[i];
                    if (Math.Sqrt((b.pos.X - pos.X) * (b.pos.X - pos.X) +
                        (b.pos.Y - pos.Y) * (b.pos.Y - pos.Y)) < 50)
                    {
                        items.Add(new Item("itemstar", b.pos, 0)); eBullets.RemoveAt(i); i--; 
                    }
                }
                if (time < 0.5) return true;
                float radians;
                if (minDist < 10000.0f) radians = MathHelper.ToRadians(minAngle);
                else radians = MathHelper.ToRadians(Player.getAngle(pos)-90.0f);
                dir += new Vector2((float)Math.Cos(radians),
                        (float)Math.Sin(radians)) / 20.0f;
                dir.X = MathHelper.Clamp(dir.X, -1, 1);
                dir.Y = MathHelper.Clamp(dir.Y, -1, 1);
                return true;
            }
            public override void draw(float layer)
            {
                spriteBatch.Draw(img, pos - dim / 2, img.Bounds, c, 0.0f,
                        Vector2.Zero, (float)Math.Sin(time*3)*0.2f+1.0f, SpriteEffects.None, layer);
            }
        }

        public class Explosion
        {
            public Texture2D img;
            public Vector2 dim;
            public Vector2 pos;
            public float expansionRate;
            public float alphaRate;
            public float totalTime;
            public float expandAmount = 0.5f;
            public float alphaAmount = 1.0f;
            public float time = 0.0f;
            public Color color;
            //Constructor taking texture file, position, expansion rate, alpha rate, total time, and color.
            public Explosion(string i, Vector2 p, float e, float a, float t, Color c)
            {
                img = getColoredTexture(i, c); pos = p; dim = new Vector2(textures[i].Width, textures[i].Height);
                expansionRate = e; alphaRate = a; totalTime = t; color = c;
            }
            public bool update()
            {
                expandAmount += (float)dt * expansionRate;
                alphaAmount -= (float)dt * alphaRate;
                time += (float)dt;
                if (totalTime <= time) return false;
                return true;
                
            }
            public void draw(float layer)
            {
                spriteBatch.Draw(img, pos - dim * expandAmount / 2, img.Bounds, new Color(255, 255, 255, alphaAmount),
                    0.0f, Vector2.Zero, expandAmount, SpriteEffects.None, layer);
            }
        }

        public class Spellcard
        {
            public string type;
            public string name;
            public string owner;
            public Script[] scripts;
            public List<Background> backgrounds;
            public double time = 0.0;
            public double totalTime;
            public Spellcard(string t, string n, string o, double ti, List<Background> b, Script[] scr)
            {
                type = t; name = n; owner = o; totalTime = ti; scripts = scr; backgrounds = b;
                sounds["spellcard"].play();
                if (owner == Player.name)
                    explosions.Add(new Explosion("explode", Player.pos, 5.0f, 0.33f, 3.0f, Color.White));
                else
                    explosions.Add(new Explosion("explode", boss.pos, 5.0f, 0.33f, 3.0f, Color.White));
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].autoCollect = true;
                }
                if (owner != Player.name)
                    for (int i = 0; i < eBullets.Count; i++)
                    {
                        items.Add(new Item("itemstar", eBullets[i].pos, 0));
                        eBullets.RemoveAt(i); i--;
                    }
            }
            virtual public bool update()
            {
                time += dt;
                if (time < 3)
                {
                    new Vector2(gameDim.X - 50 - textures[owner + "2"].Width,
                       gameDim.Y - textures[owner + "2"].Height);
                    spriteBatch.Draw(textures[owner + "2"], new Vector2(50, gameDim.Y - textures[owner + "2"].Height),
                        textures[owner + "2"].Bounds, Color.White, 0.0f, Vector2.Zero,
                        MathHelper.Clamp(1.5f - (float)time, 1.0f, 2.0f), SpriteEffects.None, 0.02f);
                    spriteBatch.Draw(textures["spellcardtext"], new Vector2(100,200),
                        textures["spellcardtext"].Bounds, Color.White, 0.0f, Vector2.Zero,
                        1.0f, SpriteEffects.None, 0.01f);
                    spriteBatch.DrawString(fontconv, type + " \"" + name + "\"", new Vector2(100, 200), Color.White);
                }
                else
                {
                    for (int i = 0; i < scripts.Length; i++) scripts[i].run(boss.pos);
                    spriteBatch.Draw(textures["spellcardtext"], new Vector2(10, 30),
                        textures["spellcardtext"].Bounds, Color.White, 0.0f, Vector2.Zero,
                        1.0f, SpriteEffects.None, 0.01f);
                    spriteBatch.DrawString(fontconv, type + " \"" + name + "\"", new Vector2(20, 30), Color.White);
                }
                spriteBatch.DrawString(fontfps, Math.Ceiling(totalTime - time).ToString(),
                    new Vector2(gameDim.X - 20, 0), Color.SkyBlue);
                if (time > totalTime) return false;
                return true;
            }
        }
        public class Bomb : Spellcard
        {
            int script;
            public Bomb(string t, string n, string o, double ti, int scr)
                : base(t, n, o, ti, null, null)
            {
                script = scr;
                switch (scr)
                {
                    case 0:
                        for (int i = 0; i < 10; i++)
                            orbs.Add(new Orb(textures["bomb1"], Player.pos, i * 36, 200, Color.Red,
                                drawType.Normal, 0.5f, 8.0f));
                        break;
                }
            }
            override public bool update()
            {
                time += dt;
                if (time < 3)
                    spriteBatch.Draw(textures[owner + "2"], new Vector2(gameDim.X - 50 - textures[owner + "2"].Width,
                       gameDim.Y - textures[owner + "2"].Height),
                        textures[owner + "2"].Bounds, Color.White, 0.0f, Vector2.Zero,
                        MathHelper.Clamp(1.5f - (float)time, 1.0f, 2.0f), SpriteEffects.None, 0.01f);
                spriteBatch.DrawString(fontconv, type + " \"" + name + "\"", new Vector2(50, 460), Color.Red);
                if (time > totalTime) return false;
                return true;
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
                score += (int)amount;
            }
            public bool update()
            {
                pos.Y -= (float)dt * 10;
                time -= (float)dt;
                if (time < 0) return false;
                return true;
            }
        }
        public class PowerText
        {
            public Texture2D text;
            public Vector2 pos;
            public Color color;
            public float time = 1.5f;
            public PowerText(Texture2D t, Vector2 p, Color c)
            {
                text = t; pos = p; color = c;
            }
            public bool update()
            {
                pos.Y -= (float)dt * 10;
                time -= (float)dt;
                if (time < 0) return false;
                return true;
            }
        }

        public class Conversation
        {
            public string[] conv;
            public int line = 0;
            bool next = false;
            public string talk;
            public Texture2D[] images = { null, null };
            public string[] characters = { null, null };
            int move = 0; int[] pos = { 0, 0 };
            int[] cutin = { 0, 0 };
            char[] delimiters = { '-', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            Vector2 textboxScale = new Vector2(1.0f, 0.0f);
            //Initializer taking the conversation data list
            public Conversation(string[] c)
            {
                conv = c;
                if (keystate.IsKeyDown(Keys.Z)) next = true; //GLITCH, FIX LATER
            }
            //Draws the conversation
            public bool show()
            {
                //Opens up the text box at the beginning
                if (textboxScale.Y < 1.0f) textboxScale.Y += 0.02f;
                else
                {
                    //When finished opening, draw conversation text and images
                    textboxScale.Y = 1.0f; return this.showConv();
                }
                spriteBatch.Draw(textures["textbox"], new Vector2(10, 400), textures["textbox"].Bounds,
                        Color.White, 0.0f, Vector2.Zero,textboxScale, SpriteEffects.None, 0.05f);
                return true;
            }
            private bool showConv()
            {
                //Programming detecting only when the Z key is pressed from being up
                if (keystate.IsKeyDown(Keys.Z))
                {
                    //Moves to the next line when Z is pressed
                    if (!next) line++;
                    next = true;
                }
                else next = false;
                //Ends conversation if all lines have been read
                if (line >= conv.Length) return false;

                //Lines starting with "-" denote an image change
                if (conv[line].StartsWith("-"))
                {
                    //Finds the character the image belongs to
                    talk = conv[line].Split(delimiters)[1];
                    //Puts the character into the appropriate conversation spot if this is their first time talking
                    if (characters[0] == null) characters[0] = talk;
                    else if (characters[1] == null) characters[1] = talk;
                    //Finds the conversation spot of the talking character and changes their image
                    if (characters[0] == talk)
                        images[0] = textures[conv[line].Substring(1)];
                    else if (characters[1] == talk)
                        images[1] = textures[conv[line].Substring(1)];
                    //Moves to the next line and continues the conversation
                    line++; this.showConv(); return true;
                }
                //Lines starting with "*" denote a special command
                else if (conv[line].StartsWith("*"))
                {
                    string[] command = conv[line].Substring(1).Split(':');
                    if (command[0] == "Music") MediaPlayer.Play(songs[command[1]]);
                    if (command[0] == "Enter") Boss.isEntering = true;
                    if (command[0] == "Speed")
                        for (int i = 0; i < backgrounds.Count; i++)
                            backgrounds[i].scrollDir *= Convert.ToInt32(command[1]);
                    Console.WriteLine(conv[line].Substring(1)); line++; this.showConv(); return true;
                }
                else
                {
                    //Determines how character images should move based on who's talking
                    if (talk == characters[0]) move = 1;
                    else move = -1;
                    //Moves character images
                    pos[0] += move;
                    pos[1] += move;
                    pos[0] = (int)MathHelper.Clamp(pos[0], -10, 0);
                    pos[1] = (int)MathHelper.Clamp(pos[1], 0, 10);
                    //Draws the left side character image
                    if (images[0] != null)
                    {
                        spriteBatch.Draw(images[0], new Vector2(pos[0], gameDim.Y - cutin[0] - pos[0]),
                            images[0].Bounds, Color.White * (pos[0] * 0.05f + 1.0f), 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.06f);
                        //Causes the player to move up when they first enter the conversation
                        cutin[0] += 10;
                        cutin[0] = (int)MathHelper.Clamp(cutin[0], 0, images[0].Height);
                    }
                    //Draw the right side character image
                    if (images[1] != null)
                    {
                        spriteBatch.Draw(images[1], new Vector2(gameDim.X - images[1].Width + pos[1], gameDim.Y - cutin[1] + pos[1]),
                            images[1].Bounds, Color.White * (pos[1] * -0.05f + 1.0f), 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.06f);
                        //Causes the player to move up when they first enter the conversation
                        cutin[1] += 10;
                        cutin[1] = (int)MathHelper.Clamp(cutin[1], 0, images[1].Height);
                    }
                    //Draws the text box
                    spriteBatch.Draw(textures["textbox"], new Vector2(10, 400), textures["textbox"].Bounds,
                        Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.05f);
                    //Draws the text
                    spriteBatch.DrawString(fontconv, conv[line], new Vector2(10, 400), convColors[talk]);
                    return true;
                }

            }


        }

        public class Script
        {
            public Texture2D img = textures["bullet1"];
            public Color color;
            public float angle;
            public float speed;
            public double time = 0.0;
            public Loop[] loopData;
            public List<Loop> loops = new List<Loop>();
            public Script(Texture2D i, Color c, float a, float s, Loop[] l)
            {
                img = i; speed = s; angle = a; color = c;
                loopData = l; loops.Add(new Loop(loopData[0].totalLoops, loopData[0].loopDelay,
                                  loopData[0].loopScript, 0.0));
            }
            public Script(Script s)
            {
                img = s.img; speed = s.speed; angle = s.angle; color = s.color;
                loopData = s.loopData; loops.Add(new Loop(loopData[0].totalLoops, loopData[0].loopDelay,
                                  loopData[0].loopScript, 0.0));
            }
            public void run(Vector2 sourcePos)
            {
                for (int i = 0; i < loops.Count; i++)
                {
                    int loopLevel = loops[i].loopScript - loopData[0].loopScript;
                    if (time >= loops[i].nextLoopTime)
                    {
                        if (loops[i].run(this,sourcePos))
                        {
                            loops[i].nextLoopTime = loops[i].loopDelay + time;
                            if (loopLevel >= loopData.Length - 1)
                            {
                                eBullets.Add(new Bullet(img,
                                sourcePos, angle, speed, color, drawType.Directional));
                                sounds["enemyshoot3"].play();
                                explosions.Add(new Explosion("shoot", sourcePos,
                                    -1.0f, 0.0f, 0.3f, color));
                            }
                            else
                            {
                                loops.Add(new Loop(loopData[loopLevel + 1].totalLoops,
                                    loopData[loopLevel + 1].loopDelay,
                                   loopData[loopLevel + 1].loopScript, time));
                            }
                            i--;
                            
                        }
                        else loops.RemoveAt(i);
                    }
                }
                time += dt;
            }
        }

        public class Loop
        {
            public int totalLoops;
            public int loopNum;
            public double loopDelay;
            public double nextLoopTime;
            public int loopScript;
            public Loop(int tLoops, double lDelay, int lScript, double time)
            {
                totalLoops = tLoops; loopDelay = lDelay; loopScript = lScript; loopNum = 0;
            }
            public bool run(Script script, Vector2 sourcePos)
            {
                if (loopNum >= totalLoops) return false;
                if (loopNum > 0) runScript(script, sourcePos);
                loopNum++;
                return true;
                
            }
            public void runScript(Script script, Vector2 sourcePos)
            {
                switch (loopScript)
                {
                    case 0:
                        script.angle = (float)random.NextDouble() * 360.0f;
                        break;
                    case 1:
                        script.angle = Player.getAngle(sourcePos) - 60.0f;
                        break;
                    case 2:
                        script.angle += 30.0f;
                        break;
                    case 3:
                        script.angle = (float)random.NextDouble() * 360.0f;
                        break;
                    case 4:
                        script.angle += 18.0f;
                        break;
                    case 5:
                        script.angle = (float)random.NextDouble() * 360.0f;
                        script.speed = (float)random.Next(50,100);
                        break;
                    case 6: break;
                    case 7:
                        script.angle = (float)random.NextDouble() * 360.0f;
                        break;
                    
                }
            }
        }

        public class Background
        {
            public Texture2D img;
            public Vector2 scrollDir;
            public Vector2 scrollPos;
            public float layer;
            public Background(Texture2D i, Vector2 sd, float l)
            {
                scrollDir = sd; img = i; layer = l;
            }
            public void draw()
            {
                scrollPos += scrollDir * (float)dt;
                scrollPos = new Vector2(scrollPos.X % img.Width, scrollPos.Y % img.Height);
                if (scrollPos.X > 0) scrollPos.X -= img.Width;
                if (scrollPos.Y > 0) scrollPos.Y -= img.Height;
                for (int x = (int)scrollPos.X; x < gameDim.X; x += img.Width)
                    for (int y = (int)scrollPos.Y; y < gameDim.Y; y += img.Height)
                        spriteBatch.Draw(img, new Vector2(x,y), img.Bounds,
                        Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layer);
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
            //Constructor taking texture file, number of frames, and frame rate
            public AnimatedTexture(Texture2D t, int f, double s)
            {
                img = t; frames = f; delay = speed = s;
                dim = new Vector2(t.Width / frames, t.Height);
            }
            //Constructor making a new instance as a copy. Use this to make several instances of
            //AnimatedTexture to animate seperately.
            public AnimatedTexture(AnimatedTexture a)
            {
                img = a.img;  frames = a.frames; delay = a.delay; speed = a.speed; delay = speed; dim = a.dim;
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
                return new Rectangle((int)dim.X * frame, 0, (int)dim.X, (int)dim.Y);
            }
            //Detects if an animation is about to finish.
            public bool willFinish(double dt)
            {
                bool willfinish = (delay - dt < 0 && frame + 1 == frames);
                if (willfinish) { frame = 0; delay += speed; }
                return willfinish;
            }
        }
    
    
    }
}