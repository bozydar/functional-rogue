using C3.XNA;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework.Graphics;
using Ruminate.Engine2D;
using Color = Microsoft.Xna.Framework.Color;

namespace Ruminate.Utils {    

    public static class SpriteBatchExtentions {

        public static void RenderWorld(this SpriteBatch spriteBatch, World world) {
            foreach (var body in world.BodyList) {
                foreach (var fixture in body.FixtureList) {
                    switch (fixture.Shape.ShapeType) {
                        case ShapeType.Polygon:
                            var polygonShape = (PolygonShape)fixture.Shape;
                            var previous = polygonShape.Vertices[0];
                            foreach (var vertex in polygonShape.Vertices) {
                                spriteBatch.DrawLine(
                                    UnitConverter.MeterToPixels(body.Position + previous),
                                    UnitConverter.MeterToPixels(body.Position + vertex),
                                    Color.Red);
                                previous = vertex;
                            }
                            spriteBatch.DrawLine(
                                UnitConverter.MeterToPixels(body.Position +
                                    polygonShape.Vertices[polygonShape.Vertices.Count - 1]),
                                UnitConverter.MeterToPixels(body.Position +
                                    polygonShape.Vertices[0]),
                                Color.Red);
                            break;
                        case ShapeType.Circle:
                            var circleShape = (CircleShape)fixture.Shape;
                            spriteBatch.DrawCircle(
                                UnitConverter.MeterToPixels(body.Position + circleShape.Position),
                                UnitConverter.MeterToPixels(circleShape.Radius),
                                15, Color.Blue);
                            break;
                        case ShapeType.Edge:
                            var edgeShape = (EdgeShape)fixture.Shape;
                            spriteBatch.DrawLine(
                                UnitConverter.MeterToPixels(body.Position + edgeShape.Vertex1),
                                UnitConverter.MeterToPixels(body.Position + edgeShape.Vertex2),
                                Color.DarkGreen);
                            break;
                    }
                }
            }
        }
    }
}
