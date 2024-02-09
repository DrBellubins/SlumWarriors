using SlumWarriorsCommon.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using SlumWarriorsCommon.Networking;
using Raylib_cs;

namespace SlumWarriorsServer.Terrain
{
    public class World
    {
        public Block BlockAheadPlayer = new Block();

        private Chunk[,] renderedChunks = new Chunk[8, 8];

        private int sqrtRenderDistance;
        private FastNoiseLite noise = new FastNoiseLite();

        public void Start()
        {
            sqrtRenderDistance = (int)MathF.Sqrt(renderedChunks.Length);

            noise.SetSeed(999);

            generateAllChunks();
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
                if (player != null)
                {
                    player.CollisionCheck[0] = GetBlockAtPos(player.Position + Vector2.UnitX); // right
                    player.CollisionCheck[1] = GetBlockAtPos(player.Position + -Vector2.UnitX); // left
                    player.CollisionCheck[2] = GetBlockAtPos(player.Position + Vector2.UnitY); // down
                    player.CollisionCheck[3] = GetBlockAtPos(player.Position + -Vector2.UnitY); // up

                    /*var bAhead = GetBlockAtPos(player.Position + player.MovementVec);

                    if (bAhead != null)
                    {
                        //Console.WriteLine($"bblock: {bAhead.Position}");
                        player.BlockAhead = bAhead;
                    }*/

                    if (player.Peer != null && !sent)
                    {
                        for (int cx = 0; cx < sqrtRenderDistance; cx++)
                        {
                            for (int cy = 0; cy < sqrtRenderDistance; cy++)
                            {
                                Network.SendChunk(player.Peer, renderedChunks[cx, cy], "wu");
                            }
                        }

                        sent = true; // TODO: not executed per-player
                    }
                }
            }
        }

        public void Draw(float tickDelta)
        {
            for (int cx = 0; cx < sqrtRenderDistance; cx++)
            {
                for (int cy = 0; cy < sqrtRenderDistance; cy++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        for (int y = 0; y < 16; y++)
                        {
                            var block = renderedChunks[cx, cy].Blocks[x, y];

                            var blockRect = new Rectangle(block.Position.X,
                                    block.Position.Y, 1f, 1f);

                            Raylib.DrawRectangleLinesEx(blockRect, 0.1f, Block.Colors[block.Info.Type]);
                        }
                    }
                }
            }
        }

        private void generateAllChunks()
        {
            for (int cx = 0; cx < sqrtRenderDistance; cx++)
            {
                for (int cy = 0; cy < sqrtRenderDistance; cy++)
                {
                    // We start at nearest chunk pos to player, 
                    // multiply current chunk pos by num of
                    // blocks in chunk (32), then offset by
                    // sqrtRenderDistance multiplied by half
                    // num of blocks in chunk
                    var chunkPos = new Vector2((cx * 16) - (sqrtRenderDistance * 8),
                        (cy * 16) - (sqrtRenderDistance * 8));

                    renderedChunks[cx, cy] = generateChunk(chunkPos);
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

        // TODO: Doesn't return a block
        private Block? GetBlockAtPos(Vector2 pos)
        {
            for (int cx = 0; cx < sqrtRenderDistance; cx++)
            {
                for (int cy = 0; cy < sqrtRenderDistance; cy++)
                {
                    if (renderedChunks[cx, cy] != null)
                    {
                        var currentChunk = renderedChunks[cx, cy];

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
