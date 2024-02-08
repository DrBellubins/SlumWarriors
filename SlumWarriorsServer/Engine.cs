using SlumWarriorsCommon.Systems;
using SlumWarriorsCommon.Networking;
using SlumWarriorsServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlumWarriorsServer.Terrain;
using SlumWarriorsCommon.Terrain;

namespace SlumWarriorsServer
{
    internal class Engine
    {
        public const int TickRate = 60;
        public const int TickDelay = (int)((1f / TickRate) * 1000f);
        public const float TickDelta = 1.0f / TickRate;

        public static bool IsRunning;

        public static Dictionary<int, ServerPlayer> Players = new Dictionary<int, ServerPlayer>();

        public static int PlayersInitialized = 0; // The num of players that ever entered the server (save this to file)

        public void Initialize()
        {
            IsRunning = true;

            Block.InitializeBlockPrefabs(true);

            // Start
            Network.Start(true);

            Network.Listener.PeerConnectedEvent += peer =>
            {
                PlayersInitialized++;

                var player = new ServerPlayer(PlayersInitialized);
                player.Peer = peer;

                player.Start();
            };

            var world = new World();
            world.Start();

            foreach (var script in Script.Scripts)
                script.Start();

            while (IsRunning)
            {
                Thread.Sleep(TickDelay);

                // Update
                Network.Poll(); // Must be updated first

                world.Update(TickDelta);

                foreach (var script in Script.Scripts)
                    script.Update(TickDelta);

                foreach (var player in Players.Values)
                    player.Update(TickDelta, world.CollisionCheck);

                Network.Update();
            }

            Network.Stop();
        }
    }
}
