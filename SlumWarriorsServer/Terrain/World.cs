using SlumWarriorsCommon.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using SlumWarriorsCommon.Networking;

namespace SlumWarriorsServer.Terrain
{
    public class World
    {
        public Block[,] CollisionCheck = new Block[4, 4];

        private FastNoiseLite noise = new FastNoiseLite();

        private Chunk testChunk = new Chunk();

        public void Start()
        {
            noise.SetSeed(999);
            testChunk = generateChunk(Vector2.Zero); // Offset chunks to make world coords whole numbers
        }

        bool sent = false;

        public void Update(float tickDelta)
        {
            // Collision check
            /*for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    CollisionCheck[x, y] = GetBlockAtPos(GetNearestBlockCoord(playerPos
                        + new Vector2(x - 1.5f, y - 1.5f)));
                }
            }*/

            foreach (var player in Engine.Players.Values)
            {
                if (player != null && player.Peer != null && !sent)
                {
                    Network.SendChunk(player.Peer, testChunk, "wu"); // world update
                    sent = true;
                }
            }
        }

        private Chunk generateChunk(Vector2 chunkPosition)
        {
            Chunk chunk = new Chunk();
            chunk.Info.Position = chunkPosition;

            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    var currentBlock = new Block();

                    noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
                    noise.SetFrequency(0.001f);
                    noise.SetFractalType(FastNoiseLite.FractalType.FBm);
                    noise.SetDomainWarpAmp(200.0f);
                    noise.SetFractalLacunarity(3.0f);
                    noise.SetFractalOctaves(5);

                    float biomeGen = noise.GetNoise(chunkPosition.X + x, chunkPosition.Y + y);

                    if (biomeGen > 0.0f && biomeGen < 0.5f) // Plains
                    {
                        // Generate block based on noise here
                        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
                        noise.SetFrequency(0.0035f);
                        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
                        noise.SetDomainWarpAmp(200.0f);
                        noise.SetFractalLacunarity(3.0f);
                        noise.SetFractalOctaves(4);

                        float mountainGen = noise.GetNoise(chunkPosition.X + x, chunkPosition.Y + y);

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
                        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
                        noise.SetFrequency(0.0035f);
                        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
                        noise.SetDomainWarpAmp(200.0f);
                        noise.SetFractalLacunarity(3.0f);
                        noise.SetFractalOctaves(4);

                        float mountainGen = noise.GetNoise(chunkPosition.X + x, chunkPosition.Y + y);

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
    }
}
