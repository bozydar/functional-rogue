using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xna.Gui.Rendering;

namespace Xna.Gui.Controls.Renderers {

    public class IconRenderer: Renderer {

        private readonly Rectangle _source;

        public Point Size { get { return new Point(_source.Width, _source.Height); } }        
        public int Width { get { return _source.Width; } }
        public int Height { get { return _source.Height; } }

        public IconRenderer(Texture2D imageMap, Rectangle source) : base(imageMap) {

            _source = source;
        }

        public override Rectangle BuildChildArea(Point size) {

            return Rectangle.Empty;
        }

        public override void Render(SpriteBatch batch, Rectangle destination) {

            batch.Draw(ImageMap, destination, _source, Color.White);
        }
    }
}
