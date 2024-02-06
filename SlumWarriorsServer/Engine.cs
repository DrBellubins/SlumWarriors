using SlumWarriorsServer.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlumWarriorsServer
{
    internal class Engine
    {
        public static bool IsRunning;

        public static Server Server = new Server();

        public void Initialize()
        {
            while (IsRunning)
            {
                PacketManager.Update(); // Must be updated first
            }
        }
    }
}
