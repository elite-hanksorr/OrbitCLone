using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OrbitCLone.Components;

namespace OrbitCLone.Systems
{
    class PlayerControllerSystem : ComponentSystem
    {
        public override void OnUpdate(GameTime gt)
        {
            Entities.ForEach((ref PlayerTag pt, ref Input i, ref PolarCoordinate p) =>
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    p.Radius += 400.0f * (float)gt.ElapsedGameTime.TotalSeconds;
                }
            });
        }
    }
}
