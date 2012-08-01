using Microsoft.Xna.Framework;

namespace Xna.Gui.Utils {

    public class Pin {

        public bool Pushed { get; private set; }

        private Vector2 _start, _offset;

        public Vector2 Shift { 
            get {
                var temp = InputSystem.MouseLocation;
                _offset.X = _start.X - temp.X;
                _offset.Y = _start.Y - temp.Y;
                return _offset;
            } 
        }

        public Pin() {
            _start = Vector2.Zero;
            _offset = Vector2.Zero;
        }

        public void Push() {            
            Pushed = true;
            _start = new Vector2(InputSystem.MouseLocation.X, InputSystem.MouseLocation.Y);
        }

        public void Pull() {
            Pushed = false;
        }
    }    
}
