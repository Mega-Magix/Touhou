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
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace Touhou.Battle
{

    public class Enemy
    {
        // Collision radius
        public int radius = 0;

        public Vector2 position = Vector2.Zero;
        public Vector2 velocity = Vector2.Zero;

        public Effect.AnimatedTexture animation;

        public Game game;
        public Level level;

        public Boolean destroyed = false;

        public int health;

        List<float> scriptWait = new List<float>();
        List<float> scriptDelay = new List<float>();
        List<int> scriptLoops = new List<int>();
        List<string> scripts = new List<string>();

        Texture2D bulletTexture;

        public Enemy(Game game, Level level, int x, int radius, int speedX, int speedY, int health)
        {
            this.game = game;
            this.level = level;

            this.radius = radius;

            this.health = health;

            Texture2D animationTexture;
            animationTexture = game.Content.Load<Texture2D>("enemy1fly");
            bulletTexture = game.Content.Load<Texture2D>("testbullet");

            position.X = x;
            position.Y = -animationTexture.Height;
            velocity.X = speedX;
            velocity.Y = speedY;

            animation = new Effect.AnimatedTexture(animationTexture, position, 4, 0.2);
        }

        public void AddScript(string scriptName, float wait, float delay, int loops)
        {
            scriptWait.Add(wait);
            scriptDelay.Add(delay);
            scriptLoops.Add(loops);
            //scripts.Add(scriptName);
            //var python = Python.CreateEngine();
            //ScriptScope scope = python.CreateScope();
            //ScriptSource source = python.CreateScriptSourceFromFile("../../Scripts/Scripts/" + scriptName + ".py");
            //scope.SetVariable("enemy", this);
            //scope.SetVariable("level", level);
            //scriptSources.Add(source);
            //scriptScopes.Add(scope);
        }

        public Vector2 GetCenter()
        {
            return new Vector2(GetCenterX(), GetCenterY());
        }

        public float GetCenterX()
        {
            return position.X + animation.width / 2;
        }
        public float GetCenterY()
        {
            return position.Y + animation.height / 2;
        }

        public void Damage(Bullet bullet)
        {
            health -= bullet.damage;
            if (health <= 0)
            {
                Effect.Explosion explosion = new Effect.Explosion(game, "explodeblue", GetCenterX(), GetCenterY(), 2.0f, 0.5f);
                level.AddExplosion(explosion);
                Destroy();
            }
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

            for (int i = 0; i < scripts.Count; i++)
            {
                scriptWait[i] -= dt;
                if (scriptWait[i] <= 0.0f && scriptLoops[i] > 0)
                {
                    scriptLoops[i]--;
                    scriptWait[i] = scriptDelay[i];
                    scriptSources[i].Execute(scriptScopes[i]);
                }
            }

            if (position.Y >= level.screenHeight)
                Destroy();
        }
    }
}