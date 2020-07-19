using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ECS;

namespace OrbitCLone.Components
{
    struct Position : IComponent
    {
        public float x, y;

        public Position(float in_x, float in_y)
        {
            x = in_x;
            y = in_y;
        }
    }
}
