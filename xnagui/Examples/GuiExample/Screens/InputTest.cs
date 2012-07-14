using Microsoft.Xna.Framework;
using Ruminate.GUI;
using Ruminate.GUI.Content;
using Ruminate.GUI.Framework;

namespace TestBed {

    public class InputTest : Screen {

        Gui _gui;

        Label _hasMouse, _charEntered, _keyDown, _keyUp, _doubleClick, 
            _mouseDown, _mouseHover, _mouseUp, _mouseWheel;

        public override void Init(Game1 game) {

            Color = Color.White;

            var skin = new Skin(game.TestImageMap, game.TestMap);
            var text = new TextRenderer(game.TestSpriteFont, Color.Black);
            _gui = new Gui(game, skin, text);
            _gui.CharEntered += CharEntered;
            _gui.KeyDown += KeyDown;
            _gui.KeyUp += KeyUp;
            _gui.MouseDoubleClick += MouseDoubleClick;
            _gui.MouseDown += MouseDown;
            _gui.MouseHover += MouseHover;
            _gui.MouseUp += MouseUp;
            _gui.MouseWheel += MouseWheel;

            _gui.AddWidget(_hasMouse = new Label(100, 10 + 30 * 0, "Test1"));
            _gui.AddWidget(_charEntered = new Label(100, 10 + 30 * 1, "Test2"));
            _gui.AddWidget(_keyDown = new Label(100, 10 + 30 * 2, "Test3"));
            _gui.AddWidget(_keyUp = new Label(100, 10 + 30 * 3, "Test3"));
            _gui.AddWidget(_doubleClick = new Label(100, 10 + 30 * 4, "Test4"));
            _gui.AddWidget(_mouseDown = new Label(100, 10 + 30 * 5, "Test5"));
            _gui.AddWidget(_mouseHover = new Label(100, 10 + 30 * 6, "Test6"));
            _gui.AddWidget(_mouseUp = new Label(100, 10 + 30 * 7, "Test7"));
            _gui.AddWidget(_mouseWheel = new Label(100, 10 + 30 * 8, "Test7"));
        }

        private void CharEntered(object sender, CharacterEventArgs args) {
            if (_gui.DefaultFont.Characters.Contains(args.Character)) {
                _charEntered.Value = "Last char entered = " + new string(new[] { args.Character });
            }
        }

        private void KeyDown(object sender, KeyEventArgs args) {
            _keyDown.Value = "Key down = " + args.KeyCode;
        }

        private void KeyUp(object sender, KeyEventArgs args) {
            _keyUp.Value = "Key up = " + args.KeyCode;
        }

        private void MouseDoubleClick(object sender, MouseEventArgs args) {
            _doubleClick.Value = "DoubleClick @ " + args.X + "," + args.Y;
        }

        private void MouseDown(object sender, MouseEventArgs args) {
            _mouseDown.Value = "Down @ " + args.X + "," + args.Y;
        }

        private void MouseHover(object sender, MouseEventArgs args) {
            _mouseHover.Value = "Hover @ " + args.X + "," + args.Y;
        }

        private void MouseUp(object sender, MouseEventArgs args) {
            _mouseUp.Value = "Up @ " + args.X + "," + args.Y;
        }

        private void MouseWheel(object sender, MouseEventArgs args) {
            _mouseWheel.Value = "Wheel Delta = " + args.Delta;
        }

        public override void Unload() {
            _gui.CharEntered -= CharEntered;
            _gui = null;
        }

        public override void Update() {
            _gui.Update();
            Color = _gui.HasMouse ? Color.LightGreen : Color.Blue;
            _hasMouse.Value = "HasMouse = " + _gui.HasMouse;
        }

        public override void Draw() {
            _gui.Draw();
        }
    }
}
