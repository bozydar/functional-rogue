using System;
using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ruminate.Engine2D {

    /// <summary>
    /// A map imported from the Tiled tile map editor.
    /// </summary>
    public class TiledMap {

        /// <summary>
        /// The width in tiles of the map.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height in tiles of the map.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The width of an individual tile in the map.
        /// </summary>
        public int TileWidth { get; set; }

        /// <summary>
        /// The height of an individual tile in the map.
        /// </summary>
        public int TileHeight { get; set; }

        /// <summary>
        /// The tile arrays associated with the map.
        /// </summary>
        public List<Tile[]> TileLayers { get; set; }

        /// <summary>
        /// Polygons associated with the map.
        /// </summary>
        public List<Vector2[]> CollisionObjects { get; set; }

        /// <summary>
        /// Holder to prevent from recreating the rectangle constantly.
        /// </summary>
        private Rectangle _holder;

        #region Collisions

        /// <summary>
        /// Creates Farseer bodies from any polygons included in the imported TileMap.
        /// </summary>
        /// <param name="farseerWold">The world to attach the bodies to.</param>
        /// <param name="pixelsPerMeter">The world to attach the bodies to.</param>
        /// <returns></returns>
        public List<Body> GenerateFarseerBodies(World farseerWold, float pixelsPerMeter) {

            var bodies = new List<Body>();
                       
            foreach (var vertList in CollisionObjects) {

                var holder = new Vertices(vertList);
                var l = new Vector2(1 / pixelsPerMeter);
                holder.Scale(ref l);
                var verts = BayazitDecomposer.ConvexPartition(holder);                

                bodies.Add(BodyFactory.CreateCompoundPolygon(farseerWold, verts, 1f));
            }

            return bodies;
        }

        #endregion

        #region Rendering

        /// <summary>
        /// Renders all layers of the map.
        /// </summary>
        /// <param name="spriteBatch">The spriteBatch to render the map with.</param>
        public void Draw(SpriteBatch spriteBatch) {

            Draw(spriteBatch, new Rectangle(0, 0, Width * TileWidth, Height * TileHeight));
        }

        /// <summary>
        /// Renders all layers of the map.
        /// </summary>
        /// <param name="spriteBatch">The spriteBatch to render the map with.</param>
        /// <param name="worldArea">The are of the map to render.</param>
        public void Draw(SpriteBatch spriteBatch, Rectangle worldArea) {

            //Init the holder
            _holder = new Rectangle(0, 0, TileWidth, TileHeight);

            //Figure out the min and max tile indices to draw
            var minX = Math.Max((int)Math.Floor((float)worldArea.Left / TileWidth), 0);
            var maxX = Math.Min((int)Math.Ceiling((float)worldArea.Right / TileWidth), Width);

            var minY = Math.Max((int)Math.Floor((float)worldArea.Top / TileHeight), 0);
            var maxY = Math.Min((int)Math.Ceiling((float)worldArea.Bottom / TileHeight), Height);

            foreach (var tileLayer in TileLayers) {
                for (var y = minY; y < maxY; y++) {
                    for (var x = minX; x < maxX; x++) {
                        
                        _holder.X = x * TileWidth;
                        _holder.Y = y * TileHeight;

                        var t = tileLayer[y * Width + x];
                        spriteBatch.Draw(
                            t.Texture,
                            _holder, 
                            t.SourceRectangle,
                            Color.White, 
                            0,
                            Vector2.Zero,
                            t.SpriteEffects, 
                            0);
                    }
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// Information on how to render an individual tile.
    /// </summary>
    public class Tile {
        /// <summary>
        /// The image map associated with this tile. 
        /// </summary>
        public Texture2D Texture { get; set; }

        /// <summary>
        /// The location on the image map to draw from.
        /// </summary>
        public Rectangle SourceRectangle { get; set; }

        /// <summary>
        /// The sprite effects associated with this tile.
        /// </summary>
        public SpriteEffects SpriteEffects { get; set; }
    }
}
