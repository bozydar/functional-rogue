using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Ruminate.GUI.Content;
using View;
using Xna.Gui.Rendering;

namespace functional_rogue_xna
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game, IClient
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Screen _screen;
        public Texture2D DefaultImageMap { get; private set; }
        public string DefaultMap { get; private set; }
        public SpriteFont DefaultSpriteFont { get; private set; }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        public void Show(Screen screen)
        {
            _screen = screen;
            _screen.Init(this, new Skin(DefaultImageMap, DefaultMap), new TextRenderer(DefaultSpriteFont, Color.Black));
        }

        protected override void Initialize()
        {
            IsMouseVisible = true;
            base.Initialize();
            Instance.Connect(this);
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            DefaultImageMap = Content.Load<Texture2D>(@"DefaultSkin\ImageMap");
            DefaultMap = File.OpenText(Content.RootDirectory + @"\DefaultSkin\Map.txt").ReadToEnd();
            DefaultSpriteFont = Content.Load<SpriteFont>(@"DefaultSkin\Font");
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (_screen != null)
            {
                _screen.Update();
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (_screen != null)
            {
                _screen.Draw();
            }
            base.Draw(gameTime);
        }
    }
}
