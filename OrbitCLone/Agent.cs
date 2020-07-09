using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NumSharp;

using OrbitLearner;

namespace OrbitCLone
{
    class Agent : Player
    {
        public Agent()
        {
            Radius = 400.0f;
            //Angle = rng.NextDouble() * Math.PI * 2;
            Angle = 0.0;
            Speed = 2;
            Score = 0;
            Size = 22;
            AgentBrain = new Brain(new int[] { 2 * numPlanetInputs + 1, 10, 1 });
            Alive = true;
            VerticalSpeed = 400.0f;
            Fitness = 0;
            RunningFitness = 0;
        }

        public Agent AsexuallyReproduce()
        {
            Agent myPreciousOnlyChild = new Agent();
            myPreciousOnlyChild.sprite = sprite;
            myPreciousOnlyChild.AgentBrain = AgentBrain.Copy();

            return myPreciousOnlyChild;
        }

        public void Init(GameTime gt)
        {
            lastScoreIncrease = gt.TotalGameTime.TotalSeconds;
            runStartTime = lastScoreIncrease;
        }

        public void Update(List<EnemyPlanet> enemies, BoundingSphere centerLimit, GameTime gt)
        {
            if (Alive)
            {
                if (AgentBrain.Eval(genInputs(enemies))[0] > 0.5f)
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
                    counter += 3 * (float)gt.ElapsedGameTime.TotalSeconds;
                }
                else if (Radius > 100)
                {
                    Speed = 3;
                    counter += 5 * (float)gt.ElapsedGameTime.TotalSeconds;
                }
                
                if (counter > threshold)
                {
                    Score++;
                    counter = 0;
                    lastScoreIncrease = gt.TotalGameTime.TotalSeconds;
                }

                //kill agents if they haven't scored for 5 seconds
                if (gt.TotalGameTime.TotalSeconds - lastScoreIncrease > 5)
                {
                    Alive = false;
                    justDied = true;
                }

                Radius -= 200 * (float)gt.ElapsedGameTime.TotalSeconds;

                foreach (var enemy in enemies)
                    if (boundingSphere.Intersects(enemy.boundingSphere))
                    {
                        Alive = false;
                        justDied = true;
                    }

                if (boundingSphere.Intersects(centerLimit))
                {
                    Alive = false;
                    justDied = true;
                }

                //Fitness = (float)Math.Pow((double)Score + counter / threshold, 2);
                if (Fitness == 0)
                    Fitness = Score + counter / threshold;
                else
                {
                    Fitness = (Score + counter / threshold + Fitness) / 2;
                }
            }
            else if (justDied)
            {
                diedAt = gt.TotalGameTime.TotalSeconds;
                //Fitness /= (float)runTime;
                justDied = false;
            }
        }

        public override void Draw(SpriteBatch sb, GameTime gt) {
            float alpha = Alive ? 1.0f : Math.Max(0.0f, 1.0f - (float) (gt.TotalGameTime.TotalSeconds - diedAt) / FADE_TIME);
            sb.Draw(sprite, position, null, new Color(alpha, alpha, alpha, alpha), 0f, new Vector2(sprite.Width / 2, sprite.Height / 2), Vector2.One, SpriteEffects.None, 0f);
        }

        protected const float FADE_TIME = 3.0f;

        public bool Alive { get; set; }
        public Brain AgentBrain { get; set; }

        public float Fitness { get; set; }
        public float RunningFitness { get; set; }

        private double lastScoreIncrease = 0;
        private double runStartTime;
        private double diedAt;
        private bool justDied = false;

        private NDArray genInputs(List<EnemyPlanet> enemies)
        {
            enemies = enemies.OrderBy((enemy) =>
            {
                var angle = (enemy.Angle - Angle) % (2 * Math.PI);
                angle = angle >= 0 ? angle : angle + 2 * Math.PI;
                var radius = Radius - enemy.Radius;
                return Math.Pow(angle / Speed, 2) + Math.Pow(radius / VerticalSpeed, 2);
            })
            .ToList();

            var inputs = np.zeros((2, numPlanetInputs));

            for (int i = 0; i < Math.Min(enemies.Count, numPlanetInputs); i++)
            {
                float angle = (float)((enemies[i].Angle - Angle) / Math.PI * 2);
                if (angle < 0) angle += 1;
                // inputs[i, enemies[i].PlanetId] = 1.0f;
                inputs[i, 0] = angle;
                inputs[i, 1] = enemies[i].Radius / 430.0f;
            }

            inputs = np.concatenate((inputs.flatten(), Radius / 430.0f));

            return inputs;
        }

        private static int numPlanetInputs = 10;
        private static Random rng = new Random();
    }
}
