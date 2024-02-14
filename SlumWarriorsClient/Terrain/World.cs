using SlumWarriorsCommon.Networking;
using SlumWarriorsCommon.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Raylib_cs;

using static Raylib_cs.Raylib;
using SlumWarriorsCommon.Utils;

namespace SlumWarriorsClient.Terrain
{
    // TODO: Separate Block class on client to a lighterweight ClientBlock
    public class World
    {
        public List<Chunk> Chunks = new List<Chunk>();

        public void Update(float deltaTime)
        {
            if (Engine.Server != null)
            {
                var rec = Network.Receive(Engine.Server, "wu"); // world update

                if (rec != null)
                {
                    var chunk = Network.DeserializeChunk(rec.Reader.RawData);

                    if (chunk != null) // TODO: Validate chunk integrity
                    {
                        Console.WriteLine($"Received chunk pos: {chunk.Info.Position}");
                        Chunks.Add(chunk);
                    }
                }
            }
        }

        public void Draw(float deltaTime)
        {
            foreach (var chunk in Chunks)
            {
                for (int x = 0; x < chunk.Blocks.GetLength(0); x++)
                {
                    for (int y = 0; y < chunk.Blocks.GetLength(1); y++)
                    {
                        var block = chunk.Blocks[x, y];

                        var origTextureRect = new Rectangle(0f, 0f, 8f, 8f);
                        var newTextureRect = new Rectangle(block.Position.X,
                                block.Position.Y, 1f, 1f);

                        DrawTexturePro(Block.Textures[block.Info.Type],
                                                origTextureRect, newTextureRect, Vector2.Zero, 0f, Color.White);
                    }
                }
            }
        }
    }
}
