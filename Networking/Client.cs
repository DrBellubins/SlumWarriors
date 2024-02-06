using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SlumWarriors.Networking
{
    public class Client
    {
        internal UdpClient UDPClient = new UdpClient(Server.Port);

        // TODO: Change loopback to actual address
        private IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback, Server.Port);

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
