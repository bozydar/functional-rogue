using Microsoft.Xna.Framework;
using Ruminate.GUI;
using Ruminate.GUI.Content;
using Xna.Gui.Controls.RenderRules;
using Xna.Gui.Utils;
using Xna.Utils.Utilities.Extentions;

namespace Xna.Gui.Controls.Elements {

    public sealed class CheckBox : WidgetBase<CheckBoxRenderRule> {

        public ElementEvent OnToggle { get; set; }
        public ElementEvent OffToggle { get; set; }

        private Point _location;
        public Point Location {
            get { return _location; } 
            set { _location = value; }
        }

        public string Label {
            get { return RenderRule.Label; }
            set { RenderRule.Label = value; }
        }

        private bool _innerIsToggled;
        public bool IsToggled {
            get { return _innerIsToggled; }
            set {
                if (value != _innerIsToggled) {
                    if (value) { if (OnToggle != null) { OnToggle(this); } } 
                    else { if (OffToggle != null) { OffToggle(this); } }
                }

                _innerIsToggled = value;
                RenderRule.Checked = _innerIsToggled;
            }
        }

        /// <summary>
        /// Sets whether or not the CheckBox is toggled without firing the 
        /// OnToggle or OffToggle events.
        /// </summary>
        /// <param name="toggle">Whether or not the CheckBox is checked.</param>
        public void SetToggle(bool toggle) {
            _innerIsToggled = toggle;
            RenderRule.Checked = _innerIsToggled;
        }

        public CheckBox(int x, int y, string label) {

            Area = new Rectangle(x, y, 0, 0);

            Label = label;
            _innerIsToggled = false;
            RenderRule = new CheckBoxRenderRule();
        }        

        protected override void Attach() {

            var size = RenderRule.Font.MeasureString(Label).ToPoint();
            var width = size.X + 2 + RenderRule.IconSize.X;
            var height = RenderRule.IconSize.Y;

            Area = new Rectangle(Area.X, Area.Y, width, height);
        }

        public override void Layout() { }

        protected override void Update() { }

        protected override void MouseClick(MouseEventArgs e) {
            IsToggled = !IsToggled;
        }

        protected override void EnterHover() {
            RenderRule.Hover = true;
        }

        protected override void ExitHover() {
            RenderRule.Hover = false;
        }
    }
}