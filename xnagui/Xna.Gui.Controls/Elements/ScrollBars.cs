using Microsoft.Xna.Framework;
using Ruminate.GUI;
using Ruminate.GUI.Content;
using Xna.Gui.Controls.RenderRules;
using Xna.Gui.Utils;

namespace Xna.Gui.Controls.Elements {

    public class ScrollBars : WidgetBase<ScrollBarsRenderRule> {

        float _yF, _xF;

        public int ScrollSpeed { get; set; }
        public int WheelSpeed { get; set; }

        private int WheelDelta { get; set; }

        private enum DragState { None, VBar, HBar, Up, Down, Left, Right, Wheel }
        
        private DragState State { get; set; }
        private Pin Pin { get; set; }

        protected override ScrollBarsRenderRule BuildRenderRule() {
            return new ScrollBarsRenderRule();
        }

        public ScrollBars() {

            Pin = new Pin();
            State = DragState.None;
        }

        protected override void Attach() {

            if (Parent != null) {
                var a = Parent.AbsoluteInputArea;
                Area = new Rectangle(0, 0, a.Width, a.Height);
            } else {
                Area = Owner.ScreenBounds;
            }

            ScrollSpeed = Owner.DefaultScrollSpeed;
            WheelSpeed = Owner.DefaultWheelSpeed;
        }

        public override void Layout() {            

            // Pixel size of panel to contain child elements
            var outerArea = BuildContainerRenderArea();
            RenderRule.BuildBars(outerArea);

            foreach (var widget in Children) {                
                widget.AbsoluteArea = new Rectangle(
                    widget.Area.X + AbsoluteInputArea.X - (int)(RenderRule.Horizontal.ChildOffset),
                    widget.Area.Y + AbsoluteInputArea.Y - (int)(RenderRule.Vertical.ChildOffset),
                    widget.Area.Width,
                    widget.Area.Height);
                widget.SissorArea = AbsoluteInputArea;
                if (Parent != null) {
                    widget.SissorArea = Rectangle.Intersect(widget.SissorArea, SissorArea);
                }
            }
        }        

        protected override void Update() {
            switch (State) {
                case DragState.None:
                    return;
                case DragState.VBar:
                    RenderRule.Vertical.BarOffset = _yF - Pin.Shift.Y;
                    break;
                case DragState.HBar:
                    RenderRule.Horizontal.BarOffset = _xF - Pin.Shift.X;
                    break;
                case DragState.Up:
                    RenderRule.Vertical.BarOffset += ScrollSpeed;
                    break;
                case DragState.Down:
                    RenderRule.Vertical.BarOffset -= ScrollSpeed;
                    break;
                case DragState.Right:
                    RenderRule.Horizontal.BarOffset += ScrollSpeed;
                    break;
                case DragState.Left:
                    RenderRule.Horizontal.BarOffset -= ScrollSpeed;
                    break;
                case DragState.Wheel:
                    RenderRule.Vertical.BarOffset -= (WheelDelta * WheelSpeed);
                    RenderRule.Vertical.BarOffset = MathHelper.Clamp(RenderRule.Vertical.BarOffset, 0, RenderRule.Vertical.MaxBarOffset);
                    break;
            }

            if (State != DragState.None) {
                GetTreeNode().DfsOperation(node=>node.Data.Layout());
            }

            if (State == DragState.Wheel) {
                State = DragState.None;
            }
        }

        private Rectangle BuildContainerRenderArea() {

            if (Children == null || Children.Length == 0) { return Rectangle.Empty; }

            var left = int.MaxValue;
            var right = int.MinValue;
            var top = int.MaxValue;
            var bottom = int.MinValue;

            foreach (var child in Children) {

                var childLeft = child.Area.X;
                var childRight = child.Area.X + child.Area.Width;
                var childTop = child.Area.Y;
                var childBottom = child.Area.Y + child.Area.Height;

                left = (left < childLeft) ? left : childLeft;
                right = (right > childRight) ? right : childRight;
                top = (top < childTop) ? top : childTop;
                bottom = (bottom > childBottom) ? bottom : childBottom;
            }

            return new Rectangle(0, 0, right + left, bottom + top);
        }

        protected override void MouseWheel(MouseEventArgs e) {

            WheelDelta = e.Delta;
            State = DragState.Wheel;
        }

        protected override void MouseDoubleClick(MouseEventArgs e) {
            MouseDown(e);
        }

        protected override void MouseDown(MouseEventArgs e) {

            var mouse = e.Location;

            if (RenderRule.BarDirection == Direction.Vertical || RenderRule.BarDirection == Direction.Both) {
                if (RenderRule.Vertical.IncreaseArea.Contains(mouse)) {
                    State = DragState.Up;
                } else if (RenderRule.Vertical.DecreaseArea.Contains(mouse)) {
                    State = DragState.Down;
                } else if (RenderRule.Vertical.BarArea.Contains(mouse)) {
                    _yF = RenderRule.Vertical.BarOffset;
                    Pin.Push();
                    State = DragState.VBar;
                } 
            }
            if (RenderRule.BarDirection == Direction.Horizontal || RenderRule.BarDirection == Direction.Both) {
                if (RenderRule.Horizontal.IncreaseArea.Contains(mouse)) {
                    State = DragState.Right;
                } else if (RenderRule.Horizontal.DecreaseArea.Contains(mouse)) {
                    State = DragState.Left;
                } else if (RenderRule.Horizontal.BarArea.Contains(mouse)) {
                    _xF = RenderRule.Horizontal.BarOffset;
                    Pin.Push();
                    State = DragState.HBar;
                }
            }
        }

        protected override void MouseUp(MouseEventArgs e) {
            Pin.Pull();
            State = DragState.None;
        }
    }
}