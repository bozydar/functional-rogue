using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ruminate.Engine2D {
    
    /// <summary>
    /// 
    /// </summary>
    public class Level {

        //Psysics
        public World FarseerWorld { get; set; }
        public List<Body> FarseerBodys { get; set; }

        //Map Contents
        public List<NPC> Actors { get; set; }

        //What a Map Consists of
        private List<TiledMap> TiledMaps { get; set; }

        public Level() {
            FarseerWorld = new World(new Vector2(0, 9.81f * 8));
            FarseerBodys = new List<Body>();

            Actors = new List<NPC>();
            TiledMaps = new List<TiledMap>();
        }

        public void Update(GameTime gameTime) {

            FarseerWorld.Step(
                (float)gameTime.ElapsedGameTime.TotalMilliseconds
                * 0.001f);
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle worldArea) {

            foreach (var tiledMap in TiledMaps) {
                tiledMap.Draw(spriteBatch, worldArea);
            }
        }

        #region TiledSupport

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tiledMap"></param>
        public void AddTiledMap(TiledMap tiledMap) {

            TiledMaps.Add(tiledMap);

            var holder = tiledMap.GenerateFarseerBodies(
                    FarseerWorld,
                    UnitConverter.PixelsPerMeter);

            foreach (var body in holder) {

                body.BodyType = BodyType.Static;
                body.Mass = float.MaxValue;
                FarseerBodys.Add(body);
            }
        }

        #endregion
    }
}
