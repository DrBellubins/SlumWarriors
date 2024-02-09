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
using Raylib_cs;

namespace SlumWarriorsServer
{
    internal class Engine
    {
        public const int TickRate = 60;
        public const int TickDelay = (int)((1f / TickRate) * 1000f);
        public const float TickDelta = 1.0f / TickRate;

        public static bool IsRunning;

        // Debug window stuff
        public const int ScreenWidth = 1600;
        public const int ScreenHeight = 900;
        public static Font MainFont;
        public static DebugCamera DebugCamera = new DebugCamera();

        public static Dictionary<int, Player> Players = new Dictionary<int, Player>();

        public static int PlayersInitialized = 0; // The num of players that ever entered the server (save this to file)

        public void Initialize()
        {
            IsRunning = true;

            // Debug window stuff
            Raylib.InitWindow(ScreenWidth, ScreenHeight, "Slum Warriors Server Debug");
            Raylib.SetExitKey(KeyboardKey.Null);
            Raylib.SetTargetFPS(60);
            MainFont = Raylib.LoadFontEx("Assets/Fonts/VarelaRound-Regular.ttf", 64, null, 250);

            Block.InitializeBlockPrefabs(true);

            // Start
            Network.Start(true);

            Network.Listener.PeerConnectedEvent += peer =>
            {
                PlayersInitialized++;

                var player = new Player(PlayersInitialized);
                player.Peer = peer;

                player.Start();
            };

            var world = new World();
            world.Start();

            foreach (var script in Script.Scripts)
                script.Start();

            DebugCamera.Start();

            while (IsRunning)
            {
                Thread.Sleep(TickDelay);

                // Update
                Network.Poll(); // Must be updated first

                

                foreach (var script in Script.Scripts)
                    script.Update(TickDelta);

                foreach (var player in Players.Values)
                    player.Update(TickDelta);

                world.Update(TickDelta);

                Network.Update();

                DebugCamera.Update(TickDelta);

                // Draw debug window
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                Raylib.BeginMode2D(DebugCamera.Camera);

                world.Draw(TickDelta);

                foreach (var player in Players.Values)
                    player.Draw(TickDelta);

                Raylib.EndMode2D();
                Raylib.EndDrawing();
            }

            Network.Stop();
        }
    }
}
