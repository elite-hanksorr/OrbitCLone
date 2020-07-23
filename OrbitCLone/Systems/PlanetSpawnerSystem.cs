using ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrbitCLone.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitCLone.Systems
{
    class PlanetSpawnerSystem : ComponentSystem
    {
        public Texture2D tinyPlanetTexture;
        public Texture2D smallPlanetTexture;
        public Texture2D mediumPlanetTexture;
        public Texture2D largePlanetTexture;

        public int tinyPlanetRadius;
        public int smallPlanetRadius;
        public int mediumPlanetRadius;
        public int largePlanetRadius;

        public float tinyPlanetSpeed;
        public float smallPlanetSpeed;
        public float mediumPlanetSpeed;
        public float largePlanetSpeed;

        private int elapsedTime;
        private Random rng;
        private Archetype enemyPlanetArchetype;

        public override void Initialize()
        {
            elapsedTime = 0;
            rng = new Random();
            enemyPlanetArchetype = entityManager.CreateArchetype(typeof(EnemyTag), typeof(Position), typeof(Sprite), typeof(CircleCollider), typeof(Velocity), typeof(PolarCoordinate));

            base.Initialize();
        }

        public override void HandleMessage(Message m)
        {
            if (m is NewRunMessage)
            {
                Entities.ForEach((ref EnemyTag _, ref Entity e) =>
                {
                    entityManager.RequestAction(entityManager.DeleteEntity, e);
                });
            }
        }

        public override void OnUpdate(GameTime gt)
        {
            // Calculate current high score.
            int high_score = 0;

            Entities.ForEach((ref Score s) =>
            {
                if (s.Value > high_score)
                    high_score = s.Value;
            });

            //Spawn enemy planets.
            if (gt.TotalGameTime.TotalSeconds > elapsedTime)
            {
                elapsedTime++;
                int r = rng.Next(0, 100);
                if (r + high_score > 50)
                    CreateSmallPlanet();

                if ((elapsedTime % 2) == 0)
                {
                    r = rng.Next(0, 100);
                    if (r + high_score > 55)
                        CreateMediumPlanet();
                }

                if ((elapsedTime % 3) == 0)
                {
                    r = rng.Next(0, 100);
                    if (r + high_score > 60)
                        CreateLargePlanet();
                }

                if ((elapsedTime % 5) == 0)
                {
                    r = rng.Next(0, 100);
                    if (r + high_score > 70)
                        CreateTinyPlanet();
                }
            }
        }

        private void CreateTinyPlanet()
        {
            var p = entityManager.CreateEntity(enemyPlanetArchetype);
            double a = rng.NextDouble() * Math.PI;
            float r = 1000.0f;
            entityManager.SetComponentData(new PolarCoordinate(a, r), p);

            float pos_x = GameConfig.SCREEN_WIDTH / 2 + (float)Math.Cos(a) * r;
            float pos_y = GameConfig.SCREEN_HEIGHT / 2 + (float)Math.Sin(a) * r;
            entityManager.SetComponentData(new Position(pos_x, pos_y), p);

            float d_x = (GameConfig.SCREEN_WIDTH / 2 - pos_x) * tinyPlanetSpeed;
            float d_y = (GameConfig.SCREEN_HEIGHT / 2 - pos_y) * tinyPlanetSpeed;
            entityManager.SetComponentData(new Velocity(d_x, d_y), p);
            entityManager.SetComponentData(new CircleCollider(tinyPlanetRadius), p);
            entityManager.SetComponentData(new Sprite(tinyPlanetTexture), p);
        }
        private void CreateSmallPlanet()
        {
            var p = entityManager.CreateEntity(enemyPlanetArchetype);
            double a = rng.NextDouble() * Math.PI;
            float r = 1000.0f;
            entityManager.SetComponentData(new PolarCoordinate(a, r), p);

            float pos_x = GameConfig.SCREEN_WIDTH / 2 + (float)Math.Cos(a) * r;
            float pos_y = GameConfig.SCREEN_HEIGHT / 2 + (float)Math.Sin(a) * r;
            entityManager.SetComponentData(new Position(pos_x, pos_y), p);

            float d_x = (GameConfig.SCREEN_WIDTH / 2 - pos_x) * smallPlanetSpeed;
            float d_y = (GameConfig.SCREEN_HEIGHT / 2 - pos_y) * smallPlanetSpeed;
            entityManager.SetComponentData(new Velocity(d_x, d_y), p);
            entityManager.SetComponentData(new CircleCollider(smallPlanetRadius), p);
            entityManager.SetComponentData(new Sprite(smallPlanetTexture), p);
        }
        private void CreateMediumPlanet()
        {
            var p = entityManager.CreateEntity(enemyPlanetArchetype);
            double a = rng.NextDouble() * Math.PI;
            float r = 1000.0f;
            entityManager.SetComponentData(new PolarCoordinate(a, r), p);

            float pos_x = GameConfig.SCREEN_WIDTH / 2 + (float)Math.Cos(a) * r;
            float pos_y = GameConfig.SCREEN_HEIGHT / 2 + (float)Math.Sin(a) * r;
            entityManager.SetComponentData(new Position(pos_x, pos_y), p);

            float d_x = (GameConfig.SCREEN_WIDTH / 2 - pos_x) * mediumPlanetSpeed;
            float d_y = (GameConfig.SCREEN_HEIGHT / 2 - pos_y) * mediumPlanetSpeed;
            entityManager.SetComponentData(new Velocity(d_x, d_y), p);
            entityManager.SetComponentData(new CircleCollider(mediumPlanetRadius), p);
            entityManager.SetComponentData(new Sprite(mediumPlanetTexture), p);
        }
        private void CreateLargePlanet()
        {
            var p = entityManager.CreateEntity(enemyPlanetArchetype);
            double a = rng.NextDouble() * Math.PI;
            float r = 1000.0f;
            entityManager.SetComponentData(new PolarCoordinate(a, r), p);

            float pos_x = GameConfig.SCREEN_WIDTH / 2 + (float)Math.Cos(a) * r;
            float pos_y = GameConfig.SCREEN_HEIGHT / 2 + (float)Math.Sin(a) * r;
            entityManager.SetComponentData(new Position(pos_x, pos_y), p);

            float d_x = (GameConfig.SCREEN_WIDTH / 2 - pos_x) * largePlanetSpeed;
            float d_y = (GameConfig.SCREEN_HEIGHT / 2 - pos_y) * largePlanetSpeed;
            entityManager.SetComponentData(new Velocity(d_x, d_y), p);
            entityManager.SetComponentData(new CircleCollider(largePlanetRadius), p);
            entityManager.SetComponentData(new Sprite(largePlanetTexture), p);
        }
    }
}
