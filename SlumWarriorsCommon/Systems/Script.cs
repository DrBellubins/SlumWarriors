using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlumWarriorsCommon.Systems
{
    public abstract class Script
    {
        public static List<Script> Scripts = new List<Script>();

        public Script()
        {
            Scripts.Add(this);
        }

        public virtual void Start() { }
        public virtual void Update(float deltaTime) { }
    }
}
