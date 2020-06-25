using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace OrbitCLone
{
    class Planet : GameEntity
    {
        public float Radius { get; set; }
        public double Angle { get; set; }
        public float Speed { get; set; }

        public Planet()
        {
            centerOfAttraction = new Vector2(1920 / 2, 1080 / 2);
        }

        protected Vector2 centerOfAttraction;
    }
}
