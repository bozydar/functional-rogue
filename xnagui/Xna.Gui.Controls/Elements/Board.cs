using Microsoft.Xna.Framework;
using Ruminate.GUI.Content;
using Xna.Gui.Controls.RenderRules;

namespace Xna.Gui.Controls.Elements
{
    public class Board : WidgetBase<PanelRenderRule> 
    {
        public override void Layout()
        {
            foreach (var widget in Children)
            {
                widget.AbsoluteArea = new Rectangle(
                    widget.Area.X + AbsoluteInputArea.X,
                    widget.Area.Y + AbsoluteInputArea.Y,
                    widget.Area.Width,
                    widget.Area.Height);
                if (Parent != null)
                {
                    widget.SissorArea = Rectangle.Intersect(widget.AbsoluteArea, SissorArea);
                }
            }
        }

        protected override void Update()
        {
            // pass
        }

        protected override PanelRenderRule BuildRenderRule()
        {
            return new PanelRenderRule();
        }

        protected override void Attach()
        {
            // pass
        }
    }
}