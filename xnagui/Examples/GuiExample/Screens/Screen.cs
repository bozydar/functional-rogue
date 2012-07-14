using Microsoft.Xna.Framework;

namespace TestBed {

    public abstract class Screen {

        public Color Color { get; set; }

        public abstract void Init(Game1 game);
        public abstract void Unload();
        public abstract void Update();
        public abstract void Draw();
    }
}
