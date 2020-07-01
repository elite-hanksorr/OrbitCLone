using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

using OrbitLearner;

namespace OrbitCLone
{
    class Agent : Player
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
            VerticalSpeed = 400.0f;
        }

        public Agent AsexuallyReproduce()
        {
            Agent myPreciousOnlyChild = new Agent();
            myPreciousOnlyChild.sprite = sprite;
            myPreciousOnlyChild.AgentBrain = AgentBrain;

            return myPreciousOnlyChild;
        }

        public void Update(List<EnemyPlanet> enemies, BoundingSphere centerLimit, GameTime gt)
        {
            if (Alive)
            {
                if (AgentBrain.FeedFoward(genInputs(enemies))[0] > 0.5f)
                {
                    Radius += (VerticalSpeed * (float)gt.ElapsedGameTime.TotalSeconds);
                }

                position = new Vector2(centerOfAttraction.X + (float)Math.Cos(Angle) * Radius, centerOfAttraction.Y + (float)Math.Sin(Angle) * Radius);
                boundingSphere = new BoundingSphere(new Vector3(position, 0), Size);

                Angle = ((Angle + Speed * (float)gt.ElapsedGameTime.TotalSeconds) % (2 * Math.PI));

                if (Radius >= 430)
                {
                    Radius = 430;
                    Speed = 1.5f;
                }
                else if (Radius > 300)
                {
                    Speed = 2.0f;
                    counter += 2 * (float)gt.ElapsedGameTime.TotalSeconds;
                }
                else if (Radius > 200)
                {
                    Speed = 2.5f;
                    counter += 2.5f * (float)gt.ElapsedGameTime.TotalSeconds;
                }
                else if (Radius > 100)
                {
                    Speed = 3;
                    counter += 3 * (float)gt.ElapsedGameTime.TotalSeconds;
                }

                if (counter > threshold)
                {
                    Score++;
                    counter = 0;
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
        public Brain AgentBrain { get; set; }

        private List<float> genInputs(List<EnemyPlanet> enemies)
        {
            enemies = enemies.OrderBy((enemy) =>
            {
                var angle = (enemy.Angle - Angle) % (2 * Math.PI);
                angle = angle >= 0 ? angle : angle + 2 * Math.PI;
                var radius = Radius - enemy.Radius;
                return Math.Pow(angle / Speed, 2) + Math.Pow(radius / VerticalSpeed, 2);
            })
            .ToList();

            List<float> inputs = new List<float>(26);
            for (int i = 0; i < 26; i++)
            {
                inputs.Add(0.0f);
            }

            for (int i = 0; i < 4; i++)
            {
                if (enemies.Count > i)
                {
                    inputs[6 * i + enemies[i].PlanetId] = 1.0f;
                    inputs[6 * i + 4] = (float)enemies[i].Angle;
                    inputs[6 * i + 5] = enemies[i].Radius;
                }
            }

            inputs[24] = (float)Angle;
            inputs[25] = Radius;

            return inputs;
        }
    }
}
