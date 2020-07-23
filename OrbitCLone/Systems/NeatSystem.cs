using ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrbitCLone.Components;
using OrbitCLone.NEAT;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitCLone.Systems
{
    class NewRunMessage : Message {}

    class NeatSystem : ComponentSystem
    {
        private List<(Genome, Score)> genomes;

        public int PopulationSize;
        public Texture2D AgentTexture;

        public override void Initialize()
        {
            genomes = new List<(Genome, Score)>();
            base.Initialize();
        }

        public override void OnUpdate(GameTime gt)
        {
            Entities.ForEach((ref Genome g, ref Score s, ref CircleCollider c, ref Entity e) =>
            {
                if (c.HasCollided)
                {
                    genomes.Add((g, s));
                    entityManager.RequestAction(entityManager.DeleteEntity, e);
                }
            });

            if (genomes.Count == PopulationSize)
                entityManager.SendMessage(new NewRunMessage());
        }

        public override void HandleMessage(Message m)
        {
            if (m is NewRunMessage)
            {
                var agentArchetype = entityManager.CreateArchetype(typeof(Position), typeof(CircleCollider), typeof(PlayerTag), typeof(Sprite), typeof(Pcnn), typeof(Gravity), typeof(RotationalSpeed), typeof(PolarCoordinate), typeof(Score), typeof(Genome));
                var agents = entityManager.CreateEntity(agentArchetype, PopulationSize);
                foreach (var a in agents)
                {
                    entityManager.SetComponentData(new PolarCoordinate(0.0, 400.0f), a);
                    entityManager.SetComponentData(new Gravity(200.0f), a);
                    entityManager.SetComponentData(new CircleCollider(22), a);
                    entityManager.SetComponentData(new RotationalSpeed(2.0f), a);
                    entityManager.SetComponentData(new Score(0), a);
                    entityManager.SetComponentData(new Sprite(AgentTexture), a);
                }
            }

            // At the end of everything.
            genomes.Clear();
        }
    }
}
