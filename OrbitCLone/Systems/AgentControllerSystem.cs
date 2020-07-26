using ECS;
using Microsoft.Xna.Framework;
using OrbitCLone.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OrbitCLone.Systems
{
    class AgentControllerSystem : ComponentSystem
    {
        public int NumPlanetInputs;

        public override void OnUpdate(GameTime gt)
        {
            // Get enemy planet positions.
            List<PolarCoordinate> enemy_positions = new List<PolarCoordinate>();
            Entities.ForEach((ref PolarCoordinate c, ref EnemyTag _) =>
            {
                enemy_positions.Add(c);
            });

            // Adjust agent radius based on nn output.
            Entities.ForEach((ref Pcnn nn, ref PolarCoordinate c, ref Gravity g, ref RotationalSpeed s) =>
            {
                // Calculate inputs to nn based on enemy positions.
                List<PolarCoordinate> enemyPositions = new List<PolarCoordinate>();
                Entities.ForEach((ref PolarCoordinate ec, ref EnemyTag _) =>
                {
                    enemyPositions.Add(ec);
                });

                PolarCoordinate a_c = c;
                Gravity a_g = g;
                RotationalSpeed a_s = s;
                enemyPositions = enemyPositions.OrderBy((e) =>
                {
                    var angle = (e.Angle - a_c.Angle) % (2 * Math.PI);
                    angle = angle >= 0 ? angle : angle + 2 * Math.PI;
                    var radius = a_c.Radius - e.Radius;
                    return Math.Pow(angle / a_s.Speed, 2) + Math.Pow(radius / a_g.Value, 2);
                }).ToList();

                int numInputs = 2 * NumPlanetInputs + 2;
                List<float> inputs = new List<float>(numInputs);
                for (int i = 0; i < numInputs; i++) inputs.Add(0.0f);

                for (int i = 0; i < NumPlanetInputs; i++)
                {
                    if (enemyPositions.Count > i)
                    {
                        float angle = (float)((enemyPositions[i].Angle - a_c.Angle) / (Math.PI * 2));
                        if (angle < 0) angle += 1;
                        inputs[2 * i] = angle;
                        inputs[2 * i + 1] = enemyPositions[i].Radius / 430;
                    }
                }

                inputs[inputs.Count - 2] = a_c.Radius / 430;
                inputs[inputs.Count - 1] = 1; // Bias neuron.                

                // Feed foward the nn based on the inputs.
                var outputs = nn.Evaluate(inputs);

                if (outputs[0] >= 0.5f)
                    c.Radius += 400.0f * (float)gt.ElapsedGameTime.TotalSeconds;
            });
        }
    }
}
