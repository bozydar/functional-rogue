using Microsoft.Xna.Framework;
using Ruminate.GUI.Content;
using Ruminate.GUI.Framework;

namespace TestBed {

    public class Test : Screen {

        Gui _gui;

        public override void Init(Game1 game) {

            var skin = new Skin(game.TestImageMap, game.TestMap);
            var text = new TextRenderer(game.TestSpriteFont, Color.Black);
            _gui = new Gui(game, skin, text);

            _gui.AddWidget(
                new ScrollBars {
                    Children = new Widget[] {
                        new Panel(100, 100, 200, 200) { 
                            Children = new Widget[] {
                                new ScrollBars {
                                     Children = new Widget[] {
                                        new Button(10, 10, "TEST"),
                                        new Button(300, 10, "TEST"),
                                        new Button(10, 300, "TEST"),
                                        new Button(300, 300, "TEST")
                                }
                            }
                        }
                    }
                }
             });
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
