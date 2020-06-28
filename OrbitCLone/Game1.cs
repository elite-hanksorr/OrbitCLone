using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;

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

        GameState state = GameState.TrainMode;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        Player player;

        List<Agent> agents;
        Texture2D agentSprite;
        GameEntity blackHole;

        PlanetData tinyPlanetData, smallPlanetData, mediumPlanetData, largePlanetData;

        //GameEntity outline;

        List<EnemyPlanet> enemyPlanets;

        int elapsedTime = 0;
        bool gameOver = false;

        Random rng;

        Brain brain;

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

            if (state == GameState.PlayMode) player = new Player();
            //outline = new GameEntity();
            if (state == GameState.TrainMode)
            {
                agents = new List<Agent>(100);
                for (int i = 0; i < 100; i++)
                {
                    agents.Add(new Agent());
                }
            }

            rng = new Random();

            enemyPlanets = new List<EnemyPlanet>();

            tinyPlanetData = new PlanetData(0.2f, 15);
            smallPlanetData = new PlanetData(0.15f, 20);
            mediumPlanetData = new PlanetData(0.10f, 23);
            largePlanetData = new PlanetData(0.07f, 28);

            brain = new Brain(new List<int> { 3, 2, 1 });

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            blackHole.sprite = Content.Load<Texture2D>("Sprites/OCL_BlackHole");

            if (state == GameState.PlayMode)
                player.sprite = Content.Load<Texture2D>("Sprites/OCL_Player");

            if (state == GameState.TrainMode)
            {
                agentSprite = Content.Load<Texture2D>("Sprites/OCL_Player");
                foreach (var agent in agents)
                {
                    agent.sprite = agentSprite;
                }
            }
            //outline.sprite = Content.Load<Texture2D>("Sprites/OCL_Outline");

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
                            if (r + highScore > 70)
                                enemyPlanets.Add(new EnemyPlanet(smallPlanetData));

                            if ((elapsedTime % 2) == 0)
                            {
                                r = rng.Next(0, 100);
                                if (r + highScore > 80)
                                    enemyPlanets.Add(new EnemyPlanet(mediumPlanetData));
                            }

                            if ((elapsedTime % 3) == 0)
                            {
                                r = rng.Next(0, 100);
                                if (r + highScore > 90)
                                    enemyPlanets.Add(new EnemyPlanet(largePlanetData));
                            }

                            if ((elapsedTime % 5) == 0)
                            {
                                r = rng.Next(0, 100);
                                if (r + highScore > 95)
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
            blackHole.Draw(spriteBatch);
            foreach (var planet in enemyPlanets)
                planet.Draw(spriteBatch);

            switch (state)
            {
                case GameState.PlayMode:
                {
                    Vector2 textMiddlePoint = font.MeasureString("Score: " + player.Score.ToString());
                    Vector2 textPos = new Vector2(graphics.PreferredBackBufferWidth / 2, 50);
                    spriteBatch.DrawString(font, "Score: " + player.Score.ToString(), textPos, Color.White, 0, textMiddlePoint, 1.5f, SpriteEffects.None, 0.5f);
                    player.Draw(spriteBatch);
                    break;
                }
                case GameState.TrainMode:
                {
                    int highScore = FindHighestScore();
                    Vector2 textMiddlePoint = font.MeasureString("Score: " + highScore.ToString());
                    Vector2 textPos = new Vector2(graphics.PreferredBackBufferWidth / 2, 50);
                    spriteBatch.DrawString(font, "Score: " + highScore.ToString(), textPos, Color.White, 0, textMiddlePoint, 1.5f, SpriteEffects.None, 0.5f);
                    foreach (var agent in agents)
                        agent.Draw(spriteBatch);
                    break;
                }
                default:
                    break;
            }
            //outline.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
