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
        private Chunk testChunk = new Chunk();

        public void Start()
        {
            var pingPong = false;

            for (int x = 0; x < testChunk.Blocks.GetLength(0); x++)
            {
                for (int y = 0; y < testChunk.Blocks.GetLength(1); y++)
                {
                    pingPong = !pingPong;

                    var block = new Block();
                    block.Layer = 0;

                    if (pingPong)
                        block.Type = BlockType.Grass;
                    else
                        block.Type = BlockType.Dirt;

                    block.Position = new Vector2(x, y);

                    testChunk.Blocks[x, y] = block;
                }
            }
        }

        bool sent = false;

        public void Update(float tickDelta)
        {
            foreach (var player in Engine.Players.Values)
            {
                if (player != null && player.Peer != null && !sent)
                {
                    Network.SendChunk(player.Peer, testChunk, "wu"); // world update
                    sent = true;
                }
            }
        }
    }
}
