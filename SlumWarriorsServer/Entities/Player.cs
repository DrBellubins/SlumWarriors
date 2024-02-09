using LiteNetLib.Utils;
using LiteNetLib;
using SlumWarriorsCommon.Systems;
using SlumWarriorsCommon.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using SlumWarriorsCommon.Terrain;

using Raylib_cs;

namespace SlumWarriorsServer.Entities
{
    public class Player
    {
        public Vector2 Position;
        public float Rotation;

        public Vector2 MovementVec;
        //public Block?[] CollisionCheck = new Block?[4];
        public Block? BlockAhead;

        public NetPeer? Peer;

        private bool hasSpawned = false;

        public Player(int id)
        {
            Engine.Players.Add(id, this);
        }

        // Runs on connection
        public void Start()
        {
            if (Peer != null)
            {
                var spawnPos = new Vector2(10f, 10f);
                Network.SendVector2(Peer, spawnPos, "pu");

                Position = spawnPos;
            }

            hasSpawned = true;
        }

        // Runs every tick
        public void Update(float deltaTime)
        {
            if (hasSpawned)
            {
                if (Peer != null)
                {
                    var rec = Network.Receive(Peer, "mu");

                    if (rec != null && rec.Reader != null) // received mu
                    {
                        MovementVec = Vector2.Normalize(new Vector2(rec.Reader.GetFloat(), rec.Reader.GetFloat()));

                        if (BlockAhead != null && BlockAhead.Layer != 1)
                            Position += MovementVec;

                        /*var colMoveVec = Vector2.Zero;

                        for (int i = 0; i < 4; i++)
                        {
                            var block = CollisionCheck[i];

                            if (block != null && block.Layer != 1)
                            {
                                colMoveVec = MovementVec;
                            }
                        }

                        Position += colMoveVec;*/

                        Network.SendVector2(Peer, Position, "pu");

                        //MovementVec = Vector2.Zero;
                    }
                }
            }
        }

        public void Draw(float deltaTime)
        {
            Raylib.DrawCircleV(Position, 0.5f, Color.Red);

            if (BlockAhead != null)
                Raylib.DrawCircleV(BlockAhead.Position + new Vector2(0.5f, 0.5f), 0.5f, Color.Blue);

            /*for (int i = 0; i < CollisionCheck.Length; i++)
            {
                var block = CollisionCheck[i];

                if (block != null)
                    Raylib.DrawCircleV(block.Position + new Vector2(0.5f, 0.5f), 0.5f, Color.Blue);
            }*/
        }
    }
}
