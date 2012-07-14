using Microsoft.Xna.Framework;
using Ruminate.GUI.Content;
using Ruminate.GUI.Framework;

namespace TestBed {

    public class LayoutNestedTest : Screen {

        Game1 _game;
        Gui _gui;

        public override void Init(Game1 game) {

            _game = game;

            Color = Color.White;

            var skin = new Skin(_game.TestImageMap, _game.TestMap);
            var text = new TextRenderer(_game.TestSpriteFont, Color.Black);
            _gui = new Gui(game, skin, text) {
                Widgets = new Widget[] {
                    new ScrollBars {
                        Children = new Widget[] {
                            new Panel(10, 10, 300, 300) {
                                Children = new Widget[] {
                                    new ScrollBars {
                                        Children = new Widget[] {
                                            new Button(10, 10, "Test1"),
                                            new Button(10, 250, "Test2"),
                                            new Button(250, 10, "Test3"),
                                            new Button(250, 250, "Test4")
                                        }
                                    }
                                }
                            },
                            new Button(10, 1000, "Long")
                        }
                    }
                }                          
            };
        }

        public override void Unload() {
            _gui = null;
        }

        public override void Update() {
            _gui.Update();
        }

        public override void Draw() {
            _gui.Draw();
        }
    }
}
