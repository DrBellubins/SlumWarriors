using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using LiteNetLib;
using LiteNetLib.Utils;
using SlumWarriorsCommon.Networking;

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

        private static NetPeer? server;

        private static Queue<NetDataWriter> bufferQueue = new Queue<NetDataWriter>();

        public static void Start()
        {
            client.Start();
            client.Connect("localhost", NetworkSettings.Port, NetworkSettings.Header);

            listener.PeerConnectedEvent += peer =>
            {
                server = peer;
            };

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
            {
                var writer = new NetDataWriter();

                var pType = dataReader.GetString(2);

                if (pType == "pu") // Pos Update
                    Engine.CurrentPlayer.Position = new Vector2(dataReader.GetFloat(), dataReader.GetFloat());

                dataReader.Recycle();
            };
        }

        public static void Poll()
        {
            client.PollEvents();
        }

        public static void Update(float deltaTime)
        {
            for (int i = 0; i < bufferQueue.Count; i++)
            {
                var com = bufferQueue.Dequeue();

                if (server != null)
                    server.Send(com, DeliveryMethod.ReliableOrdered);
            }
        }

        public static void Stop()
        {
            client.Stop();
        }

        public static void SendMovement(Vector2 movement)
        {
            if (server != null)
            {
                var writer = new NetDataWriter();

                writer.Put("mu"); // movement update
                writer.Put(movement.X);
                writer.Put(movement.Y);

                bufferQueue.Enqueue(writer);
            }
        }
    }
}
