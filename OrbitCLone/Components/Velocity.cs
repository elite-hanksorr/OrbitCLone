using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ECS;

namespace OrbitCLone.Components
{
    struct Velocity : IComponent
    {
        public float dx, dy;

        public Velocity(float in_dx, float in_dy)
        {
            dx = in_dx;
            dy = in_dy;
        }
    }
}
