using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xna.Gui.Controls.Elements.BoardItems;
using Xna.Gui.Layout;
using Xna.Gui.Rendering;

namespace Xna.Gui.Controls.RenderRules
{
    public class BoardRenderRule : RenderRule
    {
        private TextRenderer TextRenderer { get; set; }
        private SpriteFont Font { get { return TextRenderer.SpriteFont; } }
        public Tile[,] Tiles { get; private set; }

        public BoardRenderRule(int boardWidth, int boardHeight)
        {
            Tiles = new Tile[boardWidth, boardHeight];
        }

        private Rectangle _area;
        public override Rectangle Area
        {
            get { return _area; }
            set { _area = value; }
        }

        public override void SetSize(int w, int h)
        {
            _area.Width = w;
            _area.Height = h;
        }

        public override void Load()
        {
            if (Skin == null) { Skin = DefaultSkin; }
            TextRenderer = RenderManager.TextRenderers[RenderManager.DefaultTextRenderer];
            LoadRenderers();
            Loaded = true;
        }

        protected override void LoadRenderers()
        {

        }

        private const int With = 20;
        private const int Height = 20;
        public override void Draw()
        {
            for (var x = 0; x < Tiles.GetUpperBound(0); x++)
            {
                for (var y = 0; y < Tiles.GetUpperBound(1); y++)
                {
                    if (Tiles[x, y] != null)
                    {
                        foreach (var bitmapName in Tiles[x, y].BitmapNames)
                        {
                            var position = new Rectangle(_area.Left + x * With, _area.Top + y * Height, With, Height);
                            TextRenderer.Render(RenderManager.SpriteBatch, bitmapName, position,
                                                TextHorizontal.CenterAligned,
                                                TextVertical.CenterAligned);
                        }
                    }
                }
            }
        }
    }
}
