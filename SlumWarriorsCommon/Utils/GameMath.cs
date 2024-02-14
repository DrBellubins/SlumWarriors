using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SlumWarriorsCommon.Utils
{
    public class GameMath
    {
        private static uint xorRND;

        private const double xorMaxRatio = 1.0 / uint.MaxValue;

        public static void InitXorRNG()
        {
            xorRND = (uint)System.DateTime.UtcNow.Ticks;
        }

        public static float GetXorFloat(float n = 1f)
        {
            xorRND ^= xorRND << 21;
            xorRND ^= xorRND >> 35;
            xorRND ^= xorRND << 4;
            return (float)(xorRND * xorMaxRatio * n);
        }

        public static float GetXorFloat(float min, float max)
        {
            xorRND ^= xorRND << 21;
            xorRND ^= xorRND >> 35;
            xorRND ^= xorRND << 4;
            return min + (float)(xorRND * xorMaxRatio * (max - min));
        }

        public static bool InRange(float val, float min, float max)
        {
            return val <= max && val >= min;
        }

        public static bool InRange(int val, int min, int max)
        {
            return val <= max && val >= min;
        }

        public static Vector2 NearestChunkCoord(Vector2 input)
        {
            int x = (int)MathF.Floor(input.X);
            int y = (int)MathF.Floor(input.Y);

            int xRem = x % 16;
            int yRem = y % 16;

            return new Vector2(x - xRem, y - yRem);
        }

        public static Vector2 NearestBlockCoord(Vector2 input)
        {
            return new Vector2(MathF.Round(input.X), MathF.Round(input.Y));
        }
    }
}
