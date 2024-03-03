using SlumWarriorsCommon.Terrain;
using SlumWarriorsCommon.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SlumWarriorsServer.Terrain
{
    public class WorldGen
    {
        public static int SqrtRenderDistance;
        public static Chunk[,] RenderedChunks = new Chunk[64, 64];
        public static FastNoiseLite Noise = new FastNoiseLite();

        public static int Seed;

        public static void Start(int seed)
        {
            Seed = seed;
            SqrtRenderDistance = (int)MathF.Sqrt(RenderedChunks.Length);
            Noise.SetSeed(Seed);
        }

        // TODO: Make chunk generation multi-threaded (oh boi here we go)
        public static void GenerateAllChunks()
        {
            for (int cx = 0; cx < SqrtRenderDistance; cx++)
            {
                for (int cy = 0; cy < SqrtRenderDistance; cy++)
                {
                    // We start at nearest chunk pos to player, 
                    // multiply current chunk pos by num of
                    // blocks in chunk (32), then offset by
                    // sqrtRenderDistance multiplied by half
                    // num of blocks in chunk
                    var chunkPos = new Vector2((cx * 16) - (SqrtRenderDistance * 8),
                        (cy * 16) - (SqrtRenderDistance * 8));

                    RenderedChunks[cx, cy] = GenerateChunk(chunkPos);
                }
            }
        }

        public static Chunk GenerateChunk(Vector2 chunkPosition)
        {
            Chunk chunk = new Chunk();
            chunk.Info.Position = chunkPosition;

            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    var currentBlock = new Block();

                    Noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
                    Noise.SetFrequency(0.001f);
                    Noise.SetFractalType(FastNoiseLite.FractalType.FBm);
                    Noise.SetDomainWarpAmp(200.0f);
                    Noise.SetFractalLacunarity(3.0f);
                    Noise.SetFractalOctaves(5);

                    float biomeGen = Noise.GetNoise(chunkPosition.X + x, chunkPosition.Y + y);

                    if (biomeGen > 0.0f && biomeGen < 0.5f) // Plains
                    {
                        // Generate block based on noise here
                        Noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
                        Noise.SetFrequency(0.0035f);
                        Noise.SetFractalType(FastNoiseLite.FractalType.FBm);
                        Noise.SetDomainWarpAmp(200.0f);
                        Noise.SetFractalLacunarity(3.0f);
                        Noise.SetFractalOctaves(4);

                        float mountainGen = Noise.GetNoise(chunkPosition.X + x, chunkPosition.Y + y);

                        if (mountainGen < -0.5f || mountainGen > 0.5f) // Stone mountains
                        {
                            currentBlock = new Block(1, new Vector2(chunkPosition.X + (x - 0.5f),
                                chunkPosition.Y + (y - 0.5f)), Block.Prefabs[BlockType.Stone]);

                            currentBlock.Info.Biome = TerrainBiome.Mountains;
                        }
                        else // grass floor
                        {
                            currentBlock = new Block(0, new Vector2(chunkPosition.X + (x - 0.5f),
                                chunkPosition.Y + (y - 0.5f)), Block.Prefabs[BlockType.Grass]);
                            currentBlock.Info.Biome = TerrainBiome.Plains;
                        }
                    }
                    else if (biomeGen >= 0.5f) // Snowlands
                    {
                        // Generate block based on noise here
                        Noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
                        Noise.SetFrequency(0.0035f);
                        Noise.SetFractalType(FastNoiseLite.FractalType.FBm);
                        Noise.SetDomainWarpAmp(200.0f);
                        Noise.SetFractalLacunarity(3.0f);
                        Noise.SetFractalOctaves(4);

                        float mountainGen = Noise.GetNoise(chunkPosition.X + x, chunkPosition.Y + y);

                        if (mountainGen < -0.5f || mountainGen > 0.5f) // Stone mountains
                        {
                            currentBlock = new Block(1, new Vector2(chunkPosition.X + (x - 0.5f),
                                chunkPosition.Y + (y - 0.5f)), Block.Prefabs[BlockType.Stone]);

                            currentBlock.Info.Biome = TerrainBiome.Mountains;
                        }
                        else // Snow floor
                        {
                            currentBlock = new Block(0, new Vector2(chunkPosition.X + (x - 0.5f),
                                chunkPosition.Y + (y - 0.5f)), Block.Prefabs[BlockType.Snow]);

                            currentBlock.Info.Biome = TerrainBiome.Snowlands;
                        }
                    }
                    else if (biomeGen <= -0.2f) // Ocean
                    {
                        if (biomeGen <= -0.25f) // Water
                        {
                            currentBlock = new Block(0, new Vector2(chunkPosition.X + (x - 0.5f),
                                chunkPosition.Y + (y - 0.5f)), Block.Prefabs[BlockType.Water]);

                            currentBlock.Info.Biome = TerrainBiome.Ocean;

                        }
                        else // Beach
                        {
                            currentBlock = new Block(0, new Vector2(chunkPosition.X + (x - 0.5f),
                                chunkPosition.Y + (y - 0.5f)), Block.Prefabs[BlockType.Sand]);

                            currentBlock.Info.Biome = TerrainBiome.Beach;
                        }

                    }
                    else // Fallback
                    {
                        currentBlock = new Block(0, new Vector2(chunkPosition.X + (x - 0.5f),
                                chunkPosition.Y + (y - 0.5f)), Block.Prefabs[BlockType.Grass]);

                        currentBlock.Info.Biome = TerrainBiome.Flatland;
                    }

                    currentBlock.ChunkInfo = chunk.Info;

                    chunk.Blocks[x, y] = currentBlock;
                }
            }

            return chunk;
        }

        public static async Task<Vector2> GetSpawnPos()
        {
            var rndVec = Vector2.Zero;
            Block? blockCheck = null;

            while (blockCheck == null) // Potentially very slow
            {
                await Task.Delay(25);

                var rndX = GameMath.GetXorFloat(-(RenderedChunks.Length / 2), RenderedChunks.Length / 2);
                var rndY = GameMath.GetXorFloat(-(RenderedChunks.Length / 2), RenderedChunks.Length / 2);

                rndVec = GameMath.NearestBlockCoord(new Vector2(rndX, rndY));

                blockCheck = GetBlockAtPos(rndVec);

                // TODO: Could spawn players out of ounds
                if (blockCheck != null && blockCheck.Layer == 1)
                    blockCheck = null;
            }

            return rndVec;
        }

        public static Block? GetBlockAtPos(Vector2 pos)
        {
            for (int cx = 0; cx < SqrtRenderDistance; cx++)
            {
                for (int cy = 0; cy < SqrtRenderDistance; cy++)
                {
                    if (RenderedChunks[cx, cy] != null)
                    {
                        var currentChunk = RenderedChunks[cx, cy];

                        for (int x = 0; x < 16; x++)
                        {
                            for (int y = 0; y < 16; y++)
                            {
                                var curBlock = currentChunk.Blocks[x, y];

                                if ((curBlock.Position.X == (pos.X - 0.5f)) && (curBlock.Position.Y == (pos.Y - 0.5f)))
                                    return curBlock;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
