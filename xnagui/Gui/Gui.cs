using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ruminate.DataStructures;

namespace Ruminate.GUI.Framework {

    public class Gui {

        private Root<Widget> Dom { get; set; }
        internal InputManager InputManager { get; private set; }
        internal RenderManager RenderManager { get; private set; }

        public Gui(Game game, Skin defaultSkin, TextRenderer defaultTextRenderer) {            
            InputSystem.Initialize(game.Window);                        
            InitDom();
            RenderManager = new RenderManager(game.GraphicsDevice);
            SetDefaultSettings(game, defaultSkin, defaultTextRenderer);
        }

        public void BindInput()
        {
            InputManager = new InputManager(Dom);
        }

        private void InitDom() {
            Dom = new Root<Widget>();
            Dom.OnAttachedToRoot += node => node.Data.Prepare(this);
            Dom.OnAttachedToRoot += node => {
                if (node.Parent != null && node.Parent.Root != node.Parent) {
                    node.Parent.Data.Layout();
                }
            };
            Dom.OnChildrenChanged += node => node.DfsOperation(innerNode => innerNode.Data.Layout());
        }

        public Widget[] Widgets {
            get {
                return Dom.Children.ConvertAll(input => input.Data).ToArray();
            } set {
                Dom.Children.Clear(); 
                AddWidgets(value);
            }
        }

        public void AddWidget(Widget widget) {
            Dom.AddChild(widget);
        }

        public void AddWidgets(IEnumerable<Widget> widget) {
            Dom.AddChildren(widget);
        }

        public void RemoveWidget(Widget widget) {
            Dom.RemoveChild(widget);
        }

        private void SetDefaultSettings(Game game, Skin defaultSkin, TextRenderer defaultTextRenderer) {

            DefaultScrollSpeed = 3;
            DefaultWheelSpeed = 3;

            SelectionColor = new Texture2D(game.GraphicsDevice, 1, 1);
            HighlightingColor = Color.LightSkyBlue * 0.3f;

            AddSkin("Default", defaultSkin);
            DefaultSkin = "Default";

            AddTextRenderer("Default", defaultTextRenderer);
            DefaultTextRenderer = "Default";
        }

        public Rectangle ScreenBounds { get { return RenderManager.GraphicsDevice.Viewport.Bounds; } }

        public int DefaultScrollSpeed { get; set; }
        public int DefaultWheelSpeed { get; set; }

        public Texture2D SelectionColor {
            get { return RenderManager.SelectionColor; }
            set { RenderManager.SelectionColor = value; }
        }

        public Color HighlightingColor {
            get { return RenderManager.HighlightingColor; }
            set { RenderManager.HighlightingColor = value; }
        }

        public string DefaultSkin {
            get { return RenderManager.DefaultSkin; }
            set { RenderManager.DefaultSkin = value; }
        }

        public string DefaultTextRenderer {
            get { return RenderManager.DefaultTextRenderer; }
            set { RenderManager.DefaultTextRenderer = value; }
        }

        public SpriteFont DefaultFont {
            get { return RenderManager.TextRenderers[DefaultTextRenderer].SpriteFont; }
        }

        public void AddSkin(string name, Skin skin) {
            RenderManager.AddSkin(name, skin);
        }

        public void AddTextRenderer(string name, TextRenderer renderer) {
            RenderManager.AddTextRenderer(name, renderer);
        }

        public bool HasMouse { get { return InputManager.HoverElement != null; } }

        public event CharEnteredHandler CharEntered {
            add { InputManager.CharEntered += value; }
            remove { InputManager.CharEntered -= value; }
        }

        public event KeyEventHandler KeyDown {
            add { InputManager.KeyDown += value; }
            remove { InputManager.KeyDown -= value; }
        }

        public event KeyEventHandler KeyUp {
            add { InputManager.KeyUp += value; }
            remove { InputManager.KeyUp -= value; }
        }

        public event MouseEventHandler MouseDoubleClick {
            add { InputManager.MouseDoubleClick += value; }
            remove { InputManager.MouseDoubleClick -= value; }
        }

        public event MouseEventHandler MouseDown {
            add { InputManager.MouseDown += value; }
            remove { InputManager.MouseDown -= value; }
        }

        public event MouseEventHandler MouseHover {
            add { InputManager.MouseHover += value; }
            remove { InputManager.MouseHover -= value; }
        }

        public event MouseEventHandler MouseUp {
            add { InputManager.MouseUp += value; }
            remove { InputManager.MouseUp -= value; }
        }

        public event MouseEventHandler MouseWheel {
            add { InputManager.MouseWheel += value; }
            remove { InputManager.MouseWheel -= value; }
        }

        public void Update() {

            Dom.DfsOperationChildren(node => {
                if (!node.Data.Active) return;
                node.Data.Update();
            });
        }

        public void Draw() {
          
            RenderManager.Draw(Dom);            
        }
    }
}
