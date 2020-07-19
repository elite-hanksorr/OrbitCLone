using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ECS;

namespace OrbitCLone.Components
{
    struct PolarCoordinate : IComponent
    {
        public double Angle;
        public float Radius;

        public PolarCoordinate(double a, float r)
        {
            Angle = a;
            Radius = r;
        }
    }
}
