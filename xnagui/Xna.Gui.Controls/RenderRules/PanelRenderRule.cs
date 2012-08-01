using Microsoft.Xna.Framework;
using Xna.Gui.Controls.Renderers;
using Xna.Gui.Layout;

namespace Xna.Gui.Controls.RenderRules {

    public class PanelRenderRule : RenderRule {

        private Rectangle _area;
        public override Rectangle Area {
            get { return _area; }
            set { _area = value; }
        }

        public int BorderWidth { get { return _default.BorderWidth; } }
        public override Rectangle SafeArea {
            get {
                return new Rectangle(
                    Area.X + BorderWidth,
                    Area.Y + BorderWidth,
                    Area.Width - (BorderWidth * 2),
                    Area.Height - (BorderWidth * 2));
            }
        }

        private BorderRenderer _default;

        public override void SetSize(int w, int h) {
            _area.Width = w;
            _area.Height = h;
        }

        public Rectangle BuildChildArea(Point size) {

            return _default.BuildChildArea(size);
        }

        protected override void LoadRenderers() {

            _default = LoadRenderer<BorderRenderer>(Skin, "panel");
        }

        public override void Draw() {

            _default.Render(RenderManager.SpriteBatch, Area);            
        }
    }
}