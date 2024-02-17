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
using SlumWarriorsCommon.Gameplay;

namespace SlumWarriorsServer.Entities
{
    public class Player
    {
        public const float MoveSpeed = 1.0f; // Units per second

        public Vector2 Position;
        public float Rotation;

        public bool AutoMove = true;
        public bool HasSpawned = false;
        public Vector2 MovementVec;
        
        public Block?[] CollisionCheck = new Block?[4];
        public Block? BlockAhead;

        public NetPeer? Peer;

        private bool canMove = true;
        private float time = 0f;
        private float moveTimer = 0f;

        private Vector2 lastPosition;

        public Player(int id)
        {
            Engine.Players.Add(id, this);
        }

        // Runs on connection
        public void Start(Vector2 spawnPos)
        {
            if (Peer != null)
            {
                Position = spawnPos;
                Network.SendVector2(Peer, Position, "pu");
            }
        }

        // Runs every tick
        public void Update(float deltaTime)
        {
            time += deltaTime;

            if (HasSpawned && Peer != null)
            {
                var rec = Network.Receive(Peer, "mu");
                
                if (rec != null && rec.Reader != null) // received mu
                {
                    lastPosition = Position;
                
                    // Handle inputs
                    var movement = (MoveDirection)rec.Reader.GetInt();
                
                    switch (movement)
                    {
                        case MoveDirection.Up:
                            MovementVec = -Vector2.UnitY;
                            break;
                        case MoveDirection.Down:
                            MovementVec = Vector2.UnitY;
                            break;
                        case MoveDirection.Left:
                            MovementVec = -Vector2.UnitX;
                            break;
                        case MoveDirection.Right:
                            MovementVec = Vector2.UnitX;
                            break;
                    }

                    // TODO: Player can spam movement each frame
                    // Collision detection / movement
                    Position.X += MovementVec.X;

                    for (int i = 0; i < 2; i++)
                    {
                        var block = CollisionCheck[i];

                        if (block != null && block.Layer == 1)
                        {
                            var isCollidingX = Position.X == (block.Position.X + 0.5f);

                            if (isCollidingX)
                                Position.X = lastPosition.X;
                        }
                    }

                    Position.Y += MovementVec.Y;

                    for (int i = 2; i < 4; i++)
                    {
                        var block = CollisionCheck[i];

                        if (block != null && block.Layer == 1)
                        {
                            var isCollidingY = Position.Y == (block.Position.Y + 0.5f);

                            if (isCollidingY)
                                Position.Y = lastPosition.Y;
                        }
                    }

                    Network.SendVector2(Peer, Position, "pu");
                }
            }
        }

        public void Draw(float deltaTime)
        {
            Raylib.DrawCircleV(Position, 0.5f, Color.Red);

            if (BlockAhead != null)
                Raylib.DrawCircleV(BlockAhead.Position + new Vector2(0.5f, 0.5f), 0.5f, Color.Blue);

            for (int i = 0; i < CollisionCheck.Length; i++)
            {
                var block = CollisionCheck[i];

                if (block != null)
                    Raylib.DrawCircleV(block.Position + new Vector2(0.5f, 0.5f), 0.5f, Color.Blue);
            }
        }
    }
}
