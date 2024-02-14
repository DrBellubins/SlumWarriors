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
using SlumWarriorsCommon.Utils;
using System.Numerics;

namespace SlumWarriorsServer
{
    internal class Engine
    {
        public const int TickRate = 60;
        public const int TickDelay = (int)((1f / TickRate) * 1000f);
        //public const float TickDelta = 1.0f / TickRate;
        public const bool DrawDebugWindow = false;

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
            if (DrawDebugWindow)
            {
                Raylib.InitWindow(ScreenWidth, ScreenHeight, "Slum Warriors Server Debug");
                Raylib.SetExitKey(KeyboardKey.Null);
                Raylib.SetTargetFPS(60);
                MainFont = Raylib.LoadFontEx("Assets/Fonts/VarelaRound-Regular.ttf", 64, null, 250);
            }

            UI.Init(ScreenWidth, ScreenHeight, MainFont);
            Block.InitializeBlockPrefabs(true);

            var previousTimer = DateTime.Now;
            var currentTimer = DateTime.Now;

            var time = 0.0f;
            var deltaTime = 0.0f;

            // Start
            GameMath.InitXorRNG();
            Network.Start(true);

            var world = new World();
            world.Start();

            Network.Listener.PeerConnectedEvent += peer =>
            {
                PlayersInitialized++;

                var player = new Player(PlayersInitialized);
                player.Peer = peer;

                var spawnPos = Task.Run(() => world.GetSpawnPos());
                player.Start(spawnPos.Result);
            };

            foreach (var script in Script.Scripts)
                script.Start();

            DebugCamera.Start();

            while (IsRunning)
            {
                Thread.Sleep(TickDelay);
                currentTimer = DateTime.Now;

                // Update
                deltaTime = (currentTimer.Ticks - previousTimer.Ticks) / 10000000f;
                time += deltaTime;

                Network.Poll(); // Must be updated first

                foreach (var script in Script.Scripts)
                    script.Update(deltaTime);

                foreach (var player in Players.Values)
                    player.Update(deltaTime);

                world.Update(deltaTime);

                Network.Update();

                DebugCamera.Update(deltaTime);

                // Draw debug window
                if (DrawDebugWindow)
                {
                    Raylib.BeginDrawing();
                    Raylib.ClearBackground(Color.Black);

                    Raylib.BeginMode2D(DebugCamera.Camera);

                    world.Draw(deltaTime, DebugCamera.Camera);

                    var mouseWorldPos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), DebugCamera.Camera);

                    foreach (var player in Players.Values)
                    {
                        Raylib.DrawLineEx(mouseWorldPos, player.Position, 0.1f, Color.Blue);
                        player.Draw(deltaTime);
                    }

                    Raylib.EndMode2D();

                    // UI
                    var mouseBlockPos = GameMath.NearestBlockCoord(mouseWorldPos);
                    UI.DrawText($"Pos: {mouseBlockPos}", 14, Raylib.GetMousePosition() + new Vector2(25, 0));

                    UI.DrawText($"FPS: {Raylib.GetFPS()}\nGen completed: {world.PercentComplete}%", Vector2.Zero);
                    Raylib.EndDrawing();
                }

                previousTimer = currentTimer;
            }

            Network.Stop();
        }
    }
}
