using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ECS;
using Microsoft.Xna.Framework;
using OrbitCLone.Components;

namespace OrbitCLone.Systems
{
    class ScoreSystem : ComponentSystem
    {
        public float threshold = 3;

        public override void OnUpdate(GameTime gt)
        {
            Entities.ForEach((ref Score s, ref PolarCoordinate p, ref PlayerTag _) =>
            {
                if (p.Radius >= 430)
                { }
                else if (p.Radius > 300)
                    s.counter += 2 * (float)gt.ElapsedGameTime.TotalSeconds;
                else if (p.Radius > 200)
                    s.counter += 2.5f * (float)gt.ElapsedGameTime.TotalSeconds;
                else if (p.Radius > 100)
                    s.counter += 3 * (float)gt.ElapsedGameTime.TotalSeconds;

                if (s.counter > threshold)
                {
                    s.Value++;
                    s.counter = 0;
                }
            });
        }
    }
}
