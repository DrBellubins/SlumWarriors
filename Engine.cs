using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Raylib_cs;
using System.Numerics;
using SlumWarriors.Systems;
using SlumWarriors.Entities;
using SlumWarriors.Utils;
using SlumWarriors.Networking;

// TODO: Figure out how to pass packet data to scripts/entities

namespace SlumWarriors
{
    internal class Engine
    {
        public const int MaxFPS = 60;
        public const int ScreenWidth = 1600;
        public const int ScreenHeight = 900;

        public static bool IsServer;
        public static Font MainFont;
        public static Texture2D DebugTexture;

        public static List<Script> Scripts = new List<Script>();
        public static List<Player> Players = new List<Player>();

        public static Server? server;
        public static Client? client;

        public void Initialize()
        {
            // Setup
            Raylib.InitWindow(ScreenWidth, ScreenHeight, "Slum Warriors Dev");
            Raylib.SetExitKey(KeyboardKey.Q);
            Raylib.SetTargetFPS(MaxFPS);

            MainFont = Raylib.LoadFontEx("Assets/Fonts/VarelaRound-Regular.ttf", 64, null, 250);
            DebugTexture = Raylib.LoadTexture("Assets/Textures/DebugTexture.png");

            var previousTimer = DateTime.Now;
            var currentTimer = DateTime.Now;

            var time = 0.0f;
            var deltaTime = 0.0f;

            if (IsServer)
                server = new Server();
            else
                client = new Client();

            var testPlayer = new Player();

            // Start
            var testConnectPacket = new Packet(PacketBufferTypes.Connection, "TestPlayer");

            foreach (var script in Scripts)
                script.Start(testConnectPacket);

            while (!Raylib.WindowShouldClose())
            {
                currentTimer = DateTime.Now;

                // Update
                deltaTime = (currentTimer.Ticks - previousTimer.Ticks) / 10000000f;
                time += deltaTime;

                var testRuntimePacket = new Packet(PacketBufferTypes.Runtime);

                foreach (var script in Scripts)
                    script.Update(testRuntimePacket, deltaTime);

                // Draw
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                Raylib.BeginMode2D(testPlayer.Camera);

                Raylib.DrawTexture(DebugTexture, 0, 0, Color.White);

                foreach (var script in Scripts)
                {
                    if (script.GetType().IsSubclassOf(typeof(Script)))
                    {
                        var ent = (Entity)script;
                        ent.Draw(deltaTime);
                    }
                }

                Raylib.EndMode2D();
                Raylib.EndDrawing();

                previousTimer = currentTimer;
            }

            Raylib.CloseWindow();
        }
    }
}
