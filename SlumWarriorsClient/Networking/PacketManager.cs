using SlumWarriorsClient.Entities;
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

        private static List<byte> sendBuffer = new List<byte>();

        private static byte[] recBuffer = new byte[0];
        private static byte[]? posBuffer { get { return recBuffer[4..11]; } }
        private static byte[]? rotBuffer { get { return recBuffer[12..15]; } }

        public static void Update()
        {
            // Connection packet
            sendBuffer.AddRange(Encoding.ASCII.GetBytes(Header));
            sendBuffer.Add((byte)PacketType.Connection);
            //sendBuffer.AddRange(Encoding.ASCII.GetBytes(Player.Username));

            if (sendBuffer.Count == 28)
                Engine.Client.Send(sendBuffer.ToArray());

            sendBuffer.Clear();

            while (Engine.IsRunning)
            {
                recBuffer = Engine.Client.Receive();

                if (sendBuffer.Count > 0)
                {
                    Engine.Client.Send(sendBuffer.ToArray());
                }
            }
        }

        public static bool IsPacketValid()
        {
            var bufHeader = recBuffer[0..2];

            if (bufHeader != null)
                return BitConverter.ToString(bufHeader, 0) == Header;

            return false;
        }

        public static PacketType GetPacketType()
        {
            if (IsPacketValid())
                return (PacketType)recBuffer[3];
            else
                return PacketType.NULL;
        }

        public static Vector2? GetPosition()
        {
            if (IsPacketValid() && GetPacketType() == PacketType.Runtime && posBuffer != null)
            {
                var xBuf = posBuffer[0..3];
                var yBuf = posBuffer[4..7];

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

        public static void SetMovement(Vector2 movement)
        {

        }
    }
}
