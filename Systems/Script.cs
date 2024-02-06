using SlumWarriors.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlumWarriors.Systems
{
    public abstract class Script
    {
        public Script()
        {
            Engine.Scripts.Add(this);
        }

        public virtual void Start(Packet packet) { }
        public virtual void Update(Packet packet, float deltaTime) { }
    }
}
