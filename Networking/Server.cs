using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SlumWarriors.Networking
{
    public class Server
    {
        public const int Port = 55404;

        internal UdpClient UDPServer = new UdpClient(Port);
        private IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, Port);

        public byte[] Receive()
        {
            return UDPServer.Receive(ref clientEndPoint);
        }

        public void Send(byte[] data)
        {
            UDPServer.Send(data);
        }
    }
}
