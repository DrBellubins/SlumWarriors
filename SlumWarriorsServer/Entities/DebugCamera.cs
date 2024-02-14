using Raylib_cs;
using SlumWarriorsCommon.Systems;
using SlumWarriorsCommon.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SlumWarriorsServer.Entities
{
    public class DebugCamera
    {
        public Vector2 Position;
        public Camera2D Camera;
        public float CameraZoom;

        public void Start()
        {
            Camera = new Camera2D();
            Camera.Offset = new Vector2(UI.CenterPivot.X, UI.CenterPivot.Y);
            Camera.Rotation = 0f;
            Camera.Zoom = 100.0f;
        }

        public void Update(float tickDelta)
        {
            Camera.Zoom += Raylib.GetMouseWheelMove();

            if (Raylib.IsMouseButtonDown(MouseButton.Left))
                Position = Raylib.GetScreenToWorld2D(Camera.Offset + -Raylib.GetMouseDelta(), Camera);

            Camera.Target = Position;
        }
    }
}
