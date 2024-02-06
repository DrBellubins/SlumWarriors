using LiteNetLib;
using LiteNetLib.Utils;
using SlumWarriorsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlumWarriorsServer.Networking
{
    public static class Network
    {
        private static NetManager server = new NetManager(listener);
        private static EventBasedNetListener listener = new EventBasedNetListener();

        public static void Start()
        {
            server.Start(NetworkSettings.Port);

            listener.ConnectionRequestEvent += request =>
            {
                if (server.ConnectedPeersCount < 10)
                    request.AcceptIfKey(NetworkSettings.Header);
                else
                    request.Reject();
            };

            listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine("We got connection: {0}", peer.Address); // Show peer ip
                NetDataWriter writer = new NetDataWriter(); // Create writer class

                writer.Put("Hello client!");

                peer.Send(writer, DeliveryMethod.ReliableOrdered); // Send with reliability
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
