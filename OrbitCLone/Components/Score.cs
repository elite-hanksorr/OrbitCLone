using ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitCLone.Components
{
    struct Score :  IComponent
    {
        public Score(int value)
        {
            Value = value;
            counter = 0;
        }

        public int Value;
        public float counter;
    }
}
