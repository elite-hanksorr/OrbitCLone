using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ECS;

namespace OrbitCLone.Components
{
    struct RotationalSpeed : IComponent
    {
        public float Speed;

        public RotationalSpeed(float s)
        {
            Speed = s;
        }
    }
}
