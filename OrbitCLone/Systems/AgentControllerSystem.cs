using ECS;
using Microsoft.Xna.Framework;
using OrbitCLone.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitCLone.Systems
{
    class AgentControllerSystem : ComponentSystem
    {
        public override void OnUpdate(GameTime gt)
        {
            // Get enemy planet positions.
            List<PolarCoordinate> enemy_positions = new List<PolarCoordinate>();
            Entities.ForEach((ref PolarCoordinate c, ref EnemyTag _) =>
            {
                enemy_positions.Add(c);
            });

            // Adjust agent radius based on nn output.
            Entities.ForEach((ref Pcnn nn, ref PolarCoordinate c) =>
            {
                // Calculate inputs to nn based on enemy positions.

                // Feed foward the nn based on the inputs.

                // Temp test code.
                var choice = r.Next(0, 100);
                if (choice > 50)
                    c.Radius += 400.0f * (float)gt.ElapsedGameTime.TotalSeconds;
            });

        }

        private Random r = new Random();
    }
}
