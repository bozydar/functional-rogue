using Microsoft.Xna.Framework;
using Xna.Gui.Controls.Renderers;
using Xna.Gui.Layout;

namespace Xna.Gui.Controls.RenderRules {

    public class RadioButtonRenderRule : ElevatorRenderRule {
        public RadioButtonRenderRule() : base("radio_button") { }
    }

    public class CheckBoxRenderRule : ElevatorRenderRule {
        public CheckBoxRenderRule() : base("checkbox") { }
    }

    public abstract class ElevatorRenderRule : FontRenderRule {        

        private Rectangle _area, _icon;
        public override Rectangle Area {
            get {
                return _area;
            } set {
                _area = value;
                _icon.X = _area.X;
                _icon.Y = _area.Y;                
            }
        }

        public bool Hover { get; set; }
        public bool Checked { get; set; }

        public Point IconSize { get { return _default.Size; } }

        public string Label { get; set; }
        
        private IconRenderer _default, _hover, _checked, _hoverChecked;
        private readonly string _type;        

        protected ElevatorRenderRule(string type, string skin = null) {

            Skin = skin;
            _type = type;
            _icon = Rectangle.Empty;
        }

        public override void SetSize(int w, int h) {
            _area.Width = w; _area.Height = h;
        }

        protected override void LoadRenderers() {

            _default = LoadRenderer<IconRenderer>(Skin, _type);
            _icon.Width = _default.Width;
            _icon.Height = _default.Height;

            _hover = LoadRenderer<IconRenderer>(Skin, _type + "_hover");
            _checked = LoadRenderer<IconRenderer>(Skin, _type + "_checked");
            _hoverChecked = LoadRenderer<IconRenderer>(Skin, _type + "_hover_checked");            
        }

        public override void Draw() {

            if (Checked) {
                if (Hover) {
                    _hoverChecked.Render(RenderManager.SpriteBatch, _icon);
                } else {
                    _checked.Render(RenderManager.SpriteBatch, _icon);
                }
            } else {
                if (Hover) {
                    _hover.Render(RenderManager.SpriteBatch, _icon);
                } else {
                    _default.Render(RenderManager.SpriteBatch, _icon);
                }
            }

            TextRenderer.Render(
                RenderManager.SpriteBatch,
                Label,
                Area,
                TextHorizontal.RightAligned,
                TextVertical.CenterAligned);
        }
    }
}