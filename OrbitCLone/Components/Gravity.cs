using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ECS;

namespace OrbitCLone.Components
{
    struct Gravity : IComponent
    {
        public float Value;

        public Gravity(float v)
        {
            Value = v;
        }
    }
}
