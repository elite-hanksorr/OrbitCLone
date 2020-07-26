using ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrbitCLone.Components;
using OrbitCLone.NEAT;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OrbitCLone.Systems
{
    class NewRunMessage : Message
    {
        public int GenerationNumber;

        public NewRunMessage(int generation)
        {
            GenerationNumber = generation;
        }
    }

    class NeatSystem : ComponentSystem
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

        // Agent texture used to create new agents.
        public Texture2D AgentTexture;

        public override void Initialize()
        {
            genomes = new List<(Genome, Score)>();
            generation = 1;
            innovation = 1;
            structuralMutations = new List<ConnectionGene>();
            r = new Random();

            // Create initial population.
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
                Genome g = newSimpleGenome();
                entityManager.SetComponentData(g, a);
                entityManager.SetComponentData(new Pcnn(g), a);
            }

            structuralMutations.Clear();
            base.Initialize();
        }

        public override void OnUpdate(GameTime gt)
        {
            Entities.ForEach((ref Genome g, ref Score s, ref CircleCollider c, ref Entity e) =>
            {
                if (c.HasCollided)
                {
                    genomes.Add((copyGenome(g), s));
                    entityManager.RequestAction(entityManager.DeleteEntity, e);
                    Debug.WriteLine(genomes.Count);
                }
            });

            if (genomes.Count == PopulationSize)
                entityManager.SendMessage(new NewRunMessage(++generation));
        }

        public override void HandleMessage(Message m)
        {
            if (m is NewRunMessage)
            {
                var agentArchetype = entityManager.CreateArchetype(typeof(Position), typeof(CircleCollider), typeof(PlayerTag), typeof(Sprite), typeof(Pcnn), typeof(Gravity), typeof(RotationalSpeed), typeof(PolarCoordinate), typeof(Score), typeof(Genome));
                var agents = entityManager.CreateEntity(agentArchetype, PopulationSize);
                int i = 0;
                foreach (var a in agents)
                {
                    entityManager.SetComponentData(new PolarCoordinate(0.0, 400.0f), a);
                    entityManager.SetComponentData(new Gravity(200.0f), a);
                    entityManager.SetComponentData(new CircleCollider(22), a);
                    entityManager.SetComponentData(new RotationalSpeed(2.0f), a);
                    entityManager.SetComponentData(new Score(0), a);
                    entityManager.SetComponentData(new Sprite(AgentTexture), a);
                    entityManager.SetComponentData(new Pcnn(genomes[i].Item1), a);
                    entityManager.SetComponentData(copyGenome(genomes[i].Item1), a);
                    i++;
                }

                // At the end of everything.
                genomes.Clear();
                structuralMutations.Clear();
            }
        }

        private Genome mutateAddConnection(Genome in_g)
        {
            Genome g = copyGenome(in_g);
            int node1 = r.Next(0, g.NumNodes);
            int node2 = r.Next(0, g.NumNodes);
            // Cover cases where no mutation can occur.
            if (node1 == node2) return g;
            if (g.NodeGenes[node1] == NodeType.Sensor && g.NodeGenes[node2] == NodeType.Sensor) return g;
            if (g.NodeGenes[node1] == NodeType.Output && g.NodeGenes[node2] == NodeType.Output) return g;

            // Ensure node1 is in an earlier layer than node2 i.e. no recurrent connections.
            if (g.NodeGenes[node1] > g.NodeGenes[node2])
            {
                int temp = node1;
                node1 = node2;
                node2 = temp;
            }

            ConnectionGene new_connection = new ConnectionGene()
            {
                In = node1,
                Out = node2,
                Weight = (float)r.NextDouble() * 8 - 4,
                Enabled = true
            };

            // Check if genome already has this connection gene.
            if (g.ConnectionGenes.Contains(new_connection)) return g;

            // Check if same structural mutation already occured this generation.
            if (structuralMutations.Contains(new_connection))
            {
                ConnectionGene c = structuralMutations.Find((ConnectionGene cg) => cg == new_connection);
                new_connection.Innovation = c.Innovation;
            }
            else
            {
                new_connection.Innovation = innovation++;
                structuralMutations.Add(new_connection);
            }

            g.ConnectionGenes.Add(new_connection);
            return g;
        }

        private Genome copyGenome(Genome g)
        {
            var new_genome = new Genome()
            {
                NumInputs = g.NumInputs,
                NumOutputs = g.NumOutputs,
                NumNodes = g.NumNodes
            };

            new_genome.NodeGenes = new List<NodeType>(g.NodeGenes.Count);
            foreach (var n in g.NodeGenes)
                new_genome.NodeGenes.Add(n);

            new_genome.ConnectionGenes = new List<ConnectionGene>(g.ConnectionGenes.Count);
            foreach (var c in g.ConnectionGenes)
            {
                var new_connection = new ConnectionGene()
                {
                    In = c.In,
                    Out = c.Out,
                    Weight = c.Weight,
                    Enabled = c.Enabled,
                    Innovation = c.Innovation
                };

                new_genome.ConnectionGenes.Add(new_connection);
            }
            return new_genome;
        }

        private Genome newSimpleGenome()
        {
            Genome g = new Genome(NumInputs, NumOutputs, NumInputs + NumOutputs);
            
            // Initialize nodes.
            for (int i = 0; i < g.NumNodes; i++)
            {
                if (i < NumInputs)
                    g.NodeGenes.Add(NodeType.Sensor);
                else
                    g.NodeGenes.Add(NodeType.Output);
            }

            // Initialize connections.
            int numConnections = r.Next(0, g.NumInputs * g.NumOutputs);
            for (int i = 0; i < numConnections; i++)
            {
                g = mutateAddConnection(g);
            }

            return g;
        }

        private List<(Genome, Score)> genomes;
        private int generation;
        private int innovation;
        private List<ConnectionGene> structuralMutations;
        private Random r;
    }
}
