using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using LiteNetLib;
using LiteNetLib.Utils;
using SlumWarriorsCommon;

// Connection buffer format:
// 3 bytes - Header
// 1 byte - Type
// 24 bytes - Username

// Runtime buffer format:
// 3 bytes - Header
// 1 byte - Type
// 8 bytes - Position/Input vec
// 4 bytes - Rotation

namespace SlumWarriorsClient.Networking
{
    public static class Network
    {
        private static EventBasedNetListener listener = new EventBasedNetListener();
        private static NetManager client = new NetManager(listener);

        public static void Start()
        {
            client.Start();
            client.Connect("localhost", NetworkSettings.Port, NetworkSettings.Header);

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
            {
                var writer = new NetDataWriter();

                var cType = dataReader.GetString(2);

                if (cType == "fc") // First connection
                {
                    writer.Put("sr"); // spawnpoint request
                    fromPeer.Send(writer, DeliveryMethod.ReliableOrdered);
                }
                else if (cType == "pu") // Pos Update
                {
                    Engine.CurrentPlayer.Position = new Vector2(dataReader.GetFloat(), dataReader.GetFloat());
                }

                dataReader.Recycle();
            };
        }

        public static void Update()
        {
            client.PollEvents();
        }

        public static void Stop()
        {
            client.Stop();
        }
    }
}
