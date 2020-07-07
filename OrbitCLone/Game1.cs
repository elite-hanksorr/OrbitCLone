using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using OrbitLearner;

namespace OrbitCLone
{
    public class Game1 : Game
    {
        enum GameState
        {
            Menu,
            PlayMode,
            TrainMode,
            NewGame
        }

        static float[] reportPercentiles = new float[]{25, 50, 75, 90, 95, 97.5f, 99, 100};

        GameState state = GameState.TrainMode;
        const int populationSize = 1000;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        Player player;

        List<Agent> agents;
        Texture2D agentSprite;
        Texture2D bestAgentSprite;
        GameEntity blackHole;

        StreamWriter trainLog;
        SortedDictionary<int, int> agentScoresDict = new SortedDictionary<int, int>();

        PlanetData tinyPlanetData, smallPlanetData, mediumPlanetData, largePlanetData;

        GameEntity outline;

        List<EnemyPlanet> enemyPlanets;

        int elapsedTime = 0;
        int generation = 1;
        bool gameOver = false;

        Random rng;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.ApplyChanges();

            blackHole = new GameEntity
            {
                position = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2)
            };
            blackHole.boundingSphere = new BoundingSphere(new Vector3(blackHole.position, 0), 80);

            if (state == GameState.PlayMode)
            {
                player = new Player();
                outline = new GameEntity();
            }
            if (state == GameState.TrainMode)
            {
                trainLog = new StreamWriter(@"train.csv", append: false, encoding: Encoding.UTF8);
                trainLog.Write("Generation");
                foreach (var percentile in reportPercentiles)
                    trainLog.Write($",{percentile,5:f1}%");

                trainLog.WriteLine();
                trainLog.Flush();

                agents = new List<Agent>(populationSize);
                for (int i = 0; i < populationSize; i++)
                {
                    agents.Add(new Agent());
                }

                agentScoresDict.Clear();
            }

            rng = new Random();

            enemyPlanets = new List<EnemyPlanet>();

            tinyPlanetData = new PlanetData(0.2f, 15, 0);
            smallPlanetData = new PlanetData(0.15f, 20, 1);
            mediumPlanetData = new PlanetData(0.10f, 23, 2);
            largePlanetData = new PlanetData(0.07f, 28, 3);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            blackHole.sprite = Content.Load<Texture2D>("Sprites/OCL_BlackHole");

            if (state == GameState.PlayMode)
            {
                player.sprite = Content.Load<Texture2D>("Sprites/OCL_Player");
                outline.sprite = Content.Load<Texture2D>("Sprites/OCL_Outline");
            }

            if (state == GameState.TrainMode)
            {
                agentSprite = Content.Load<Texture2D>("Sprites/OCL_Player");
                bestAgentSprite = Content.Load<Texture2D>("Sprites/OCL_BestPlayer");
                foreach (var agent in agents)
                {
                    agent.sprite = agentSprite;
                }
            }

            tinyPlanetData.Texture = Content.Load<Texture2D>("Sprites/OCL_TinyPlanet");
            smallPlanetData.Texture = Content.Load<Texture2D>("Sprites/OCL_SmallPlanet");
            mediumPlanetData.Texture = Content.Load<Texture2D>("Sprites/OCL_MediumPlanet");
            largePlanetData.Texture = Content.Load<Texture2D>("Sprites/OCL_LargePlanet");

            font = Content.Load<SpriteFont>("Fonts/myFont");
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private int FindHighestScore()
        {
            int maxScore = 0;
            foreach (var agent in agents)
            {
                if (agent.Score > maxScore)
                {
                    maxScore = agent.Score;
                }
            }

            return maxScore;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (state)
            {
                case GameState.PlayMode:
                    if (!gameOver)
                    {
                        //Spawn enemy planets.
                        if (gameTime.TotalGameTime.TotalSeconds > elapsedTime)
                        {
                            elapsedTime++;
                            int r = rng.Next(0, 100);
                            if (r + player.Score > 70)
                                enemyPlanets.Add(new EnemyPlanet(smallPlanetData));

                            if ((elapsedTime % 2) == 0)
                            {
                                r = rng.Next(0, 100);
                                if (r + player.Score > 80)
                                    enemyPlanets.Add(new EnemyPlanet(mediumPlanetData));
                            }

                            if ((elapsedTime % 3) == 0)
                            {
                                r = rng.Next(0, 100);
                                if (r + player.Score > 90)
                                    enemyPlanets.Add(new EnemyPlanet(largePlanetData));
                            }

                            if ((elapsedTime % 5) == 0)
                            {
                                r = rng.Next(0, 100);
                                if (r + player.Score > 95)
                                    enemyPlanets.Add(new EnemyPlanet(tinyPlanetData));
                            }
                        }

                        player.Update(gameTime);

                        for (int i = 0; i < enemyPlanets.Count; i++)
                        {
                            var planet = enemyPlanets[i];
                            planet.Update(gameTime);
                            if (player.boundingSphere.Intersects(planet.boundingSphere))
                                gameOver = true;
                            if (blackHole.boundingSphere.Contains(planet.boundingSphere) == ContainmentType.Contains)
                            {
                                enemyPlanets.Remove(planet);
                                i--;
                            }
                        }

                        outline.position = enemyPlanets
                            .OrderBy((enemy) => {
                                    var angle = (enemy.Angle - player.Angle) % (2 * Math.PI);
                                    angle = angle >= 0 ? angle : angle + 2 * Math.PI;
                                    var radius = player.Radius - enemy.Radius;
                                    return Math.Pow(angle / player.Speed, 2) + Math.Pow(radius / player.VerticalSpeed, 2);
                                })
                            .Select((e) => e.position)
                            .DefaultIfEmpty(new Vector2(0, 0))
                            .First();

                        if (player.boundingSphere.Intersects(blackHole.boundingSphere))
                            gameOver = true;
                    }
                    break;
                case GameState.TrainMode:
                    if (!gameOver)
                    {
                        //Spawn enemy planets.
                        if (gameTime.TotalGameTime.TotalSeconds > elapsedTime)
                        {
                            elapsedTime++;
                            int highScore = FindHighestScore();
                            int r = rng.Next(0, 100);
                            if (r + 2 * highScore > 70)
                                enemyPlanets.Add(new EnemyPlanet(smallPlanetData));

                            if ((elapsedTime % 2) == 0)
                            {
                                r = rng.Next(0, 100);
                                if (r + 2 * highScore > 80)
                                    enemyPlanets.Add(new EnemyPlanet(mediumPlanetData));
                            }

                            if ((elapsedTime % 3) == 0)
                            {
                                r = rng.Next(0, 100);
                                if (r + 2 * highScore > 90)
                                    enemyPlanets.Add(new EnemyPlanet(largePlanetData));
                            }

                            if ((elapsedTime % 5) == 0)
                            {
                                r = rng.Next(0, 100);
                                if (r + 2 * highScore > 95)
                                    enemyPlanets.Add(new EnemyPlanet(tinyPlanetData));
                            }
                        }

                        for (int i = 0; i < enemyPlanets.Count; i++)
                        {
                            var planet = enemyPlanets[i];
                            planet.Update(gameTime);
                            if (blackHole.boundingSphere.Contains(planet.boundingSphere) == ContainmentType.Contains)
                            {
                                enemyPlanets.Remove(planet);
                                i--;
                            }
                        }

                        gameOver = true;
                        foreach (var agent in agents)
                        {
                            agent.Update(enemyPlanets, blackHole.boundingSphere, gameTime);
                            gameOver = gameOver && !agent.Alive;
                        }

                    }
                    else
                    {
                        //prepare new run
                        enemyPlanets.Clear();
                        enemyPlanets.Add(new EnemyPlanet(smallPlanetData));

                        //log agent scores
                        {
                            var agentScores = agents
                                .Select(agent => agent.Fitness)
                                .OrderBy(x => x)
                                .ToList();

                            trainLog.Write($"{generation,10}");

                            foreach (float percentile in reportPercentiles) {
                                var percentileScore = agentScores[Math.Min(agentScores.Count() - 1, (int) (agentScores.Count() * percentile / 100.0))];
                                trainLog.Write($",{percentileScore,6:f2}");
                            }

                            trainLog.WriteLine();
                            trainLog.Flush();
                        }

                        //sort agents by fitness
                        agents.Sort((Agent a1, Agent a2) => (int)Math.Floor(a1.Fitness - a2.Fitness));
                        agents.Reverse();

                        //calculate fitness sum
                        float fitnessSum = 0;
                        foreach (var agent in agents) fitnessSum += agent.Fitness * agent.Fitness;

                        //prepare next generation
                        List<Agent> newAgents = new List<Agent>(agents.Count);

                        //put best 25% straight into next gen
                        for (int i = 0; i < populationSize / 4; i++)
                        {
                            var newAgent = agents[i].AsexuallyReproduce();
                            newAgent.Init(gameTime);
                            newAgents.Add(newAgent);
                        }

                        //make 10% completely new agents
                        for (int i = populationSize - 1; i >= populationSize - populationSize / 10; i--)
                        {
                            var newAgent = new Agent();
                            newAgent.Init(gameTime);
                            newAgent.sprite = agentSprite;
                            newAgents.Add(newAgent);
                        }

                        Agent selectParent()
                        {
                            float choice = (float)rng.NextDouble() * fitnessSum;
                            float runningSum = 0;
                            foreach (var agent in agents)
                            {
                                runningSum += agent.Fitness * agent.Fitness;
                                if (runningSum > choice)
                                    return agent;
                            }

                            return null;
                        }

                        for (int i = 0; i < populationSize - populationSize / 4 - populationSize / 10; i++)
                        {
                            var parent = selectParent();
                            
                            var baby = parent.AsexuallyReproduce();
                            baby.AgentBrain.Mutate(1.0f);
                            baby.Init(gameTime);
                            newAgents.Add(baby);
                        }

                        agents = newAgents;
                        generation++;
                        gameOver = false;
                    }
                    break;
                default:
                    break;
            }


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(34, 18, 57));

            spriteBatch.Begin();
            blackHole.Draw(spriteBatch, gameTime);
            foreach (var planet in enemyPlanets)
                planet.Draw(spriteBatch, gameTime);

            switch (state)
            {
                case GameState.PlayMode:
                {
                    Vector2 textMiddlePoint = font.MeasureString("Score: " + player.Score.ToString());
                    Vector2 textPos = new Vector2(graphics.PreferredBackBufferWidth / 2, 50);
                    spriteBatch.DrawString(font, "Score: " + player.Score.ToString(), textPos, Color.White, 0, textMiddlePoint, 1.5f, SpriteEffects.None, 0.5f);
                    player.Draw(spriteBatch, gameTime);
                    outline.Draw(spriteBatch, gameTime);
                    break;
                }
                case GameState.TrainMode:
                {
                    int highScore = FindHighestScore();
                    Vector2 topTextMiddlePoint = font.MeasureString("Score: " + highScore.ToString());
                    Vector2 textPos = new Vector2(graphics.PreferredBackBufferWidth / 2, 50);
                    spriteBatch.DrawString(font, "Score: " + highScore.ToString(), textPos, Color.White, 0, topTextMiddlePoint, 1.5f, SpriteEffects.None, 0.5f);

                    int alive = agents.Where((agent) => agent.Alive).Count();
                    string s = $"generation: {generation}\nalive: {alive}";
                    Vector2 bottomTextMiddlePoint = font.MeasureString(s) / 2;
                    textPos = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight - 20);
                    spriteBatch.DrawString(font, s, textPos, Color.White, 0, bottomTextMiddlePoint, 1.5f, SpriteEffects.None, 0.5f);
                    for (int i = agents.Count - 1; i > 0; i--)
                        agents[i].Draw(spriteBatch, gameTime);
                    break;
                }
                default:
                    break;
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
