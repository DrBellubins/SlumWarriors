using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SlumWarriorsServer.Networking
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
        private static byte[]? movBuffer { get { return buffer[4..11]; } }
        private static byte[]? rotBuffer { get { return buffer[12..15]; } }

        public static void Update()
        {
            while (Engine.IsRunning)
                buffer = Engine.Server.Receive();
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

        public static Vector2? GetMovement()
        {
            if (IsPacketValid() && GetPacketType() == PacketType.Runtime && movBuffer != null)
            {
                var xBuf = movBuffer[0..3];
                var yBuf = movBuffer[4..7];

                if (xBuf != null && yBuf != null)
                    return new Vector2(BitConverter.ToSingle(xBuf, 0), BitConverter.ToSingle(yBuf, 0));
            }

            return null;
        }

        public static float GetRotation()
        {
            if (IsPacketValid() && GetPacketType() == PacketType.Runtime && rotBuffer != null)
            {
                var rotBuf = rotBuffer[0..3];

                if (rotBuf != null)
                    return BitConverter.ToSingle(rotBuf, 0);
            }

            return 0f;
        }
    }
}
