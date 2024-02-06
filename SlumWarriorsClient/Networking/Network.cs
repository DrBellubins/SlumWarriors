using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using LiteNetLib;
using LiteNetLib.Utils;
using SlumWarriorsCommon;

namespace SlumWarriorsClient.Networking
{
    public static class Network
    {
        private static NetManager client = new NetManager(listener);
        private static EventBasedNetListener listener = new EventBasedNetListener();

        public static void Start()
        {
            client.Start();
            client.Connect("localhost", NetworkSettings.Port, NetworkSettings.Header);

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
            {
                Console.WriteLine(dataReader.GetString(100));
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
