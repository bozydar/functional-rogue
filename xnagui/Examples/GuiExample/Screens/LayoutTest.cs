using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ruminate.GUI.Content;
using Ruminate.GUI.Framework;

namespace TestBed {

    public class NestedTest : Screen {

        Gui _gui;
        Texture2D _beaker;

        public override void Init(Game1 game) {

            Color = Color.White;

            _beaker = game.Content.Load<Texture2D>("beaker");

            var skin = new Skin(game.TestImageMap, game.TestMap);
            var text = new TextRenderer(game.TestSpriteFont, Color.Black);
            _gui = new Gui(game, skin, text) {
                Widgets = new Widget[] {
                    new ScrollBars {
                        Children = new Widget[] {
                            new Panel(10, 10, 1000, 1000) {
                                Children = new Widget[] {
                                    new ScrollBars {
                                        Children = new Widget[] {
                                            new Button(10, 10, "Test1"),
                                            new Button(10, 40, "Test2"),
                                            new Button(10, 70, "Test3"),
                                            new Button(10, 100, "Test4"),
                                            new Button(10, 130, "Test5"),
                                            new Button(10, 160, "Test6"),
                                            new Button(10, 190, "Test7"),
                                            new Button(10, 220, "Test8"),
                                            new Button(10, 250, "Test9"),
                                            new Button(10, 280, "Test10"),
                                            new Button(10, 310, "Test11"),
                                            new Button(10, 340, "Test12"),
                                            new Button(10, 370, "Test13"),
                                            new Button(10, 400, "Test14"),
                                            new Button(10, 430, "Test15"),
                                            new Button(10, 460, "Test16"),
                                            new Panel(100, 10, 200, 200) {
                                                Children = new Widget[] {
                                                    new Button(10, 10, "Test1"),
                                                    new Button(10, 40, "Test2"),
                                                    new Button(10, 70, "Test3")      
                                                }
                                            },
                                            new Panel(100, 230, 400, 400) {
                                                Children = new Widget[] {
                                                    new ScrollBars {
                                                        Children = new Widget[] {
                                                            new Button(10, 10, "Test1"),
                                                            new Button(10, 40, "Test2"),
                                                            new Button(10, 70, "Test3"),
                                                            new Button(10, 100, "Test4"),
                                                            new Button(10, 130, "Test5"),
                                                            new Button(10, 160, "Test6"),
                                                            new Button(10, 190, "Test7"),
                                                            new Button(10, 220, "Test8"),
                                                            new Button(10, 250, "Test9"),
                                                            new Button(10, 280, "Test10"),
                                                            new Button(10, 310, "Test11"),
                                                            new Button(10, 340, "Test12"),
                                                            new Button(10, 370, "Test13"),
                                                            new Button(10, 400, "Test14"),
                                                            new Button(10, 430, "Test15"),
                                                            new Button(10, 460, "Test16"),
                                                            new Panel(100, 10, 600, 600) {
                                                                Children = new Widget[] { 
                                                                    new ScrollBars {
                                                                        Children = new Widget[] {
                                                                            new Button(10, 10, "Button"),
                                                                            new ToggleButton(10, 40, "Toggle Button"),
                                                                            new Panel(10, 70, 120, 120),
                                                                            new CheckBox(10, 195, "Check Box"),
                                                                            new RadioButton(10, 215, "GRP", "Radio 1"),
                                                                            new RadioButton(10, 235, "GRP", "Radio 2"),
                                                                            new RadioButton(10, 255, "GRP", "Radio 3"),
                                                                            new Label(10, 275, "Research"),
                                                                            new Label(10, 295, _beaker, "Research"),
                                                                            new Panel(140, 70, 220, 220) {
                                                                                Children = new Widget[] {
                                                                                    new TextBox(2, 600)
                                                                                }
                                                                            },
                                                                            new Panel(370, 70, 220, 220) {
                                                                                Children = new Widget[] {
                                                                                    new ScrollBars() {
                                                                                        Children = new Widget[] {
                                                                                            new CheckBox(10, 10, "Button"),
                                                                                            new CheckBox(210, 10, "Button"),
                                                                                            new CheckBox(10, 210, "Button"),
                                                                                            new CheckBox(210, 210, "Button"),
                                                                                            new Panel(10, 230, 300, 300) {
                                                                                                Children = new Widget[] {
                                                                                                    new TextBox(2, 300)
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }           
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
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
