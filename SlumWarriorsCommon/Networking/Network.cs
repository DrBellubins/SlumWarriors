using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using SlumWarriorsCommon.Terrain;
using SlumWarriorsCommon.Utils;

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
        public static string IP { get; private set; } = "localhost";

        public static EventBasedNetListener Listener = new EventBasedNetListener();
        public static NetManager Manager = new NetManager(Listener);

        private static Queue<NetSend> sendQueue = new Queue<NetSend>();
        private static List<NetReceive> receiveQueue = new List<NetReceive>();

        public static void Start(bool isServer)
        {
            if (isServer)
                Manager.Start(NetworkSettings.Port);
            else
            {
                Manager.Start();
                Manager.Connect(IP, NetworkSettings.Port, NetworkSettings.Header);
            }

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
                var tag = dataReader.PeekString(2);

                var receive = new NetReceive(tag, fromPeer, dataReader);
                receiveQueue.Add(receive);
            };
        }

        public static void Poll()
        {
            Manager.PollEvents();
        }

        public static void Update()
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

        public static void SetIP(string ip)
        {
            IP = ip;
        }

        public static NetReceive? Receive(NetPeer peer, string tag)
        {
            foreach (var receive in receiveQueue)
            {
                if (receive.Tag == tag && receive.Peer == peer)
                {
                    var recTag = receive.Reader.GetString(2); // Must be called to remove tag from buffer!

                    receiveQueue.Remove(receive);
                    return receive;
                }
            }

            return null;
        }

        public static void SendInt(NetPeer peer, int integer, string tag)
        {
            var writer = new NetDataWriter();
            writer.Put(tag);
            writer.Put(integer);

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

        public static void SendVector2(NetPeer peer, Vector2 vec, string tag)
        {
            var writer = new NetDataWriter();
            writer.Put(tag);
            writer.Put(vec.X);
            writer.Put(vec.Y);

            var com = new NetSend(peer, writer);
            sendQueue.Enqueue(com);
        }

        public static void SendChunk(NetPeer peer, Chunk chunk, string tag)
        {
            var writer = new NetDataWriter();
            writer.Put(tag);
            writer.Put(SerializeChunk(chunk));

            var com = new NetSend(peer, writer);
            sendQueue.Enqueue(com);
        }

        // TODO (maybe): Only send chunks as a block type grid
        public static byte[] SerializeChunk(Chunk chunk)
        {
            var byteList = new List<byte>();

            byteList.AddRange(BitConverter.GetBytes(343434)); // Integrity check
            byteList.AddRange(BitConverter.GetBytes(chunk.Info.Position.X));
            byteList.AddRange(BitConverter.GetBytes(chunk.Info.Position.Y));

            for (int x = 0; x < chunk.Blocks.GetLength(0); x++)
            {
                for (int y = 0; y < chunk.Blocks.GetLength(1); y++)
                {
                    byteList.AddRange(BitConverter.GetBytes((int)chunk.Blocks[x, y].Info.Type));
                }
            }

            return byteList.ToArray();
        }

        public static Chunk? DeserializeChunk(byte[] chunkBytes)
        {
            Chunk? chunk;
            var index = 4; // Skip header

            var integrityCheck = BitConverter.ToInt32(chunkBytes, index);
            index += sizeof(int);

            var cPosX = BitConverter.ToSingle(chunkBytes, index);
            index += sizeof(float);

            var cPosY = BitConverter.ToSingle(chunkBytes, index);
            index += sizeof(float);

            if (integrityCheck == 343434) // Valid chunk packet
            {
                chunk = new Chunk();
                chunk.Info.Position = new Vector2(cPosX, cPosY);

                for (int x = 0; x < chunk.Blocks.GetLength(0); x++)
                {
                    for (int y = 0; y < chunk.Blocks.GetLength(1); y++)
                    {
                        var block = new Block();

                        block.Info.Type = (BlockType)BitConverter.ToInt32(chunkBytes, index);
                        index += sizeof(int);

                        block.Layer = 0; // TODO: Always set 0 on client
                        block.Position = new Vector2(cPosX + x, cPosY + y) - new Vector2(0.5f, 0.5f);

                        chunk.Blocks[x, y] = block;
                    }
                }

                return chunk;
            }
            else
            {
                Console.WriteLine($"invalid chunk integrity key: {integrityCheck}");
                return null;
            }
        }
    }
}
