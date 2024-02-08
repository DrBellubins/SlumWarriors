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

namespace SlumWarriorsClient.Entities
{
    public class Player : Entity
    {
        public string Username = "TEST_PLAYER_BIG_BOB_BOBS"; // Must always be 24 chars long!
        public Vector2 MovementVec = new Vector2();

        public Camera2D Camera;
        public float CameraZoom;

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

            isMoving = IsKeyPressed(KeyboardKey.W) || IsKeyPressed(KeyboardKey.S)
                || IsKeyPressed(KeyboardKey.D) || IsKeyPressed(KeyboardKey.A);

            // Update movement
            if (IsKeyPressed(KeyboardKey.W))
                MovementVec.Y = -1f;

            if (IsKeyPressed(KeyboardKey.S))
                MovementVec.Y = 1f;

            if (IsKeyPressed(KeyboardKey.D))
                MovementVec.X = 1f;

            if (IsKeyPressed(KeyboardKey.A))
                MovementVec.X = -1f;

            // Send movement
            if (isMoving && Engine.Server != null)
            {
                Network.SendVector2(Engine.Server, MovementVec, "mu");
                MovementVec = Vector2.Zero;
            }

            Camera.Target = Vector2.Lerp(Camera.Target, Position, 3.5f * deltaTime);
        }

        public override void Draw(float deltaTime)
        {
            DrawCircleV(Position, 0.5f, Color.Green);
        }
    }
}
