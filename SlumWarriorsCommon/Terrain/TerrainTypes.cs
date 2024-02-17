using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using Raylib_cs;

namespace SlumWarriorsCommon.Terrain
{
    public enum BlockType
    {
        Grass,
        Stone,
        Dirt,
        Sand,
        Water,
        Snow
    }

    public enum TerrainBiome
    {
        Flatland,
        Plains,
        Ocean,
        Beach,
        Mountains,
        Forest,
        Snowlands
    }

    public class ChunkInfo
    {
        public bool Generated; // Runtime quality-of-life check
        public bool Modified;
        public Vector2 Position;

        public ChunkInfo()  {}

        public ChunkInfo(bool generated, bool modified, Vector2 position)
        {
            Generated = generated;
            Modified = modified;
            Position = position;
        }
    }

    public class Chunk
    {
        public ChunkInfo Info;
        public Block[,] Blocks = new Block[16, 16];

        public Chunk()
        {
            Info = new ChunkInfo();
        }
    }

    public class BlockInfo
    {
        public BlockType Type;
        public TerrainBiome Biome;
        public int Hardness; // 0 to 10 (10 being unbreakable)
        public float Thickness; // 0 to 1 (Used for slowling player down)
        public int MaxStack;

        public BlockInfo() {}

        public BlockInfo(BlockType type, int hardness, float thickness, int maxStack)
        {
            Type = type;
            Hardness = hardness;
            Thickness = thickness;
            MaxStack = maxStack;
        }
    }

    public class BlockSounds
    {
        public Sound[] Sounds = new Sound[4];

        /// <summary>
        /// Get random sound from array
        /// </summary>
        public Sound RND
        {
            get { return Sounds[new Random().Next(0, 3)]; }
        }
    }

    public class Block
    {
        public static Dictionary<BlockType, BlockInfo> Prefabs = new Dictionary<BlockType, BlockInfo>();
        public static Dictionary<BlockType, Texture2D> Textures = new Dictionary<BlockType, Texture2D>();
        public static Dictionary<BlockType, BlockSounds> Sounds = new Dictionary<BlockType, BlockSounds>();

        public static Dictionary<BlockType, Color> Colors = new Dictionary<BlockType, Color>();

        public int Layer; // TODO: Set to byte and send over network (for lighting)
        public Vector2 Position;
        public BlockInfo Info;
        public ChunkInfo ChunkInfo;
        public float LightLevel; // 0 to 1

        public Block()
        {
            Layer = 0;
            Position = Vector2.Zero;
            Info = new BlockInfo(BlockType.Grass, 2, 0.0f, 64);
            ChunkInfo = new ChunkInfo();
            LightLevel = 1.0f;
        }

        public Block(int layer, Vector2 position, BlockInfo info)
        {
            Layer = layer;
            Position = position;
            Info = info;
            ChunkInfo = new ChunkInfo();
            LightLevel = 1.0f;
        }

        public static void InitializeBlockPrefabs(bool isServer)
        {
            // MUST BE IN BLOCKTYPE ORDER!!!
            // BlockInfo(hardness, thickness)
            Prefabs.Add(BlockType.Grass, new BlockInfo(BlockType.Grass, 2, 0.0f, 64));
            Prefabs.Add(BlockType.Stone, new BlockInfo(BlockType.Stone, 4, 0.0f, 64));
            Prefabs.Add(BlockType.Dirt, new BlockInfo(BlockType.Dirt, 2, 0.0f, 64));
            Prefabs.Add(BlockType.Sand, new BlockInfo(BlockType.Sand, 1, 0.0f, 64));
            Prefabs.Add(BlockType.Water, new BlockInfo(BlockType.Water, 10, 0.5f, 64));
            Prefabs.Add(BlockType.Snow, new BlockInfo(BlockType.Snow, 1, 0.0f, 64));

            // Load all block textures for later access
            if (!isServer)
            {
                var blockTypeCount = Enum.GetNames(typeof(BlockType)).Length;

                for (int i = 0; i < blockTypeCount; i++)
                {
                    Textures.Add((BlockType)i, loadBlockTexture((BlockType)i));

                    var blockSounds = new BlockSounds();

                    for (int ii = 0; ii < 4; ii++)
                    {
                        Console.WriteLine(ii);
                        blockSounds.Sounds[ii] = loadBlockSounds((BlockType)i, ii);
                    }

                    Sounds.Add((BlockType)i, blockSounds);
                }
            }
            else
            {
                var blockTypeCount = Enum.GetNames(typeof(BlockType)).Length;

                for (int i = 0; i < blockTypeCount; i++)
                    Colors.Add((BlockType)i, loadBlockColor((BlockType)i));
            }
        }

        private static Texture2D loadBlockTexture(BlockType blockType)
        {
            return Raylib.LoadTexture($"Assets/Textures/Blocks/{blockType}.png");
        }

        private static Color loadBlockColor(BlockType blockType)
        {
            switch (blockType)
            {
                case BlockType.Grass:
                    return Color.Green;
                case BlockType.Stone:
                    return Color.Gray;
                case BlockType.Dirt:
                    return Color.Brown;
                case BlockType.Sand:
                    return Color.Beige;
                case BlockType.Water:
                    return Color.Blue;
                case BlockType.Snow:
                    return Color.White;
                default:
                    return Color.Pink;
            }
        }

        private static Sound loadBlockSounds(BlockType blockType, int index)
        {
            var i = index + 1;
            return Raylib.LoadSound($"Assets/Sounds/Blocks/{blockType}/{blockType}{i}.ogg");
        }
    }
}
