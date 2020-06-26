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
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        Player player;
        GameEntity blackHole;

        PlanetData tinyPlanetData, smallPlanetData, mediumPlanetData, largePlanetData;

        GameEntity outline;

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

            blackHole = new GameEntity();
            blackHole.position = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
            blackHole.boundingSphere = new BoundingSphere(new Vector3(blackHole.position, 0), 80);

            player = new Player();
            outline = new GameEntity();

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
            player.sprite = Content.Load<Texture2D>("Sprites/OCL_Player");
            outline.sprite = Content.Load<Texture2D>("Sprites/OCL_Outline");

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

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (!gameOver)
            {
                //test code to find nearest planet in front of player
                /*if (smallPlanets.Count != 0)
                {
                    //double min_angle = 2 * Math.PI;
                    float min_value = 30000.0f;
                    Vector2 nearestPlanetPosition = new Vector2();
                    foreach (var planet in smallPlanets)
                    {

                        //if (planet.angle > angle && planet.angle < angle + Math.PI * 2/3)
                        //{
                            //calc distance
                            float distance = (float)Math.Sqrt(
                                (planet.position.X - player.position.X) *
                                (planet.position.X - player.position.X) +
                                (planet.position.Y - player.position.Y) *
                                (planet.position.Y - player.position.Y));
                            if (distance < min_value)
                            {
                                min_value = distance;
                                nearestPlanetPosition = planet.position;
                            }
                        //}
                    }

                    outline.position = nearestPlanetPosition;
                }
                else
                    outline.position = new Vector2();*/

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

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(34, 18, 57));

            spriteBatch.Begin();
            Vector2 textMiddlePoint = font.MeasureString("Score: " + player.Score.ToString());
            Vector2 textPos = new Vector2(graphics.PreferredBackBufferWidth / 2, 50);
            spriteBatch.DrawString(font, "Score: " + player.Score.ToString(), textPos, Color.White, 0, textMiddlePoint, 1.5f, SpriteEffects.None, 0.5f);
            blackHole.Draw(spriteBatch);
            player.Draw(spriteBatch);
            foreach (var planet in enemyPlanets)
                planet.Draw(spriteBatch);
            outline.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
