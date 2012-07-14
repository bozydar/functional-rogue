using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ruminate.GUI.Content;
using Ruminate.GUI.Framework;

namespace TestBed {

    public class WidgetTest : Screen {

        Gui _gui;
        ScrollBars _container;
        Panel _visible;

        public override void Init(Game1 game) {

            Color = Color.White;

            var beaker = game.Content.Load<Texture2D>("beaker");

            var skin = new Skin(game.TestImageMap, game.TestMap);
            var text = new TextRenderer(game.TestSpriteFont, Color.Black);
            _gui = new Gui(game, skin, text) {
                Widgets = new Widget[] {
                    _container = new ScrollBars { 
                        Children = new Widget[] {
                            //By default the Button is as thin as the width of the label,
                            new TextBox(10, 10),
                            //new Button(10, 10 + (30 * 0), "Button"),
                            new Button(10, 10 + (30 * 1), "Wide Button", 2, delegate {throw new Exception("dupa");}),
                            new Button(10, 10 + (30 * 2), "T"),
                            new Button(10, 10 + (30 * 3), 120, "Width 120"),
                            //Button will resized if the specified width is smaller than the width of the text
                            new Button(10, 10 + (30 * 4), 20, "Width 20"), 
                            //The optional padding argument causes the button to be as wide as the label + (padding * 2)
                            new Button(10, 10 + (30 * 5), "5 Padding", 5),
                            new Button(10, 10 + (30 * 6), "10 Padding", 10),
                            new Button(10, 10 + (30 * 7), "Orange", 0, delegate { Color = Color.Orange; }),

                            //The ToggleButton behaves similarly to the Button but toggles between being 
                            //pressed or released each time its clicked.
                            new ToggleButton(150, 10 + (30 * 0), "Button"), 
                            new ToggleButton(150, 10 + (30 * 1), "Wide Button"),
                            new ToggleButton(150, 10 + (30 * 2), "T"),
                            new ToggleButton(150, 10 + (30 * 3), 120, "Width 120"),
                            new ToggleButton(150, 10 + (30 * 4), 20, "Width 20"), 
                            new ToggleButton(150, 10 + (30 * 5), "5 Padding", 5),
                            new ToggleButton(150, 10 + (30 * 6), "10 Padding", 10),
                            new ToggleButton(150, 10 + (30 * 7), "Hide ScrollBars") {
                                OnToggle = delegate { _visible.Visible = false; },
                                OffToggle = delegate { _visible.Visible = true; }
                            },
                            new ToggleButton(150, 10 + (30 * 8), "Deactivate ScrollBars") {
                                OnToggle = delegate { _visible.Active = false; },
                                OffToggle = delegate { _visible.Active = true; }
                            },

                            new CheckBox(300, 10, "ScrollBars") {
                                OnToggle = delegate { _visible.Visible = false; },
                                OffToggle = delegate { _visible.Visible = true; }
                            },

                            new RadioButton(300, 30, "RBG", "Red"){
                                OnCheck = delegate { Color = Color.Red; }
                            },
                            new RadioButton(300, 50, "RBG", "Blue"){
                                OnCheck = delegate { Color = Color.Blue; }
                            },
                            new RadioButton(300, 70, "RBG", "Green"){
                                OnCheck = delegate { Color = Color.Green; }
                            },

                            //For labels with icons the label is centered in the height of the icon
                            new Label(450, 10, "Research"),
                            new Label(450, 40, beaker, "Research"),
                            new Label(450, 70, beaker, "Research", 4), //Use the optional field for padding 

                            //Panels have a min size of twice the renderers border width.
                            new Panel(10, 300, 120, 120),
                            new Panel(10, 430, 60, 60),
                            new Panel(10, 500, 30, 30),
                            new Panel(10, 540, 15, 15),


                            //TextBoxs no longer have borders so nest them in panels if you need them.
                            new Panel(140, 300, 220, 220) { Children = new Widget[] { new TextBox(2, 800)  } },
                            new Panel(140, 530, 120, 120) { Children = new Widget[] { new TextBox(2, 300)  } },

                            //ScrollBars no longer have borders so nest them in panels if you need them.
                            //_visible = new Panel(370, 300, 220, 220) {
                            //    Children = new Widget[] {
                            //        new ScrollBars { 
                            //            Children = new Widget[] {
                            //                new CheckBox(10, 10, "Button"),
                            //                new CheckBox(210, 10, "Button"),
                            //                new CheckBox(10, 210, "Button"),
                            //                new CheckBox(210, 210, "Button")  
                            //            }
                            //        }
                            //    }
                            //}
                        }
                    }
                }
            };

            _gui.AddSkin("test", new Skin(game.GreyImageMap, game.GreyMap));
            _gui.DefaultSkin = "test";

            _gui.AddTextRenderer("test", new TextRenderer(game.GreySpriteFont, Color.Green));
            _gui.DefaultTextRenderer = "test";

            _container.AddWidgets(
                new Widget[] {
                    //By default the Button is as thin as the width of the label
                    new Button(10, 800 + 10 + (40 * 0), "Button"),
                    new Button(10, 800 + 10 + (40 * 1), "Wide Button"),
                    new Button(10, 800 + 10 + (40 * 2), "T"),
                    new Button(10, 800 + 10 + (40 * 3), 120, "Width 120"),
                    //Button will resized if the specified width is smaller than the width of the text
                    new Button(10, 800 + 10 + (40 * 4), 20, "Width 20"), 
                    //The optional padding argument causes the button to be as wide as the label + (padding * 2)
                    new Button(10, 800 + 10 + (40 * 5), "5 Padding", 5),
                    new Button(10, 800 + 10 + (40 * 6), "10 Padding", 10),

                    //The ToggleButton behaves similarly to the Button but toggles between being 
                    //pressed or released each time its clicked.
                    new ToggleButton(150, 800 + 10 + (40 * 0), "Button"), 
                    new ToggleButton(150, 800 + 10 + (40 * 1), "Wide Button"),
                    new ToggleButton(150, 800 + 10 + (40 * 2), "T"),
                    new ToggleButton(150, 800 + 10 + (40 * 3), 120, "Width 120"),
                    new ToggleButton(150, 800 + 10 + (40 * 4), 20, "Width 20"), 
                    new ToggleButton(150, 800 + 10 + (40 * 5), "5 Padding", 5),
                    new ToggleButton(150, 800 + 10 + (40 * 6), "10 Padding", 10),
                    
                    new CheckBox(300, 800 + 10, "Check Box"),

                    new RadioButton(300, 800 + 40, "GRP", "Group GRP"),
                    new RadioButton(300, 800 + 70, "GRP", "Group GRP"),
                    new RadioButton(300, 800 + 100, "GRP", "Group GRP"),

                    //For labels with icons the label is centered in the height of the icon
                    new Label(450, 800 + 10, "Research"),
                    new Label(450, 800 + 40, beaker, "Research"),
                    new Label(450, 800 + 70, beaker, "Research", 4), //Use the optional field for padding 

                    //Panels have a min size of twice the renderers border width.
                    new Panel(10, 800 + 300, 120, 120),
                    new Panel(10, 800 + 430, 60, 60),
                    new Panel(10, 800 + 500, 30, 30),
                    new Panel(10, 800 + 540, 15, 15),

                    //TextBoxs no longer have borders so nest them in panels if you need them.
                    new Panel(140, 800 + 300, 220, 220) { Children = new Widget[] { new TextBox(2, 800)  } },
                    new Panel(140, 800 + 530, 120, 120) { Children = new Widget[] { new TextBox(2, 300)  } },

                    //ScrollBars no longer have borders so nest them in panels if you need them.
                    new Panel(370, 800 + 300, 220, 220) {
                        Children = new Widget[] {
                            new ScrollBars { 
                                Children = new Widget[] {
                                    new CheckBox(10, 10, "Button"),
                                    new CheckBox(210, 10, "Button"),
                                    new CheckBox(10, 210, "Button"),
                                    new CheckBox(210, 210, "Button")  
                                }
                            }
                        }
                    }                    
                }
            );            
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
