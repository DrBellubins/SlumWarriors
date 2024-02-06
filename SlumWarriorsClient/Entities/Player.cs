using SlumWarriorsCommon.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Raylib_cs;

using static Raylib_cs.Raylib;
using SlumWarriorsClient.Utils;
using SlumWarriorsClient.Networking;

namespace SlumWarriorsClient.Entities
{
    public class Player : Entity
    {
        public string Username = "";

        public Camera2D Camera;
        public float CameraZoom;

        public override void Start()
        {
            Camera = new Camera2D();
            Camera.Target = new Vector2(Position.X, Position.Y);
            Camera.Offset = new Vector2(UI.CenterPivot.X, UI.CenterPivot.Y);
            Camera.Rotation = 180.0f; // Flip camera so that north is +Y
            Camera.Zoom = 100.0f;
        }

        public override void Update(float deltaTime)
        {
            Camera.Zoom += GetMouseWheelMove();

            if (IsKeyPressed(KeyboardKey.W))
                Position.Y += 1f;

            if (IsKeyPressed(KeyboardKey.S))
                Position.Y -= 1f;

            if (IsKeyPressed(KeyboardKey.D))
                Position.X -= 1f;

            if (IsKeyPressed(KeyboardKey.A))
                Position.X += 1f;

            Camera.Target = Vector2.Lerp(Camera.Target, Position, 3.5f * deltaTime);
        }

        public override void Draw(float deltaTime)
        {
            Raylib.DrawCircleV(Position, 0.5f, Color.Green);
        }
    }
}
