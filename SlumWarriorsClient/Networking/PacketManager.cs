using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SlumWarriorsClient.Networking
{
    public enum PacketType
    {
        NULL,
        Connection,
        Runtime
    }

    // Connection buffer format:
    // 3 bytes - Header
    // 1 byte - Type
    // 24 bytes - Username

    // Runtime buffer format:
    // 3 bytes - Header
    // 1 byte - Type
    // 8 bytes - Position/Input vec
    // 4 bytes - Rotation

    public static class PacketManager
    {
        public const string Header = "swh";

        private static byte[] buffer = new byte[0];

        public static void Update()
        { 
            while (Engine.IsRunning)
                buffer = Engine.Client.Receive();
        }

        public static bool IsPacketValid()
        {
            var bufHeader = buffer[0..2];

            if (bufHeader != null)
                return BitConverter.ToString(bufHeader, 0) == Header;

            return false;
        }

        public static PacketType GetPacketType()
        {
            if (IsPacketValid())
                return (PacketType)buffer[3];
            else
                return PacketType.NULL;
        }

        public static Vector2? GetPosition()
        {
            if (IsPacketValid() && GetPacketType() == PacketType.Runtime)
            {
                var xBuf = buffer[4..7];
                var yBuf = buffer[8..11];

                if (xBuf != null && yBuf != null)
                    return new Vector2(BitConverter.ToSingle(xBuf, 0), BitConverter.ToSingle(yBuf, 0));
            }

            return null;
        }

        public static float GetRotation()
        {
            if (IsPacketValid() && GetPacketType() == PacketType.Runtime)
            {
                return 0f;
            }

            return 0f;
        }
    }
}
