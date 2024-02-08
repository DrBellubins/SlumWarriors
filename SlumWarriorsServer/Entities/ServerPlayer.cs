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

namespace SlumWarriorsServer.Entities
{
    public class ServerPlayer
    {
        public Vector2 Position;
        public float Rotation;

        public Vector2 MovementVec;

        public NetPeer? Peer;

        private bool hasSpawned = false;

        public ServerPlayer(int id)
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
        public void Update(float deltaTime, Block[,] collCheck)
        {
            if (hasSpawned)
            {
                if (Peer != null)
                {
                    var rec = Network.Receive(Peer, "mu");

                    if (rec != null && rec.Reader != null) // received mu
                    {
                        var movementVec = Vector2.Normalize(new Vector2(rec.Reader.GetFloat(), rec.Reader.GetFloat()));
                        Position += movementVec;

                        Network.SendVector2(Peer, Position, "pu"); // Never gets sent
                    }
                }
            }
        }
    }
}
