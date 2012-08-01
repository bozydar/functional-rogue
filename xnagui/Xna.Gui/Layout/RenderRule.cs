using Microsoft.Xna.Framework;
using Xna.Gui.Rendering;

namespace Xna.Gui.Layout {

    public abstract class RenderRule {

        public bool Loaded { get; set; }

        internal protected RenderManager RenderManager { get; set; }

        public abstract void SetSize(int w, int h);

        public abstract Rectangle Area { get; set; }
        public virtual Rectangle SafeArea { get { return Area; } }

        protected string DefaultSkin {
            get {
                return RenderManager.DefaultSkin;
            }
        }        

        private string _skin;
        protected string Skin {
            get {
                return _skin;
            } set {
                if (value == null) { return; }
                _skin = value;
                Load();
            }
        }

        protected RenderRule() {
            Loaded = false;
        }

        protected T LoadRenderer<T>(string skin, string widget) where T : Renderer {
            return (T)RenderManager.Skins[skin].WidgetMap[widget];
        }

        protected abstract void LoadRenderers();
        public virtual void Load() {
            if (Skin == null) { Skin = DefaultSkin; }
            LoadRenderers();
            Loaded = true;
        }        

        public abstract void Draw();
    }
}
