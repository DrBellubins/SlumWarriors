using Raylib_cs;
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
        Stone,
        Dirt,
        Sand,
        Water,
        Snow
    }

    public class Block
    {
        public static Dictionary<BlockType, Texture2D> Textures = new Dictionary<BlockType, Texture2D>();

        public int Layer; // 0 = ground, 1 = wall, 2 = ceiling
        public BlockType Type;
        public Vector2 Position;

        public static void InitializeBlockTextures()
        {
            var blockTypeCount = Enum.GetNames(typeof(BlockType)).Length;

            for (int i = 0; i < blockTypeCount; i++)
            {
                Textures.Add((BlockType)i, loadBlockTexture((BlockType)i));
            }
        }

        private static Texture2D loadBlockTexture(BlockType blockType)
        {
            return Raylib.LoadTexture($"Assets/Textures/Blocks/{blockType}.png");
        }
    }
}
