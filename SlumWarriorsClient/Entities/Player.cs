using SlumWarriorsCommon.Systems;
using SlumWarriorsCommon.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Raylib_cs;

using SlumWarriorsClient.Utils;

using static Raylib_cs.Raylib;
using SlumWarriorsCommon.Gameplay;

namespace SlumWarriorsClient.Entities
{
    public class Player : Entity
    {
        public string Username = "TEST_PLAYER_BIG_BOB_BOBS"; // Must always be 24 chars long!
        public Vector2 MovementVec = new Vector2();

        public Camera2D Camera;
        public float CameraZoom;

        MoveDirection movement = new MoveDirection();
        private bool isMoving = false;

        public override void Start()
        {
            Camera = new Camera2D();
            Camera.Target = new Vector2(Position.X, Position.Y);
            Camera.Offset = new Vector2(UI.CenterPivot.X, UI.CenterPivot.Y);
            //Camera.Rotation = 180.0f; // Flip camera so that north is +Y
            Camera.Rotation = 0f;
            Camera.Zoom = 100.0f;
        }

        public override void Update(float deltaTime)
        {
            Camera.Zoom += GetMouseWheelMove();

            // Update position
            if (Engine.Server != null)
            {
                var rec = Network.Receive(Engine.Server, "pu");

                if (rec != null && rec.Reader != null) // received pu
                    Position = new Vector2(rec.Reader.GetFloat(), rec.Reader.GetFloat());
            }

            isMoving = IsKeyDown(KeyboardKey.W) || IsKeyDown(KeyboardKey.S)
                || IsKeyDown(KeyboardKey.D) || IsKeyDown(KeyboardKey.A);

            // Update movement
            if (IsKeyDown(KeyboardKey.W))
                movement = MoveDirection.Up;

            if (IsKeyDown(KeyboardKey.S))
                movement = MoveDirection.Down;

            if (IsKeyDown(KeyboardKey.D))
                movement = MoveDirection.Right;

            if (IsKeyDown(KeyboardKey.A))
                movement = MoveDirection.Left;

            // Send movement
            if (isMoving && Engine.Server != null)
            {
                Network.SendInt(Engine.Server, (int)movement, "mu");
                movement = MoveDirection.NONE;
            }

            Camera.Target = Vector2.Lerp(Camera.Target, Position, 3.5f * deltaTime);
        }

        public override void Draw(float deltaTime)
        {
            DrawCircleV(Position, 0.5f, Color.Green);
        }
    }
}
