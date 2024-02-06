using SlumWarriorsCommon.Systems;
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
        public const int TickRate = 60;
        public const int TickDelay = (int)((1f / TickRate) * 1000f);
        public const float TickDelta = 1.0f / TickRate;

        public static bool IsRunning;

        public static Server Server = new Server();

        public void Initialize()
        {
            IsRunning = true;

            // Start
            foreach (var script in Script.Scripts)
                script.Start();

            while (IsRunning)
            {
                Thread.Sleep(TickDelay);

                Console.WriteLine($"Test {TickDelay}");

                // Update
                PacketManager.Update(); // Must be updated first

                foreach (var script in Script.Scripts)
                    script.Update(TickDelta);
            }
        }
    }
}
