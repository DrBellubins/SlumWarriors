using LiteNetLib;
using LiteNetLib.Utils;
using SlumWarriorsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

// Connection buffer format:
// 3 bytes - Header
// 1 byte - Type
// 24 bytes - Username

// Runtime buffer format:
// 3 bytes - Header
// 1 byte - Type
// 8 bytes - Position/Input vec
// 4 bytes - Rotation

namespace SlumWarriorsServer.Networking
{
    public static class Network
    {
        private static EventBasedNetListener listener = new EventBasedNetListener();
        private static NetManager server = new NetManager(listener);

        public static void Start()
        {
            server.Start(NetworkSettings.Port);

            listener.ConnectionRequestEvent += request =>
            {
                if (server.ConnectedPeersCount < 10)
                    request.AcceptIfKey(NetworkSettings.Header);
                else
                {
                    Console.WriteLine("Connection rejected: Too many players!");
                    request.Reject();
                }
            };

            listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine("We got connection: {0}", peer.Address);
                var writer = new NetDataWriter();

                writer.Put("fc");

                peer.Send(writer, DeliveryMethod.ReliableOrdered);
            };

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
            {
                var writer = new NetDataWriter();

                if (dataReader.GetString(2) == "sr") // spawnpoint request
                {
                    writer.Put("pu");
                    writer.Put(10.5f);
                    writer.Put(10.5f);

                    fromPeer.Send(writer, DeliveryMethod.ReliableOrdered);
                }

                dataReader.Recycle();
            };
        }

        public static void Update()
        {
            server.PollEvents();
        }

        public static void Stop()
        {
            server.Stop();
        }
    }
}
