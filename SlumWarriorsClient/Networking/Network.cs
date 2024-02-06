using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using LiteNetLib;
using LiteNetLib.Utils;

namespace SlumWarriorsClient.Networking
{
    public static class Network
    {
        private static NetManager client = new NetManager(listener);
        private static EventBasedNetListener listener = new EventBasedNetListener();

        public static void Start()
        {
            client.Start();
            client.Connect("localhost", 55404, "SWH");

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
            {
                Engine.CurrentPlayer.Position = new Vector2(dataReader.GetFloat(), dataReader.GetFloat());
                dataReader.Recycle();

                var writer = new NetDataWriter();
                writer.Put(Engine.CurrentPlayer.MovementVec.X);
                writer.Put(Engine.CurrentPlayer.MovementVec.Y);
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
