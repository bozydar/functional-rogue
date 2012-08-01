using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xna.Utils.Utilities.Components {

    public class FrameRateCounter {

        public Vector2 Location { get; set; }
        public SpriteBatch SpriteBatch { get; set; }
        public SpriteFont SpriteFont { get; set; }

        int _frameRate, _frameCounter;        
        TimeSpan _elapsedTime = TimeSpan.Zero;        

        public FrameRateCounter(GraphicsDevice device, SpriteFont font) {

            SpriteFont = font;
            SpriteBatch = new SpriteBatch(device);
        }

        public void Update(GameTime gameTime) {

            _elapsedTime += gameTime.ElapsedGameTime;

            if (_elapsedTime <= TimeSpan.FromSeconds(1)) { return; }

            _elapsedTime -= TimeSpan.FromSeconds(1);
            _frameRate = _frameCounter;
            _frameCounter = 0;
        }


        public void Draw() {

            _frameCounter++;

            var fps = string.Format("fps: {0}", _frameRate);

            SpriteBatch.Begin();

            SpriteBatch.DrawString(SpriteFont, fps, Location, Color.LightGray);
            SpriteBatch.DrawString(SpriteFont, fps, Location, Color.Black);

            SpriteBatch.End();
        }
    }
}
