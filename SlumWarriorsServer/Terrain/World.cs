using SlumWarriorsCommon.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using SlumWarriorsCommon.Networking;
using Raylib_cs;
using SlumWarriorsCommon.Utils;

namespace SlumWarriorsServer.Terrain
{
    public class World
    {
        public Block BlockAheadPlayer = new Block();

        private Chunk[,] renderedChunks = new Chunk[64, 64];

        private int sqrtRenderDistance;
        private FastNoiseLite noise = new FastNoiseLite();

        private Thread? chunkThread;

        public int PercentComplete = 0;
        private int genCounter = 0;

        public void Start()
        {
            sqrtRenderDistance = (int)MathF.Sqrt(renderedChunks.Length);

            noise.SetSeed(new Random().Next(int.MinValue, int.MaxValue));

            chunkThread = new Thread(() => generateAllChunks());
            chunkThread.Start();
        }

        bool sent = false;

        public void Update(float tickDelta)
        {
            PercentComplete = (int)Math.Round((double)(100 * genCounter) / (256 * renderedChunks.Length));

            if (PercentComplete < 100)
                Console.WriteLine($"Gen progress: {PercentComplete}");

            foreach (var player in Engine.Players.Values)
            {
                if (player != null)
                {
                    player.CollisionCheck[0] = GetBlockAtPos(player.Position + Vector2.UnitX); // right
                    player.CollisionCheck[1] = GetBlockAtPos(player.Position + -Vector2.UnitX); // left
                    player.CollisionCheck[2] = GetBlockAtPos(player.Position + Vector2.UnitY); // down
                    player.CollisionCheck[3] = GetBlockAtPos(player.Position + -Vector2.UnitY); // up

                    if (player.Peer != null && !sent)
                    {
                        for (int cx = 0; cx < sqrtRenderDistance; cx++)
                        {
                            for (int cy = 0; cy < sqrtRenderDistance; cy++)
                            {
                                var chunk = renderedChunks[cx, cy];

                                if (player.HasSpawned && chunk != null)
                                {
                                    var cPlayerPos = GameMath.NearestChunkCoord(player.Position);

                                    // TODO: Sometimes sends corrupted chunks to client
                                    // Or just doesn't send chunks at all
                                    if (Vector2.Distance(cPlayerPos, chunk.Info.Position) < 64f)
                                        Network.SendChunk(player.Peer, chunk, "wu");
                                }
                            }
                        }

                        sent = true; // TODO: not executed per-player
                    }
                }
            }
        }

        public void Draw(float tickDelta, Camera2D cam)
        {
            for (int cx = 0; cx < sqrtRenderDistance; cx++)
            {
                for (int cy = 0; cy < sqrtRenderDistance; cy++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        for (int y = 0; y < 16; y++)
                        {
                            if (chunkThread != null && chunkThread.ThreadState != ThreadState.Running)
                            {
                                var block = renderedChunks[cx, cy].Blocks[x, y];

                                var blockRect = new Rectangle(block.Position.X,
                                        block.Position.Y, 1f, 1f);

                                // TODO: Doesn't reduce lag, somehow...
                                if (Vector2.Distance(block.Position, Raylib.GetScreenToWorld2D(
                                    Raylib.GetMousePosition(), cam)) < 64f)
                                {
                                    Raylib.DrawRectangleLinesEx(blockRect, 0.1f, Block.Colors[block.Info.Type]);
                                }
                            }
                        }
                    }
                }
            }
        }

        // TODO: Make chunk generation multi-threaded (oh boi here we go)
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

                    genCounter++;
                }
            }

            return chunk;
        }

        public async Task<Vector2> GetSpawnPos()
        {
            var rndVec = Vector2.Zero;
            Block? blockCheck = null;

            while (blockCheck == null) // Potentially very slow
            {
                await Task.Delay(25);

                var rndX = GameMath.GetXorFloat(-(renderedChunks.Length / 2), renderedChunks.Length / 2);
                var rndY = GameMath.GetXorFloat(-(renderedChunks.Length / 2), renderedChunks.Length / 2);

                rndVec = GameMath.NearestBlockCoord(new Vector2(rndX, rndY));

                blockCheck = GetBlockAtPos(rndVec);

                // TODO: Could spawn players out of ounds
                if (blockCheck != null && blockCheck.Layer == 1)
                    blockCheck = null;
            }

            return rndVec;
        }

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
