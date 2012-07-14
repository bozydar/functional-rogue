using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ruminate.Utils;

namespace Ruminate.GUI.Framework {

    public class TextRenderer {

        public Font Font { get; set; }
        public SpriteFont SpriteFont { get { return Font.SpriteFont; } }

        public Color Color { get; set; }

        public TextRenderer(SpriteFont font, Color color) {

            Font = new Font(font);
            Color = color;
        }

        public void Render(SpriteBatch spriteBatch, string value, Rectangle renderArea,
            TextHorizontal h = TextHorizontal.LeftAligned, TextVertical v = TextVertical.TopAligned) {

            var location = new Vector2(renderArea.X, renderArea.Y);

            var size = Font.MeasureString(value);

            switch (h) {
                case TextHorizontal.CenterAligned:
                    location.X += (renderArea.Width - size.X) / 1.9f;
                    break;
                case TextHorizontal.RightAligned:
                    location.X += renderArea.Width - size.X;
                    break;
            }

            switch (v) {
                case TextVertical.CenterAligned:
                    location.Y += (renderArea.Height - size.Y) / 1.9f;
                    break;
                case TextVertical.BottomAligned:
                    location.Y += renderArea.Height - size.Y;
                    break;
            }

            spriteBatch.DrawString(SpriteFont, value, location, Color);
        }
    }
}
