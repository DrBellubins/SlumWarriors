using LiteNetLib.Utils;
using LiteNetLib;
using SlumWarriorsCommon.Systems;
using SlumWarriorsServer.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SlumWarriorsServer.Entities
{
    public class Player
    {
        public Vector2 Position;
        public float Rotation;

        public Vector2 MovementVec;

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

                    if (rec != null && rec.Reader != null)
                        Position += new Vector2(rec.Reader.GetFloat(), rec.Reader.GetFloat());

                    Network.SendVector2(Peer, Position, "pu");
                }
            }
        }
    }
}
