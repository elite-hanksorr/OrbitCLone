using ECS;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitCLone.Components
{
    struct CircleCollider : IComponent
    {
        public int Radius;
        public BoundingSphere Collider; 

        public CircleCollider(int radius)
        {
            Radius = radius;
            Collider = new BoundingSphere();
        }
    }
}
