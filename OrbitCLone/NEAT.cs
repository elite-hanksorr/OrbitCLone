using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitCLone
{
    namespace NEAT
    {
        enum NodeType
        {
            Sensor,
            Hidden,
            Output
        }

        struct ConnectionGene
        {
            public int In { get; set; }
            public int Out { get; set; }
            public float Weight { get; set; }
            public bool Enabled { get; set; }
            public int Innovation { get; set; }
        }

        class Genome
        {
            public List<NodeType> NodeGenes { get; set; }
            public List<ConnectionGene> ConnectionGenes { get; set; }
        }

        // Partially connected neural network.
        class PCNN
        {
            // TODO.
            public PCNN(Genome g)
            {

            }

            public List<float> FeedFoward(List<float> inputs)
            {
                return new List<float>();
            }
        }
        class Agent : Player
        {
            public Genome Genotype { get; set; }
            public PCNN Phenotype { get; set; }
            public float Fitness { get; set; }
            public bool Alive { get; set; }

            public Agent()
            {
                Genotype = new Genome();
                Phenotype = new PCNN(Genotype);
                Fitness = 0;
                Alive = true;

                Radius = 400.0f;
                Angle = 0.0;
                Speed = 2;
                Score = 0;
                Size = 22;
                VerticalSpeed = 400.0f;
            }

            public void Update(List<EnemyPlanet> enemies, BoundingSphere centerLimit, GameTime gt)
            {
                if (Alive)
                {
                    if (Phenotype.FeedFoward(genInputs(enemies))[0] > 0.5f)
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
                    justDied = false;
                }
            }
            public void Init(GameTime gt)
            {
                lastScoreIncrease = gt.TotalGameTime.TotalSeconds;
            }

            public override void Draw(SpriteBatch sb, GameTime gt)
            {
                float alpha = Alive ? 1.0f : Math.Max(0.0f, 1.0f - (float)(gt.TotalGameTime.TotalSeconds - diedAt) / FADE_TIME);
                sb.Draw(sprite, position, null, new Color(alpha, alpha, alpha, alpha), 0f, new Vector2(sprite.Width / 2, sprite.Height / 2), Vector2.One, SpriteEffects.None, 0f);
            }

            protected const float FADE_TIME = 3.0f;
            private double lastScoreIncrease = 0;
            private double diedAt;
            private bool justDied = false;

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

                int numInputs = 2 * numPlanetInputs + 1;
                List<float> inputs = new List<float>(numInputs);
                for (int i = 0; i < numInputs; i++)
                {
                    inputs.Add(0.0f);
                }

                for (int i = 0; i < numPlanetInputs; i++)
                {
                    if (enemies.Count > i)
                    {
                        float angle = (float)((enemies[i].Angle - Angle) / Math.PI * 2);
                        if (angle < 0) angle += 1;
                        //inputs[6 * i + enemies[i].PlanetId] = 1.0f;
                        inputs[2 * i] = angle;
                        inputs[2 * i + 1] = enemies[i].Radius / 430;
                    }
                }

                inputs[inputs.Count() - 1] = Radius / 430;

                return inputs;
            }

            private static int numPlanetInputs = 10;
        }

        class Population
        {
            // Population parameters.
            public int PopulationSize { get; set; }

            // Species parameters.
            public float ExcessCoefficient { get; set; }
            public float DisjointCoefficient { get; set; }
            public float WeightDifferenceCoefficient { get; set; }
            public float Threshold { get; set; }

            // Network parameters.
            public int NumInputs { get; set; }
            public int NumOutputs { get; set; }

            // Hyper parameters.
            public float AddConnectionMutationChance { get; set; }
            public float AddNodeMutationChance { get; set; }
            public float ModifyWeightMutationChance { get; set; }
            public float WeightPerturbationChance { get; set; }
            public float WeightOverrideChance { get; set; }

            public List<Agent> Agents { get; set; }

            // Randomly initialize the population.
            public void Initialize()
            {
                int maxConnections = (NumInputs + 1) * NumOutputs;

                for (int i = 0; i < PopulationSize; i++)
                {
                    int numConnections = rng.Next(0, maxConnections);

                }
            }

            public void UpdateAgents(List<EnemyPlanet> enemies, BoundingSphere centerLimit, GameTime gt)
            {
                foreach (var agent in Agents)
                    agent.Update(enemies, centerLimit, gt);
            }

            public Population()
            {
                Agents = new List<Agent>();
                structuralInnovations = new List<ConnectionGene>();
                innovationNumber = 1;
                rng = new Random();
            }

            

            private List<ConnectionGene> structuralInnovations;
            private int innovationNumber;
            private Random rng;
        }
    }
}
