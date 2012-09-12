using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ruminate.GUI.Content;
using Xna.Gui.Controls.RenderRules;

namespace Xna.Gui.Controls.Elements {

    public sealed class Image : WidgetBase<LabelRenderRule> {

        public Texture2D Icon {
            get { return RenderRule.Image; }
            set { RenderRule.Image = value; }
        }

        public Image(int x, int y, Texture2D texture) {

            Icon = texture;
            Area = new Rectangle(x, y, texture.Width, texture.Height);
            RenderRule = new LabelRenderRule();
        }        

        public Image(int x, int y, int width, int height, Texture2D texture) {

            Icon = texture;
            Area = new Rectangle(x, y, width, height);
            RenderRule = new LabelRenderRule();
        }        

        protected override void Attach() { }

        public override void Layout() { }      

        protected override void Update() { }       
    }
}