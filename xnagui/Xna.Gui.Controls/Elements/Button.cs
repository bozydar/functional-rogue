using Microsoft.Xna.Framework;
using Ruminate.GUI;
using Ruminate.GUI.Content;
using Xna.Gui.Controls.RenderRules;
using Xna.Gui.Utils;

namespace Xna.Gui.Controls.Elements {

    public sealed class Button : WidgetBase<ButtonRenderRule> {

        /*####################################################################*/
        /*                               Variables                            */
        /*####################################################################*/

        /// <summary>
        /// Label displaced on the button.
        /// </summary>
        public string Label {
            get { return RenderRule.Label; }
            set { RenderRule.Label = value; }
        }

        /// <summary>
        /// The gap between the text and the edge of the button.
        /// </summary>
        private int? _textPadding;
        public int? TextPadding {
            get {
                return _textPadding;
            } set {
                _textPadding = value;
                _width = null;
            }
        }

        /// <summary>
        /// The width of the button.
        /// </summary>
        private int? _width;
        public int? Width {
            get {
                return _width;
            } set {
                _width = value;
                _textPadding = null;
            }
        }

        /// <summary>
        /// Event fired when the element is clicked.
        /// </summary>
        public event ElementEvent ClickEvent;

        /*####################################################################*/
        /*                           Initialization                           */
        /*####################################################################*/

        protected override ButtonRenderRule BuildRenderRule() {
            return new ButtonRenderRule();
        }

        /// <summary>
        /// Creates a new button at the location specified. The button defaults to
        /// the height of the RenderRule and width of the label.
        /// </summary>
        /// <param name="x">The X coordinate of the element.</param>
        /// <param name="y">The Y coordinate of the element.</param>
        /// <param name="label">The label to be rendered on the button.</param>
        /// <param name="padding">If specified the padding on either side of the label.</param>
        /// <param name="buttonEvent">Event fired when the button is clicked.</param>
        public Button(int x, int y, string label, int padding = 2, ElementEvent buttonEvent = null) {

            Area = new Rectangle(x, y, 0, 0);

            ClickEvent = buttonEvent;
            Label = label;

            TextPadding = padding;
        }

        /// <summary>
        /// Creates a new button at the location specified. The button defaults to
        /// the height of the RenderRule and width of the label.
        /// </summary>
        /// <param name="x">The X coordinate of the element.</param>
        /// <param name="y">The Y coordinate of the element.</param>
        /// <param name="width">The width of the Button. Ignored if the width is less that the width of the label.</param>
        /// <param name="label">The label to be rendered on the button.</param>
        /// <param name="buttonEvent">Event fired when the button is clicked.</param>
        public Button(int x, int y, int width, string label, ElementEvent buttonEvent = null) {

            Area = new Rectangle(x, y, 0, 0);
            Width = width;
            ClickEvent = buttonEvent;
            Label = label;
        }        

        protected override void Attach() {

            var minWidth = (int)RenderRule.Font.MeasureString(Label).X + (2 * RenderRule.Edge);

            if (TextPadding.HasValue) {
                Area = new Rectangle(Area.X, Area.Y, minWidth + (TextPadding.Value * 2), RenderRule.Height);
            } else if (Width.HasValue) {
                Area = new Rectangle(Area.X, Area.Y, (minWidth > Width.Value) ? minWidth : Width.Value, RenderRule.Height);
            }
        }

        public override void Layout() { }

        protected override void Update() { }

        /*####################################################################*/
        /*                                Events                              */
        /*####################################################################*/

        protected override void MouseClick(MouseEventArgs e) {
            if (ClickEvent != null) {
                ClickEvent(this);
            }
        }

        protected override void EnterPressed() {
            RenderRule.Mode = ButtonRenderRule.RenderMode.Pressed;
        }

        protected override void ExitPressed() {
            RenderRule.Mode = IsHover 
                ? ButtonRenderRule.RenderMode.Hover 
                : ButtonRenderRule.RenderMode.Default;
        }

        protected override void EnterHover() {
            if (RenderRule.Mode != ButtonRenderRule.RenderMode.Pressed) {
                RenderRule.Mode = ButtonRenderRule.RenderMode.Hover;
            }
        }

        protected override void ExitHover() {
            if (RenderRule.Mode != ButtonRenderRule.RenderMode.Pressed) {
                RenderRule.Mode = ButtonRenderRule.RenderMode.Default;
            }
        }
    }
}