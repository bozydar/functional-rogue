using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xna.Gui.Rendering {

    abstract public class Renderer {

        protected Texture2D ImageMap { get; set; }

        protected Renderer(Texture2D imageMap) { ImageMap = imageMap; }

        abstract public Rectangle BuildChildArea(Point size);
        abstract public void Render(SpriteBatch batch, Rectangle destination);
    }
}
