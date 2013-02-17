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

        public int width;
        public int height;

        public int numFrames;
        public int frame = 0;
        public double wait;
        public double delay;

        int realFrame;

        public Rectangle rectangle;

        public Dictionary<string, List<int>> animationSets = new Dictionary<string,List<int>>();
        string _animationSet = "_default";
        public string nextAnimationSet = "_default";
        public List<int> animationSetValue;

        public AnimatedTexture(Texture2D texture, Vector2 position, int numFrames, double frameRate)
        {
            this.texture = texture;
            this.width = texture.Width / numFrames;
            this.height = texture.Height;
            this.position = position;
            this.numFrames = numFrames;
            this.delay = this.wait = frameRate;

            this.dimensions = new Vector2(texture.Width / numFrames, texture.Height);

            CreateDefaultAnimationSet();
            animationSet = "_default";
        }

        // Animation Sets are lists that contain indices to represent animations in a spritesheet
        public string animationSet
        {

            get
            {
                return this._animationSet;
            }

            set
            {
                if (this._animationSet != value)
                {
                    this._animationSet = value;
                    frame = 0;
                    delay = 0.0f;
                    this.nextAnimationSet = value;
                    this.animationSetValue = this.animationSets[value];
                }

            }

        }

        // Populates the animation lits with a full set of frames
        public void CreateDefaultAnimationSet()
        {
            List<int> defaultList = new List<int>();
            int i = 0;
            for (i=0; i<numFrames; i++)
                defaultList.Add(i);
            animationSets.Add("_default", defaultList);
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
                if (frame == animationSetValue.Count - 1)
                {
                    this.animationSet = this.nextAnimationSet;
                    frame = 0;
                }
                realFrame = animationSetValue[frame];
                rectangle = new Rectangle(texture.Width * realFrame / numFrames, 0, texture.Width / numFrames, texture.Height);

                frame++;
            }
        }
    }

}