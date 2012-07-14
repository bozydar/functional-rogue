//
// Press tab to toggle between screens.
//

using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Ruminate.Utils;

namespace TestBed {

    public class Game1 : Game {

        GraphicsDeviceManager _graphics;
        Screen _currentScreen;
        Screen[] _currentScreens;
        KeyboardState _oldState;
        int _index;

        public SpriteFont DefaultSpriteFont;
        public Texture2D DefaultImageMap;
        public string DefaultMap;

        public SpriteFont TestSpriteFont;
        public Texture2D TestImageMap;
        public string TestMap;

        public SpriteFont GreySpriteFont;
        public Texture2D GreyImageMap;
        public string GreyMap;

        public Game1() {            

            _graphics = new GraphicsDeviceManager(this);            

            Content.RootDirectory = "GuiExampleContent";            
        }

        protected override void Initialize() {

            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 800;
            _graphics.ApplyChanges();

            _currentScreens = new Screen[] {
                new WidgetTest(),
                new InputTest(),
                new NestedTest(),                
                new LayoutNestedTest(),
                new Test()
            };            

            IsMouseVisible = true;
            base.Initialize();
        }

        protected override void LoadContent() {

            DefaultImageMap = Content.Load<Texture2D>(@"DefaultSkin\ImageMap");
            DefaultMap = File.OpenText(@"GuiExampleContent\DefaultSkin\Map.txt").ReadToEnd();
            DefaultSpriteFont = Content.Load<SpriteFont>(@"DefaultSkin\Font");            

            TestImageMap = Content.Load<Texture2D>(@"TestSkin\ImageMap");
            TestMap = File.OpenText(@"GuiExampleContent\TestSkin\Map.txt").ReadToEnd();
            TestSpriteFont = Content.Load<SpriteFont>(@"DefaultSkin\Font");

            GreyImageMap = Content.Load<Texture2D>(@"GreySkin\ImageMap");
            GreyMap = File.OpenText(@"GuiExampleContent\GreySkin\Map.txt").ReadToEnd();
            GreySpriteFont = Content.Load<SpriteFont>(@"DefaultSkin\Font");

            DebugUtils.Init(_graphics.GraphicsDevice, TestSpriteFont);

            _index = 0;
            _currentScreen = _currentScreens[_index];
            _currentScreen.Init(this);
        }

        protected override void Update(GameTime gameTime) {

            var newState = Keyboard.GetState();

            if (_oldState.IsKeyUp(Keys.Tab) && newState.IsKeyDown(Keys.Tab)) {
                if (_index + 1 == _currentScreens.Length) {
                    _index = 0;
                } else {
                    _index++;
                }

                _currentScreen = _currentScreens[_index];
                _currentScreen.Init(this);
            }

            _currentScreen.Update();

            _oldState = newState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {

            GraphicsDevice.Clear(_currentScreen.Color);

            _currentScreen.Draw();

            base.Draw(gameTime);
        }
    }
}
