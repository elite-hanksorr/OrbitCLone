using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;

using OrbitLearner;

namespace OrbitCLone
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        GameEntity player;
        GameEntity blackHole;

        Texture2D tinyPlanetTexture;
        Texture2D smallPlanetTexture;
        Texture2D mediumPlanetTexture;
        Texture2D largePlanetTexture;

        Queue<TinyPlanet> tinyPlanets;
        Queue<SmallPlanet> smallPlanets;
        Queue<MediumPlanet> mediumPlanets;
        Queue<LargePlanet> largePlanets;

        double angle;
        float radius;
        float speed;
        int score;

        bool spawnTinyPlanet = false;
        bool spawnSmallPlanet = true;
        bool spawnMediumPlanet = false;
        bool spawnLargePlanet = false;

        int elapsedTime = 0;
        bool gameOver = false;

        Random rng;

        Brain brain;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.ApplyChanges();

            blackHole = new GameEntity();
            blackHole.position = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
            blackHole.boundingSphere = new BoundingSphere(new Vector3(blackHole.position, 0), 80);

            player = new GameEntity();

            rng = new Random();

            tinyPlanets = new Queue<TinyPlanet>();
            smallPlanets = new Queue<SmallPlanet>();
            mediumPlanets = new Queue<MediumPlanet>();
            largePlanets = new Queue<LargePlanet>();

            radius = 400.0f;
            angle = 0.0f;
            speed = 2;
            score = 0;

            brain = new Brain(new List<int> { 5, 3, 1 });

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            blackHole.spriteBatch = spriteBatch;
            player.spriteBatch = spriteBatch;

            blackHole.sprite = Content.Load<Texture2D>("Sprites/OCL_BlackHole");
            player.sprite = Content.Load<Texture2D>("Sprites/OCL_Player");

            tinyPlanetTexture = Content.Load<Texture2D>("Sprites/OCL_TinyPlanet");
            smallPlanetTexture = Content.Load<Texture2D>("Sprites/OCL_SmallPlanet");
            mediumPlanetTexture = Content.Load<Texture2D>("Sprites/OCL_MediumPlanet");
            largePlanetTexture = Content.Load<Texture2D>("Sprites/OCL_LargePlanet");

            font = Content.Load<SpriteFont>("Fonts/myFont");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            if (!gameOver)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                    radius += 7;

                List<float> inputs = new List<float> {
                    (float)rng.Next(-5, 5),
                    (float)rng.Next(-5, 5),
                    (float)rng.Next(-5, 5),
                    (float)rng.Next(-5, 5),
                    (float)rng.Next(-5, 5)};

                var output = brain.FeedFoward(inputs);
                if (output[0] > 0)
                    radius += 7;


                if (gameTime.TotalGameTime.TotalSeconds > elapsedTime)
                {
                    elapsedTime++;
                    int r = rng.Next(0, 100);
                    if (r + score > 70)
                        spawnSmallPlanet = true;

                    if ((elapsedTime % 2) == 0)
                    {
                        r = rng.Next(0, 100);
                        if (r + score > 80)
                            spawnMediumPlanet = true;
                    }

                    if ((elapsedTime % 3) == 0)
                    {
                        r = rng.Next(0, 100);
                        if (r + score > 90)
                            spawnLargePlanet = true;
                    }

                    if ((elapsedTime % 5) == 0)
                    {
                        r = rng.Next(0, 100);
                        if (r + score > 95)
                            spawnTinyPlanet = true;
                    }
                }


                if (spawnTinyPlanet)
                {
                    tinyPlanets.Enqueue(new TinyPlanet(blackHole.position, rng, spriteBatch, tinyPlanetTexture));
                    spawnTinyPlanet = false;
                }
                if (spawnSmallPlanet)
                {
                    smallPlanets.Enqueue(new SmallPlanet(blackHole.position, rng, spriteBatch, smallPlanetTexture));
                    spawnSmallPlanet = false;
                }
                if (spawnMediumPlanet)
                {
                    mediumPlanets.Enqueue(new MediumPlanet(blackHole.position, rng, spriteBatch, mediumPlanetTexture));
                    spawnMediumPlanet = false;
                }
                if (spawnLargePlanet)
                {
                    largePlanets.Enqueue(new LargePlanet(blackHole.position, rng, spriteBatch, largePlanetTexture));
                    spawnLargePlanet = false;
                }

                player.position = new Vector2(blackHole.position.X + (float)Math.Cos(angle) * radius, blackHole.position.Y + (float)Math.Sin(angle) * radius);
                player.boundingSphere = new BoundingSphere(new Vector3(player.position, 0), 22);

                double previousAngle = angle;
                angle = ((angle + speed * (float)gameTime.ElapsedGameTime.TotalSeconds) % (2*Math.PI));
                if (angle < previousAngle)
                {
                    score++;
                }

                if (radius > 430)
                {
                    radius = 430;
                    speed = 2.0f;
                }
                else if (radius > 300)
                {
                    speed = 2.0f;
                }
                else if (radius > 200)
                {
                    speed = 2.5f;
                }
                else if (radius > 100)
                {
                    speed = 3;
                }

                radius -= 200 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                bool planetExpired = false;

                foreach (var planet in tinyPlanets)
                {
                    planet.Update(gameTime);

                    if (player.boundingSphere.Intersects(planet.boundingSphere))
                        gameOver = true;

                    if (blackHole.boundingSphere.Contains(planet.boundingSphere) == ContainmentType.Contains)
                        planetExpired = true;
                }

                if (planetExpired)
                    tinyPlanets.Dequeue();

                planetExpired = false;

                foreach (var planet in smallPlanets)
                {
                    planet.Update(gameTime);

                    if (player.boundingSphere.Intersects(planet.boundingSphere))
                        gameOver = true;

                    if (blackHole.boundingSphere.Contains(planet.boundingSphere) == ContainmentType.Contains)
                        planetExpired = true;
                }

                if (planetExpired)
                    smallPlanets.Dequeue();

                planetExpired = false;

                foreach (var planet in mediumPlanets)
                {
                    planet.Update(gameTime);

                    if (player.boundingSphere.Intersects(planet.boundingSphere))
                        gameOver = true;

                    if (blackHole.boundingSphere.Contains(planet.boundingSphere) == ContainmentType.Contains)
                        planetExpired = true;
                }

                if (planetExpired)
                    mediumPlanets.Dequeue();

                planetExpired = false;

                foreach (var planet in largePlanets)
                {
                    planet.Update(gameTime);

                    if (player.boundingSphere.Intersects(planet.boundingSphere))
                        gameOver = true;

                    if (blackHole.boundingSphere.Contains(planet.boundingSphere) == ContainmentType.Contains)
                        planetExpired = true;
                }

                if (planetExpired)
                    largePlanets.Dequeue();

                if (player.boundingSphere.Intersects(blackHole.boundingSphere))
                    gameOver = true;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(34, 18, 57));

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            Vector2 textMiddlePoint = font.MeasureString("Score: " + score.ToString());
            Vector2 textPos = new Vector2(graphics.PreferredBackBufferWidth / 2, 50);
            spriteBatch.DrawString(font, "Score: " + score.ToString(), textPos, Color.White, 0, textMiddlePoint, 1.5f, SpriteEffects.None, 0.5f);
            blackHole.Draw();
            player.Draw();
            foreach (var planet in tinyPlanets)
                planet.Draw();
            foreach (var planet in smallPlanets)
                planet.Draw();
            foreach (var planet in mediumPlanets)
                planet.Draw();
            foreach (var planet in largePlanets)
                planet.Draw();
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
