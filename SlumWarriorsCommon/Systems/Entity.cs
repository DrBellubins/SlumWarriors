using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SlumWarriorsCommon.Systems
{
    public abstract class Entity : Script
    {
        public Vector2 Position;
        public float Rotation;

        public virtual void Draw(float deltaTime) { }
    }
}
