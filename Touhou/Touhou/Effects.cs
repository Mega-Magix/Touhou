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

namespace Touhou.Effects
{

    public class AnimatedTexture
    {
        public Texture2D texture;
        public Vector2 dimensions;
        public Vector2 position;

        public int numFrames;
        public int frame = 0;
        public double wait;
        public double delay;

        int realFrame;

        // List of the frame indices to animate over
        public List<int> animationFrames;
        // List of the frame indices to animate over when this current one is finished
        public List<int> nextAnimationFrames;
        public int nextNumFrames;
        public Texture2D nextTexture;

        public Rectangle rectangle;

        public AnimatedTexture(Texture2D texture, Vector2 position, int numFrames, double frameRate)
        {
            this.texture = texture;
            this.nextTexture = texture;
            this.position = position;
            this.numFrames = numFrames;
            this.nextNumFrames = numFrames;
            this.delay = this.wait = frameRate;

            resetAnimationList();

            this.dimensions = new Vector2(texture.Width / numFrames, texture.Height);
        }

        public AnimatedTexture(Texture2D texture, Vector2 position, List<int> animationFrames, double frameRate)
        {
            this.texture = texture;
            this.nextTexture = texture;
            this.position = position;
            this.numFrames = animationFrames.Count;
            this.nextNumFrames = this.numFrames;
            this.delay = this.wait = frameRate;

            this.animationFrames = animationFrames;

            this.dimensions = new Vector2(texture.Width / numFrames, texture.Height);
        }

        // Populates the animation lits with a full set of frames
        public void resetAnimationList()
        {
            this.animationFrames = new List<int>();
            int i = 0;
            for (i = 0; i < numFrames; i++)
                this.animationFrames.Add(i);

            this.nextAnimationFrames = new List<int>(this.animationFrames);

            resetAnimation();
        }

        public void resetAnimation()
        {
            // Call this if you happen to change the animation frames list
            frame = 0;
            delay = wait;
            rectangle = new Rectangle(0, 0, texture.Width / numFrames, texture.Height);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, rectangle, Color.White);
        }

        public void Update(double dt)
        {
            delay -= dt;
            if (delay < 0.0)
            {
                delay += wait;
                frame++;
                if (frame >= numFrames)
                {
                    numFrames = nextNumFrames;
                    if (nextAnimationFrames.Equals(animationFrames))
                        resetAnimationList();
                    else
                        animationFrames = nextAnimationFrames;
                    texture = nextTexture;
                    frame = 0;
                }
                realFrame = animationFrames[frame];
                rectangle = new Rectangle(texture.Width * realFrame / numFrames, 0, texture.Width / numFrames, texture.Height);
                
            }
        }
    }

}