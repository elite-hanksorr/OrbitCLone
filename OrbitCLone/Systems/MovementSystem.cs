using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ECS;
using Microsoft.Xna.Framework;
using OrbitCLone.Components;

namespace OrbitCLone.Systems
{
    class MovementSystem : ComponentSystem
    {
        public override void OnUpdate(GameTime gt)
        {
            // Move enemy planets.
            Entities.ForEach((ref Position p, ref Velocity v) =>
            {
                p.x += v.dx * (float)gt.ElapsedGameTime.TotalSeconds;
                p.y += v.dy * (float)gt.ElapsedGameTime.TotalSeconds;
            });

            // Move players.
            Entities.ForEach((ref Position p, ref PolarCoordinate pc, ref RotationalSpeed s, ref Gravity g) =>
            {
                pc.Angle = (pc.Angle + s.Speed * (float)gt.ElapsedGameTime.TotalSeconds) % (2 * Math.PI);
                pc.Radius -= g.Value * (float)gt.ElapsedGameTime.TotalSeconds;
                if (pc.Radius >= 430)
                {
                    pc.Radius = 430;
                    s.Speed = 1.5f;
                }
                else if (pc.Radius > 300)
                {
                    s.Speed = 2.0f;
                }
                else if (pc.Radius > 200)
                {
                    s.Speed = 2.5f;
                }
                else if (pc.Radius > 100)
                {
                    s.Speed = 3;
                }
                p.x = GameConfig.SCREEN_WIDTH / 2 + (float)Math.Cos(pc.Angle) * pc.Radius;
                p.y = GameConfig.SCREEN_HEIGHT / 2 + (float)Math.Sin(pc.Angle) * pc.Radius;
            });
        }
    }
}
