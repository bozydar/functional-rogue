using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ruminate.Utils {

    /// <summary>
    /// Load a texture from disk and convert to premulitplied alpha. 
    /// 
    /// Thanks to Jake Poznanski (http://jakepoz.com/jake_poznanski__speeding_up_xna.html)
    /// </summary>
    static public class PngLoader {        

        static public Texture2D LoadPng(string loc, GraphicsDevice graphicsDevice) {

            Texture2D file;
            RenderTarget2D result;

            using (var titleStream = TitleContainer.OpenStream("Content\\" + loc + ".png")) {
                file = Texture2D.FromStream(graphicsDevice, titleStream);
            }

            //Setup a render target to hold our final texture which will have premulitplied alpha values
            result = new RenderTarget2D(graphicsDevice, file.Width, file.Height);

            graphicsDevice.SetRenderTarget(result);
            graphicsDevice.Clear(Color.Black);

            //Multiply each color by the source alpha, and write in just the color values into the final texture
            var blendColor = new BlendState {
                ColorWriteChannels = ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue,
                AlphaDestinationBlend = Blend.Zero,
                ColorDestinationBlend = Blend.Zero,
                AlphaSourceBlend = Blend.SourceAlpha,
                ColorSourceBlend = Blend.SourceAlpha
            };

            var spriteBatch = new SpriteBatch(graphicsDevice);
            spriteBatch.Begin(SpriteSortMode.Immediate, blendColor);
            spriteBatch.Draw(file, file.Bounds, Color.White);
            spriteBatch.End();

            //Now copy over the alpha values from the PNG source texture to the final one, without multiplying them
            var blendAlpha = new BlendState {
                ColorWriteChannels = ColorWriteChannels.Alpha,
                AlphaDestinationBlend = Blend.Zero,
                ColorDestinationBlend = Blend.Zero,
                AlphaSourceBlend = Blend.One,
                ColorSourceBlend = Blend.One
            };

            spriteBatch.Begin(SpriteSortMode.Immediate, blendAlpha);
            spriteBatch.Draw(file, file.Bounds, Color.White);
            spriteBatch.End();

            //Release the GPU back to drawing to the screen
            graphicsDevice.SetRenderTarget(null);

            return result as Texture2D;
        }
    }
}
