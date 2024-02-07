using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SlumWarriorsCommon.Terrain
{
    public class Chunk
    {
        Vector2 Position;
        public Block[,] Blocks = new Block[16, 16];
    }
}
