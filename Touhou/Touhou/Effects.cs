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
        public double delay;
        public double wait;

        int realFrame;

        public Rectangle rectangle;

        public Dictionary<string, List<int>> _animationSets = new Dictionary<string,List<int>>();
        string _animationSet = "_null";
        public string nextAnimationSet = "_default";
        public List<int> animationSetValue;

        public AnimatedTexture(Texture2D texture, Vector2 position, int numFrames, double frameRate)
        {
            this.texture = texture;
            this.width = texture.Width / numFrames;
            this.height = texture.Height;
            this.position = position;
            this.numFrames = numFrames;
            this.wait = this.delay = frameRate;

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
                    wait = 0.0f;
                    this.nextAnimationSet = value;
                    this.animationSetValue = this._animationSets[value];
                }

            }

        }

        public Dictionary<string, List<int>> animationSets
        {
            get
            {
                return this._animationSets;
            }
            set
            {
                this._animationSets = value;
                CreateDefaultAnimationSet();
                this.animationSetValue = this._animationSets[this._animationSet];
            }
        }

        // Populates the animation lits with a full set of frames
        public void CreateDefaultAnimationSet()
        {
            List<int> defaultList = new List<int>();
            int i = 0;
            for (i=0; i<numFrames; i++)
                defaultList.Add(i);
            _animationSets.Add("_default", defaultList);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, rectangle, Color.White);
        }

        public void Update(double dt)
        {
            wait -= dt;
            if (wait < 0.0)
            {
                wait += delay;
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