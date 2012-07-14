using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Ruminate.Engine2D {

    /// <summary>
    /// 
    /// </summary>
    public class NPC {

        public Body PhysicsBody { get; set; }

        private JumpData Jumpdata { get; set; }

        public static Texture2D tex;

        public NPC(World world, int width, int height) {

            Jumpdata =  new JumpData();

            PhysicsBody = BodyFactory.CreateEdge(
                world,
                new Vector2(10, 10) + new Vector2(
                    UnitConverter.PixelsToMeter(width / 2), 
                    0),
                new Vector2(10, 10) + new Vector2
                    (UnitConverter.PixelsToMeter(width / 2), 
                    UnitConverter.PixelsToMeter(height)));

            /*PhysicsBody = BodyFactory.CreateCircle(
                world, 
                UnitConverter.PixelsToMeter(width) / 2f, 
                1f, 
                new Vector2(10, 10));*/
            /*PhysicsBody = BodyFactory.CreateRectangle(
                world,
                UnitConverter.PixelsToMeter(width),
                UnitConverter.PixelsToMeter(height),
                1f,
                new Vector2(10, 10));*/

            PhysicsBody.BodyType = BodyType.Dynamic;
            PhysicsBody.Restitution = 0.0f;
            PhysicsBody.Friction = 0.05f;
            PhysicsBody.Inertia = float.MaxValue;
            PhysicsBody.Mass = 81.64f;
        }

        public void Update(KeyboardState oldState, KeyboardState newState) {

            var movementForce = Vector2.Zero;

            if (newState.IsKeyDown(Keys.A)) {
                movementForce.X = -15000;
            }

            if (newState.IsKeyDown(Keys.D)) {
                movementForce.X = 15000;
            }

            if (movementForce == Vector2.Zero) {

            PhysicsBody.LinearVelocity = new Vector2(
                MathHelper.Lerp(PhysicsBody.LinearVelocity.X, 0, 0.05f),
                PhysicsBody.LinearVelocity.Y);
            }

            PhysicsBody.ApplyForce(movementForce);

            if (!oldState.IsKeyDown(Keys.Space) && newState.IsKeyDown(Keys.Space)) {
                PhysicsBody.LinearVelocity = new Vector2(PhysicsBody.LinearVelocity.X, -10.0f * 2);
            }

            PhysicsBody.LinearVelocity = new Vector2(
                MathHelper.Clamp(PhysicsBody.LinearVelocity.X, -12f, 12f),
                MathHelper.Clamp(PhysicsBody.LinearVelocity.Y, -52f, 52f));
        }

        public void Draw(SpriteBatch spriteBatch) {

            //var center = new Vector2(2 * tex.Width, 2 * tex.Height);
            spriteBatch.Draw(
                tex,
                UnitConverter.MeterToPixels(new Vector2(10, 10) + PhysicsBody.Position),
                null,
                Color.White,
                PhysicsBody.Rotation,
                Vector2.Zero,
                1f,
                SpriteEffects.None, 0f);
        }

        private struct JumpData {

            public bool IsAirborne { get; set; }
            public float StartVelocity { get; set; }
        }
    }
}
