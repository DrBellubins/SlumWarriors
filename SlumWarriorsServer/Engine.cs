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

        public void Initialize()
        {
            IsRunning = true;

            // Start
            Network.Start();

            foreach (var script in Script.Scripts)
                script.Start();

            while (IsRunning)
            {
                Thread.Sleep(TickDelay);

                // Update
                Network.Update(); // Must be updated first

                foreach (var script in Script.Scripts)
                    script.Update(TickDelta);
            }

            Network.Stop();
        }
    }
}
