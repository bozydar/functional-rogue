﻿using Microsoft.Xna.Framework;
using Ruminate.GUI.Content;
using Xna.Gui.Controls.Elements.BoardItems;
using Xna.Gui.Controls.RenderRules;

namespace Xna.Gui.Controls.Elements
{
    public class Board : WidgetBase<BoardRenderRule> 
    {
    
        public int TileCountX { get; private set; }
        public int TileCountY { get; private set; }

        public Board(int tileCountX, int tileCountY) : base()
        {
            TileCountX = tileCountX;
            TileCountY = tileCountY;
            RenderRule = new BoardRenderRule(TileCountX, TileCountY);
        }

        public void PutTile(int x, int y, Tile tile)
        {
            RenderRule.Tiles[x, y] = tile;
        }

        public override void Layout()
        {
            foreach (var widget in Children)
            {
                widget.AbsoluteArea = new Rectangle(
                    widget.Area.X + AbsoluteInputArea.X,
                    widget.Area.Y + AbsoluteInputArea.Y,
                    widget.Area.Width,
                    widget.Area.Height);
                if (Parent != null)
                {
                    widget.SissorArea = Rectangle.Intersect(widget.AbsoluteArea, SissorArea);
                }
            }
        }

        protected override void Update()
        {
            // pass
        }

        protected override void Attach()
        {
            // pass
        }
    }
}