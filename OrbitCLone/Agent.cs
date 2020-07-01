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
            AgentBrain = new Brain(new List<int> { 26, 15, 10, 1 });
            Alive = true;
        }

        public void Update(List<EnemyPlanet> enemies, BoundingSphere centerLimit, GameTime gt)
        {
            if (Alive)
            {
                if (AgentBrain.FeedFoward(genInputs(enemies))[0] > 0.5f)
                {
                    Radius += (400 * (float)gt.ElapsedGameTime.TotalSeconds);
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
            List<(float, EnemyPlanet)> distances = new List<(float, EnemyPlanet)>(enemies.Count);

            foreach (var e in enemies)
            {
                double pa = Angle;
                double ea = e.Angle;
                float a;

                if (ea > pa)
                    a = (float)(ea - pa);
                else
                    a = (float)(Math.PI * 2 - pa + ea);

                float r = e.Radius - Radius;

                float d = a * a + r * r;
                distances.Add((d, e));
            }

            distances.Sort(
                    ((float, EnemyPlanet) x, (float, EnemyPlanet) y) => (int)(x.Item1 - y.Item1));

            List<float> inputs = new List<float>(26);
            for (int i = 0; i < 26; i++)
            {
                inputs.Add(0.0f);
            }

            for (int i = 0; i < 4; i++)
            {
                if (distances.Count > i)
                {
                    inputs[6 * i + distances[i].Item2.PlanetId] = 1.0f;
                    inputs[6 * i + 4] = (float)distances[i].Item2.Angle;
                    inputs[6 * i + 5] = distances[i].Item2.Radius;
                }
            }

            inputs[24] = (float)Angle;
            inputs[25] = Radius;

            return inputs;
        }
    }
}
