﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Raylib_cs;
using System.Numerics;
using SlumWarriorsCommon.Systems;
using SlumWarriorsCommon.Networking;
using SlumWarriorsClient.Entities;
using SlumWarriorsClient.Utils;
using LiteNetLib;
using SlumWarriorsClient.Terrain;
using SlumWarriorsCommon.Terrain;

// TODO: Figure out how to pass packet data to scripts/entities
namespace SlumWarriorsClient
{
    internal class Engine
    {
        public const int MaxFPS = 60;
        public const int ScreenWidth = 1600;
        public const int ScreenHeight = 900;

        public static bool IsRunning;
        public static Font MainFont;
        public static Texture2D DebugTexture;

        public static Player CurrentPlayer = new Player();
        public static NetPeer? Server;

        public void Initialize()
        {
            // Setup
            IsRunning = true;

            Raylib.InitWindow(ScreenWidth, ScreenHeight, "Slum Warriors Dev");
            Raylib.SetExitKey(KeyboardKey.Null);
            Raylib.SetTargetFPS(MaxFPS);

            MainFont = Raylib.LoadFontEx("Assets/Fonts/VarelaRound-Regular.ttf", 64, null, 250);
            DebugTexture = Raylib.LoadTexture("Assets/Textures/Blocks/error.png");

            Block.InitializeBlockPrefabs(false);

            var previousTimer = DateTime.Now;
            var currentTimer = DateTime.Now;

            var time = 0.0f;
            var deltaTime = 0.0f;

            // Start
            Network.Start(false);

            Network.Listener.PeerConnectedEvent += peer =>
            {
                Server = peer;
                CurrentPlayer.Start();
            };

            foreach (var script in Script.Scripts)
                script.Start();

            var world = new World();

            while (IsRunning)
            {
                if (Raylib.IsKeyPressed(KeyboardKey.Q))
                    IsRunning = false;

                currentTimer = DateTime.Now;

                // Update
                deltaTime = (currentTimer.Ticks - previousTimer.Ticks) / 10000000f;
                time += deltaTime;

                Network.Poll(); // Must update first

                world.Update(deltaTime);

                foreach (var script in Script.Scripts)
                    script.Update(deltaTime);

                Network.Update();

                // Draw
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                Raylib.BeginMode2D(CurrentPlayer.Camera);

                world.Draw(deltaTime);

                foreach (var script in Script.Scripts)
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

            Network.Stop();

            Raylib.CloseWindow();
            Environment.Exit(0);
        }

        public void Close()
        {
            IsRunning = false;
        }
    }
}