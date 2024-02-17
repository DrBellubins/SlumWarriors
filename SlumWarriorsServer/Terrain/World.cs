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

using static SlumWarriorsCommon.Terrain.WorldGen;

namespace SlumWarriorsServer.Terrain
{
    // TODO: Collision no longer works...
    public class World
    {
        public Block BlockAheadPlayer = new Block();

        private Thread? chunkThread;

        public void Start()
        {
            WorldGen.Start(new Random().Next(int.MinValue, int.MaxValue));

            chunkThread = new Thread(() => GenerateAllChunks());
            chunkThread.Start();
        }

        public void Update(float tickDelta)
        {
            foreach (var player in Engine.Players.Values)
            {
                if (player != null)
                {
                    player.CollisionCheck[0] = GetBlockAtPos(player.Position + Vector2.UnitX); // right
                    player.CollisionCheck[1] = GetBlockAtPos(player.Position + -Vector2.UnitX); // left
                    player.CollisionCheck[2] = GetBlockAtPos(player.Position + Vector2.UnitY); // down
                    player.CollisionCheck[3] = GetBlockAtPos(player.Position + -Vector2.UnitY); // up

                    if (player.Peer != null && !player.HasSpawned)
                    {
                        for (int cx = 0; cx < SqrtRenderDistance; cx++)
                        {
                            for (int cy = 0; cy < SqrtRenderDistance; cy++)
                            {
                                var chunk = RenderedChunks[cx, cy];

                                if (chunk != null)
                                {
                                    var cPlayerPos = GameMath.NearestChunkCoord(player.Position);

                                    // TODO: Not all chunks get sent (might be packet corruption?)
                                    if (Vector2.Distance(cPlayerPos, chunk.Info.Position) < 64f)
                                        Network.SendChunkInfo(player.Peer, chunk.Info, "cu");
                                        //Network.SendChunk(player.Peer, chunk, "wu");
                                }
                            }
                        }

                        player.HasSpawned = true;
                    }
                }
            }
        }

        public void Draw(float tickDelta, Camera2D cam)
        {
            for (int cx = 0; cx <  SqrtRenderDistance; cx++)
            {
                for (int cy = 0; cy < SqrtRenderDistance; cy++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        for (int y = 0; y < 16; y++)
                        {
                            if (chunkThread != null && chunkThread.ThreadState != ThreadState.Running)
                            {
                                var block = RenderedChunks[cx, cy].Blocks[x, y];

                                // TODO: Doesn't reduce lag, somehow...
                                if (Vector2.Distance(block.Position, Raylib.GetScreenToWorld2D(
                                    Raylib.GetMousePosition(), cam)) < 64f)
                                {
                                    var blockRect = new Rectangle(block.Position.X,
                                        block.Position.Y, 1f, 1f);

                                    Raylib.DrawRectangleLinesEx(blockRect, 0.1f, Block.Colors[block.Info.Type]);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
