﻿using LiteNetLib;
using LiteNetLib.Utils;
using SlumWarriorsCommon.Networking;
using SlumWarriorsServer.Entities;
using System;
using System.Collections.Concurrent;
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
        private static EventBasedNetListener listener = new EventBasedNetListener();
        private static NetManager server = new NetManager(listener);

        private static Queue<NetSend> sendQueue = new Queue<NetSend>();
        private static List<NetReceive> receiveQueue = new List<NetReceive>();

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

                Engine.PlayersInitialized++;

                var player = new Player(Engine.PlayersInitialized);
                player.Peer = peer;

                player.Start();
            };

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
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
            server.PollEvents();
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
            server.Stop();
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
