using Microsoft.Xna.Framework;

namespace Ruminate.Engine2D {

    /// <summary>
    /// Converts pixels into meters and vice versa
    /// </summary>
    public static class UnitConverter {

        /// <summary>
        /// Number of pixels equivalent to a Farseer meter
        /// </summary>
        public static float PixelsPerMeter { get; set; }

        public static void Init(float pixelsPerMeter) {

            PixelsPerMeter = pixelsPerMeter;
        }

        public static float PixelsToMeter(int pixels) {

            return pixels / PixelsPerMeter;
        }

        public static Vector2 PixelsToMeter(Vector2 pixels) {

            return pixels / PixelsPerMeter;
        }

        public static int MeterToPixels(float meters) {

            return (int)(meters * PixelsPerMeter);
        }

        public static Vector2 MeterToPixels(Vector2 meters) {

            return meters * PixelsPerMeter;
        }
    }
}
