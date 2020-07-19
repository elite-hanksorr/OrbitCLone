using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using ECS;

namespace OrbitCLone.Components
{
    struct Sprite : IComponent
    {
        public Texture2D Texture;
        
        public Sprite(Texture2D t)
        {
            Texture = t;
        }
    }
}
