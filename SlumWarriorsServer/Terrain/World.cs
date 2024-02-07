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
            for (int x = 0; x < (testChunk.Blocks.Length / 16); x++)
            {
                for (int y = 0; y < (testChunk.Blocks.Length / 16); y++)
                {
                    var block = new Block();
                    block.Layer = 0;
                    block.Type = BlockType.Grass;
                    block.Position = new Vector2(x, y);
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
