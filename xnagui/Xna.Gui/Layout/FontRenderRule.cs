using Microsoft.Xna.Framework.Graphics;
using Xna.Gui.Rendering;

namespace Xna.Gui.Layout {

    public abstract class FontRenderRule : RenderRule {

        protected TextRenderer TextRenderer { get; set; }
        public SpriteFont Font { get { return TextRenderer.SpriteFont; } }

        public override void Load() {
            if (Skin == null) { Skin = DefaultSkin; }
            TextRenderer = RenderManager.TextRenderers[RenderManager.DefaultTextRenderer];
            LoadRenderers();
            Loaded = true;
        } 
    }
}
