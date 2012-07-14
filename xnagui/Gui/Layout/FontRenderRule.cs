using Microsoft.Xna.Framework.Graphics;

namespace Ruminate.GUI.Framework {

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
