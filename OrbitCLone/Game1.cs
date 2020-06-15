using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;

namespace OrbitCLone
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        GameEntity player;
        GameEntity blackHole;

        double angle;
        float radius;
        float speed;

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
            player = new GameEntity();

            blackHole.position = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
            radius = 400.0f;
            angle = 0.0f;
            speed = 2;

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
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                radius += 10;

            player.position = new Vector2(blackHole.position.X + (float)Math.Cos(angle) * radius, blackHole.position.Y + (float)Math.Sin(angle) * radius);

            angle = ((angle + speed * (float)gameTime.ElapsedGameTime.TotalSeconds) % 365);

            if (radius > 400)
            {
                radius = 400;
                speed = 2.5f;
            }
            else if (radius > 300)
            {
                speed = 2.5f;
            }
            else if (radius > 200)
            {
                speed = 3;
            }
            else if (radius > 100)
            {
                speed = 4;
            }

            radius -= 200 * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (radius <= 64)
            {
                radius = 64;
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
            blackHole.Draw();
            player.Draw();
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
