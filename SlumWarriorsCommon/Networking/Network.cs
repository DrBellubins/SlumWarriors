using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;

namespace SlumWarriorsCommon.Networking
{
    public class NetSend
    {
        public NetPeer Peer;
        public NetDataWriter Writer;

        public NetSend(NetPeer peer, NetDataWriter writer)
        {
            Peer = peer;
            Writer = writer;
        }
    }

    public class NetReceive
    {
        public string Tag;
        public NetPeer Peer;
        public NetDataReader Reader;

        public NetReceive(string tag, NetPeer peer, NetDataReader reader)
        {
            Tag = tag;
            Peer = peer;
            Reader = reader;
        }
    }

    public static class Network
    {
        public static EventBasedNetListener Listener = new EventBasedNetListener();
        public static NetManager Manager = new NetManager(Listener);

        private static Queue<NetSend> sendQueue = new Queue<NetSend>();
        private static List<NetReceive> receiveQueue = new List<NetReceive>();

        public static void Start()
        {
            Manager.Start(NetworkSettings.Port);

            Listener.ConnectionRequestEvent += request =>
            {
                if (Manager.ConnectedPeersCount < 10)
                    request.AcceptIfKey(NetworkSettings.Header);
                else
                {
                    Console.WriteLine("Connection rejected: Too many players!");
                    request.Reject();
                }
            };

            Listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine("We got connection: {0}", peer.Address);
            };

            Listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
            {
                var writer = new NetDataWriter();

                var tag = dataReader.GetString(2);

                var receive = new NetReceive(tag, fromPeer, dataReader);
                receiveQueue.Add(receive);

                dataReader.Recycle();
            };
        }

        public static void Poll()
        {
            Manager.PollEvents();
        }

        public static void Update(float tickDelta)
        {
            for (int i = 0; i < sendQueue.Count; i++)
            {
                var send = sendQueue.Dequeue();

                if (send.Peer != null)
                    send.Peer.Send(send.Writer, DeliveryMethod.ReliableOrdered);
            }
        }

        public static void Stop()
        {
            Manager.Stop();
        }

        public static NetReceive? Receive(NetPeer peer, string tag)
        {
            foreach (var receive in receiveQueue)
            {
                if (receive.Peer == peer && receive.Tag == tag)
                {
                    receiveQueue.Remove(receive);
                    return receive;
                }
            }

            return null;
        }

        public static void SendVector2(NetPeer peer, Vector2 vec, string tag)
        {
            var writer = new NetDataWriter();
            writer.Put(tag);
            writer.Put(vec.X);
            writer.Put(vec.Y);

            var com = new NetSend(peer, writer);
            sendQueue.Enqueue(com);
        }

        public static void SendSingle(NetPeer peer, float single, string tag)
        {
            var writer = new NetDataWriter();
            writer.Put(tag);
            writer.Put(single);

            var com = new NetSend(peer, writer);
            sendQueue.Enqueue(com);
        }
    }
}
