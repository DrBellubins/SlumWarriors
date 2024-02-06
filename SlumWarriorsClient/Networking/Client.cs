﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using SlumWarriorsCommon;

namespace SlumWarriorsClient.Networking
{
    public class Client
    {
        private ConcurrentBag<byte[]> packetBuffer = new ConcurrentBag<byte[]>();

        internal UdpClient UDPClient = new UdpClient(NetworkSettings.Port);

        // TODO: Change loopback to actual address
        private IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback, NetworkSettings.Port);

        public byte[] Receive()
        {
            return UDPClient.Receive(ref serverEndPoint);
        }

        public void Send(byte[] data)
        {
            UDPClient.Send(data);
        }
    }
}
