using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SlumWarriorsCommon.Terrain
{
    public enum BlockType
    {
        Air,
        Grass,
        Stone
    }

    public class Block
    {
        public int Layer; // 0 = ground, 1 = wall, 2 = ceiling
        public BlockType Type;
        public Vector2 Position;
    }
}
