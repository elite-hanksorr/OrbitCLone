using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

using OrbitLearner;

namespace OrbitCLone
{
    class Agent : Planet
    {
        public Agent()
        {
            Radius = 400.0f;
            Angle = 0.0f;
            Speed = 2;
            Score = 0;
            Size = 22;
            AgentBrain = new Brain(new List<int> { 5, 3, 1 });
            Alive = true;
        }

        public void Update(List<EnemyPlanet> enemies, BoundingSphere centerLimit, GameTime gt)
        {
            if (Alive)
            {
                if (AgentBrain.FeedFoward(genInputs(enemies))[0] > 0.5f)
                {
                    Radius += 7;
                }

                position = new Vector2(centerOfAttraction.X + (float)Math.Cos(Angle) * Radius, centerOfAttraction.Y + (float)Math.Sin(Angle) * Radius);
                boundingSphere = new BoundingSphere(new Vector3(position, 0), Size);

                double previousAngle = Angle;
                Angle = ((Angle + Speed * (float)gt.ElapsedGameTime.TotalSeconds) % (2 * Math.PI));
                if (Angle < previousAngle)
                    Score++;

                if (Radius > 430)
                {
                    Radius = 430;
                    Speed = 2.0f;
                }
                else if (Radius > 300)
                {
                    Speed = 2.0f;
                }
                else if (Radius > 200)
                {
                    Speed = 2.5f;
                }
                else if (Radius > 100)
                {
                    Speed = 3;
                }

                Radius -= 200 * (float)gt.ElapsedGameTime.TotalSeconds;

                foreach (var enemy in enemies)
                    if (boundingSphere.Intersects(enemy.boundingSphere))
                        Alive = false;

                if (boundingSphere.Intersects(centerLimit))
                    Alive = false;
            }
        }

        public bool Alive { get; set; }
        public int Score { get; set; }
        public Brain AgentBrain { get; set; }

        private List<float> genInputs(List<EnemyPlanet> enemies)
        {
            //generate the inputs for the brain based on the position of enemies
            var r = new Random(new Random().Next(0, 10000));
            return new List<float> { (float)r.NextDouble() * 4 - 2, (float)r.NextDouble() * 4 - 2, (float)r.NextDouble() * 4 - 2, (float)r.NextDouble() * 4 - 2, (float)r.NextDouble() * 4 - 2};
        }
    }
}
