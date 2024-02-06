using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SlumWarriors.Networking
{
    public enum PacketBufferTypes
    {
        NULL,
        Connection,
        Runtime
    }

    // Buffer types:
    // C - Connection
    // R - Runtime

    // Connection buffer format:
    // 3 bytes - Header
    // 1 byte - Type
    // 24 bytes - Username

    // Runtime buffer format:
    // 3 bytes - Header
    // 1 byte - Type
    // 8 bytes - Position/Input vec
    // 4 bytes - Rotation

    public class Packet
    {
        public PacketBufferTypes Type;
        public string? Username;

        public Packet(PacketBufferTypes type, string? username = null)
        {
            Type = type;
            Username = username;
        }

        public Vector2 GetPosition()
        {
            return Vector2.Zero;
        }
    }
}
