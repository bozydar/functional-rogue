using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ruminate.GUI.Content;
using Xna.Gui.Controls.RenderRules;

namespace Xna.Gui.Controls.Elements {

    public sealed class Label : WidgetBase<LabelRenderRule> {

        public int Padding { get; private set; }

        public string Value {
            get {
                return RenderRule.Label;
            } set {                
                RenderRule.Label = value;
                if (RenderRule.Loaded) {
                    Attach();
                }
            }
        }        

        public Texture2D Icon {
            get {
                return RenderRule.Image;
            } set {
                RenderRule.Image = value;
                if (RenderRule.Loaded) {
                    Attach();
                }
            }
        }

        public Label(int x, int y, string value = "") {

            Area = new Rectangle(x, y, 0, 0);
            RenderRule = new LabelRenderRule();
            Value = value;
        }

        public Label(int x, int y, Texture2D icon, string value = "", int padding = 0) {

            Area = new Rectangle(x, y, 0, 0);

            Icon = icon;
            Value = value;
            Padding = padding;
            RenderRule = new LabelRenderRule();
        }        

        private void Resize() {
            var size = RenderRule.Font.MeasureString(RenderRule.Label);
            if (RenderRule.Image != null) {
                Area = new Rectangle(Area.X, Area.Y,
                    (int)size.X + Padding + RenderRule.Image.Width,
                    (size.Y > RenderRule.Image.Height) ? (int)size.Y : RenderRule.Image.Height);
            } else {
                Area = new Rectangle(Area.X, Area.Y, (int)size.X, (int)size.Y);
            }

            RenderRule.SetSize(Area.Width, Area.Height);
        }

        protected override void Attach() { Resize(); }

        public override void Layout() { }

        protected override void Update() { }
    }
}