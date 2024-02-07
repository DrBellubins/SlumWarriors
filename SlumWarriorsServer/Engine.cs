using SlumWarriorsCommon.Systems;
using SlumWarriorsServer.Entities;
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

        public static Dictionary<int, Player> Players = new Dictionary<int, Player>();

        public static int PlayersInitialized = 0; // The num of players that ever entered the server (save this to file)

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
                Network.Poll(); // Must be updated first

                foreach (var script in Script.Scripts)
                    script.Update(TickDelta);

                foreach (var player in Players.Values)
                    player.Update(TickDelta);

                Network.Update(TickDelta);
            }

            Network.Stop();
        }
    }
}
