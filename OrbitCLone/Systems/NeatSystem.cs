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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OrbitCLone.Systems
{
    // Type of message to indicate that one run has ended and another should begin.
    class NewRunMessage : Message
    {
        public int GenerationNumber;

        public NewRunMessage(int generation)
        {
            GenerationNumber = generation;
        }
    }

    class Species
    {
        public Genome Representative;
        public List<(Genome genome, float fitness)> Members;
        //public List<float> Fitnesses;
        public float MaxFitness;
        public float FitnessSum;
        public int Staleness;
        public int NumOffspring;

        public Species()
        {
            Members = new List<(Genome genome, float fitness)>();
            //Fitnesses = new List<float>();
            MaxFitness = 0.0f;
            FitnessSum = 0.0f;
            Staleness = 0;
            Representative = new Genome();
            NumOffspring = 0;
        }
    }
    
    // NeatSystem handles the implementation of the N.E.A.T. algorithm.
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
        public double AddNodeMutationChance { get; set; }
        public double ModifyWeightMutationChance { get; set; }
        public double WeightPerturbationChance { get; set; }
        public double WeightOverrideChance { get; set; }

        // Determines the chance that an inherited gene will be disabled if it was disabled in either parent.
        public double DisableOnCrossoverChance { get; set; }

        // Agent texture used to create new agents.
        public Texture2D AgentTexture;

        public override void Initialize()
        {
            genomes = new List<(Genome, Score)>();
            speciesList = new List<Species>();
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

                //Speciate the initial genomes.
                if (speciesList.Any())
                {
                    bool isNewSpecies = false;
                    var newSpecies = new Species();
                    // Iterate through species to see if g belongs to any.
                    foreach(var species in speciesList)
                    {
                        if (distance(g, species.Representative) > Threshold)
                        {
                            isNewSpecies = true;
                            break;
                        }
                    }
                    if (isNewSpecies)
                    {
                        newSpecies.Representative = copyGenome(g);
                        speciesList.Add(newSpecies);
                    }
                }
                else
                {
                    var newSpecies = new Species();
                    newSpecies.Representative = copyGenome(g);
                    speciesList.Add(newSpecies);
                }

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
                }
            });

            if (genomes.Count == PopulationSize)
                entityManager.SendMessage(new NewRunMessage(++generation));
        }

        public override void HandleMessage(Message m)
        {
            // If m is a NewRunMessage, prepare a new generation.
            if (m is NewRunMessage)
            {
                // Speciate the previous generation.
                foreach (var (g, s) in genomes)
                {
                    bool isNewSpecies = true;
                    for (int i = 0; i < speciesList.Count; i++)
                    {
                        if (distance(g, speciesList[i].Representative) < Threshold)
                        {
                            isNewSpecies = false;
                            speciesList[i].Members.Add((copyGenome(g), calcFitness(s)));
                            //speciesList[i].Fitnesses.Add(calcFitness(s));
                            break;
                        }
                    }

                    if (isNewSpecies)
                    {
                        var newSpecies = new Species();
                        newSpecies.Representative = copyGenome(g);
                        newSpecies.Members.Add((copyGenome(g), calcFitness(s)));
                        //newSpecies.Fitnesses.Add(calcFitness(s));
                        speciesList.Add(newSpecies);
                    }
                }

                genomes.Clear();

                // Cull empty species.
                var removeIndices = new List<int>();
                for (int i = 0; i < speciesList.Count; i++)
                {
                    if (!speciesList[i].Members.Any())
                        removeIndices.Add(i);
                }
                removeIndices.Reverse();
                foreach (var index in removeIndices)
                    speciesList.RemoveAt(index);

                // Adjust the fitness of individuals based on species size. (Explicit fitness sharing).
                foreach (var species in speciesList)
                {
                    for (int i = 0; i < species.Members.Count; i++)
                        species.Members[i] = (species.Members[i].genome, species.Members[i].fitness / species.Members.Count);
                }

                // Calculate each species' max fitness, fitness sum, and staleness.
                foreach (var species in speciesList)
                {
                    float maxFitness = 0.0f;
                    foreach (var (_, fitness) in species.Members)
                    {
                        species.FitnessSum += fitness;
                        if (fitness > maxFitness)
                            maxFitness = fitness;
                    }

                    if (maxFitness > species.MaxFitness)
                        species.MaxFitness = maxFitness;
                    else
                        species.Staleness++;
                }

                float totalFitness = 0.0f;
                foreach (var species in speciesList)
                    totalFitness += species.FitnessSum;

                // Determine how many offspring each species should produce.
                int n = 0;
                foreach (var species in speciesList)
                {
                    double proportion = species.FitnessSum / totalFitness;
                    species.NumOffspring = (int)Math.Round(PopulationSize * proportion);
                    n += species.NumOffspring;
                }

                Debug.WriteLine(n);

                // Kill the bottom half of each species.
                foreach (var species in speciesList)
                {
                    species.Members.Sort(((Genome, float fitness) m1, (Genome, float fitness) m2) => (int)(m2.fitness - m1.fitness));
                    int removeIndex = species.Members.Count / 2 + species.Members.Count % 2;
                    species.Members.RemoveRange(removeIndex, species.Members.Count - removeIndex);
                }                

                // Have the species reproduce.
                foreach (var species in speciesList)
                {
                    int numBabiesProduced = 0;

                    // If the species has 5 or more members, copy the most fit individual straight over.
                    if (species.Members.Count >= 5)
                    {
                        genomes.Add((copyGenome(species.Members[0].genome), new Score(0)));
                        numBabiesProduced++;
                    }

                    // If there is only one organism in the species, have it reproduce asexually.
                    if (species.Members.Count == 1)
                    {
                        for (int i = 0; i < species.NumOffspring; i++)
                        {
                            Genome baby = species.Members[0].genome;
                            // TODO: mutate baby.
                            genomes.Add((baby, new Score(0)));
                        }
                    }
                    else
                    {
                        while (numBabiesProduced < species.NumOffspring)
                        {
                            for (int i = 0; i < species.Members.Count; i++)
                                for (int j = i + 1; j < species.Members.Count; j++)
                                {
                                    Genome baby = crossover(species.Members[i], species.Members[j]);
                                    // TODO: mutate baby.
                                    genomes.Add((baby, new Score(0)));
                                    numBabiesProduced++;
                                    if (numBabiesProduced >= species.NumOffspring)
                                    {
                                        i = int.MaxValue - 1;
                                        break;
                                    }
                                }
                        }
                    }
                    species.Members.Clear();
                }

                int remaining = PopulationSize - genomes.Count;
                for (int i = 0; i < remaining; i++)
                {
                    genomes.Add((newSimpleGenome(), new Score(0)));
                }
                Debug.WriteLine(genomes.Count);

                // Populate new entities with baby genomes.
                var agentArchetype = entityManager.CreateArchetype(
                    typeof(Position),
                    typeof(CircleCollider),
                    typeof(PlayerTag),
                    typeof(Sprite),
                    typeof(Pcnn),
                    typeof(Gravity),
                    typeof(RotationalSpeed),
                    typeof(PolarCoordinate),
                    typeof(Score),
                    typeof(Genome));

                foreach (var genome in genomes)
                {
                    var agent = entityManager.CreateEntity(agentArchetype);
                    entityManager.SetComponentData(new PolarCoordinate(0.0, 400.0f), agent);
                    entityManager.SetComponentData(new Gravity(200.0f), agent);
                    entityManager.SetComponentData(new CircleCollider(22), agent);
                    entityManager.SetComponentData(new RotationalSpeed(2.0f), agent);
                    entityManager.SetComponentData(genome.Item2, agent);
                    entityManager.SetComponentData(new Sprite(AgentTexture), agent);
                    entityManager.SetComponentData(new Pcnn(genome.Item1), agent);
                    entityManager.SetComponentData(copyGenome(genome.Item1), agent);
                }

                /*var agentArchetype = entityManager.CreateArchetype(typeof(Position), typeof(CircleCollider), typeof(PlayerTag), typeof(Sprite), typeof(Pcnn), typeof(Gravity), typeof(RotationalSpeed), typeof(PolarCoordinate), typeof(Score), typeof(Genome));
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
                }*/

                // At the end of everything.
                genomes.Clear();
                structuralMutations.Clear();
            }
        }

        // Crosses over parent1 and parent2
        private Genome crossover((Genome genome, float fitness) parent1, (Genome genome, float fitness) parent2)
        {
            Genome fitGenome, unfitGenome;
            if (parent1.fitness > parent2.fitness)
            {
                fitGenome = parent1.genome;
                unfitGenome = parent2.genome;
            }
            else
            {
                fitGenome = parent2.genome;
                unfitGenome = parent1.genome;
            }

            Genome newGenome = new Genome(fitGenome.NumInputs, fitGenome.NumOutputs, fitGenome.NumNodes);           

            foreach (var connectionGene in fitGenome.ConnectionGenes)
            {
                ConnectionGene matchingGene = unfitGenome.ConnectionGenes.Find((ConnectionGene cg) => cg.Innovation == connectionGene.Innovation);

                // If both parents have this gene, one is randomly inherited.
                if (matchingGene != default)
                {
                    ConnectionGene newGene;
                    bool enabled = true;
                    if (!matchingGene.Enabled || !connectionGene.Enabled)
                    {
                        enabled = r.NextDouble() > DisableOnCrossoverChance;
                    }

                    if (r.Next(0, 1) == 1)
                        newGene = connectionGene;
                    else
                        newGene = matchingGene;

                    newGene.Enabled = enabled;
                    newGenome.ConnectionGenes.Add(newGene);
                }
                // Otherwise, inherit the gene from the more fit parent.
                else
                {
                    ConnectionGene newGene;
                    bool enabled = true;
                    if (!connectionGene.Enabled)
                        enabled = r.NextDouble() > DisableOnCrossoverChance;
                    newGene = connectionGene;
                    newGene.Enabled = enabled;
                    newGenome.ConnectionGenes.Add(newGene);
                }
            }

            // Copy the more fit parent's node genes.
            foreach (var nodeGene in fitGenome.NodeGenes)
            {
                newGenome.NodeGenes.Add(nodeGene);
            }

            return newGenome;
        }

        // Mutate a genome with a new node.
        private Genome mutateAddNode(Genome in_g)
        {
            Genome g = copyGenome(in_g);
            int split_choice = r.Next(0, g.ConnectionGenes.Count);
            ConnectionGene split_gene = g.ConnectionGenes[split_choice];
            split_gene.Enabled = false;
            g.ConnectionGenes[split_choice] = split_gene;
            int new_node = g.NodeGenes.Count;
            g.NodeGenes.Add(NodeType.Hidden);
            g.NumNodes++;
            ConnectionGene new_conn_in = new ConnectionGene()
            {
                In = split_gene.In,
                Out = new_node,
                Weight = 1,
                Enabled = true
            };

            ConnectionGene new_conn_out = new ConnectionGene()
            {
                In = new_node,
                Out = split_gene.Out,
                Weight = split_gene.Weight,
                Enabled = true
            };

            if (structuralMutations.Contains(new_conn_in))
            {
                ConnectionGene c = structuralMutations.Find((ConnectionGene cg) => cg == new_conn_in);
                new_conn_in.Innovation = c.Innovation;
            }
            else
            {
                new_conn_in.Innovation = innovation++;
                structuralMutations.Add(new_conn_in);
            }

            if (structuralMutations.Contains(new_conn_out))
            {
                ConnectionGene c = structuralMutations.Find((ConnectionGene cg) => cg == new_conn_out);
                new_conn_out.Innovation = c.Innovation;
            }
            else
            {
                new_conn_out.Innovation = innovation++;
                structuralMutations.Add(new_conn_out);
            }

            g.ConnectionGenes.Add(new_conn_in);
            g.ConnectionGenes.Add(new_conn_out);

            return g;
        }
        
        // Mutate a genome with a new connection.
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

        // Calculate organism fitness based on score.
        private float calcFitness(Score s)
        {
            return (float)Math.Pow(s.Value + s.counter / 3, 2);
        }

        // Calculate adjusted fitness of an organism based on explicit fitness sharing.
        private float calcAdjustedFitness(Genome g, float fitness)
        {
            int speciesSize = 0;

            foreach (var other in genomes)
            {
                if (distance(g, other.Item1) <= Threshold)
                    speciesSize++;
            }

            return fitness / speciesSize;
        }

        // Finds the compatibility distance between different genomes.
        private float distance(Genome g1, Genome g2)
        {
            int N = g1.ConnectionGenes.Count > g2.ConnectionGenes.Count ? g1.ConnectionGenes.Count : g2.ConnectionGenes.Count;
            float c1 = ExcessCoefficient;
            float c2 = DisjointCoefficient;
            float c3 = WeightDifferenceCoefficient;

            int E = findNumExcess(g1, g2);
            int D = findNumDisjoint(g1, g2);
            float W = calcAverageWeightDifference(g1, g2);

            return c1 * E / N + c2 * D / N + c3 * W;
        }

        // Find the number of disjoint genes between two genomes.
        private int findNumDisjoint(Genome g1, Genome g2)
        {
            int numDisjoint = 0;

            var g1Innovations = new HashSet<int>();
            var g2Innovations = new HashSet<int>();

            int g1MaxInnovation = g1.ConnectionGenes.Any() ? g1.ConnectionGenes.Last().Innovation : -1;
            int g2MaxInnovation = g2.ConnectionGenes.Any() ? g2.ConnectionGenes.Last().Innovation : -1;

            // Fill in the sets with all non-excess innovations.
            foreach (var connectionGene in g1.ConnectionGenes)
            {
                if (connectionGene.Innovation <= g2MaxInnovation)
                {
                    g1Innovations.Add(connectionGene.Innovation);
                }
                else
                    break;
            }

            foreach (var connectionGene in g2.ConnectionGenes)
            {
                if (connectionGene.Innovation <= g1MaxInnovation)
                {
                    g2Innovations.Add(connectionGene.Innovation);
                }
                else
                    break;
            }

            foreach (var innovation in g1Innovations)
            {
                if (!g2Innovations.Contains(innovation))
                    numDisjoint++;
            }

            foreach (var innovation in g2Innovations)
            {
                if (!g1Innovations.Contains(innovation))
                    numDisjoint++;
            }

            return numDisjoint;
        }

        // Find the number of excess genes between two genomes.
        private int findNumExcess(Genome g1, Genome g2)
        {
            int numExcess = 0;
            int g1MaxInnovation = g1.ConnectionGenes.Any() ? g1.ConnectionGenes.Last().Innovation : -1;
            int g2MaxInnovation = g2.ConnectionGenes.Any() ? g2.ConnectionGenes.Last().Innovation : -1;

            if (g1MaxInnovation > g2MaxInnovation)
            {
                for (int i = g1.ConnectionGenes.Count - 1; i >= 0; i--)
                {
                    if (g1.ConnectionGenes[i].Innovation > g2MaxInnovation)
                        numExcess++;
                    else
                        break;
                }
            }
            else if (g2MaxInnovation > g1MaxInnovation)
            {
                for (int i = g2.ConnectionGenes.Count - 1; i >= 0; i--)
                {
                    if (g2.ConnectionGenes[i].Innovation > g1MaxInnovation)
                        numExcess++;
                    else
                        break;
                }
            }

            return numExcess;
        }

        // Finds the average weight difference of matching genes, including disabled genes.
        private float calcAverageWeightDifference(Genome g1, Genome g2)
        {
            float weightSum = 0.0f;
            int numMatches = 0;

            foreach (var g1Connection in g1.ConnectionGenes)
            {
                ConnectionGene g2Connection = g2.ConnectionGenes.Find((ConnectionGene g) => g.Innovation == g1Connection.Innovation);
                if (g2Connection != default)
                {
                    weightSum += g1Connection.Weight - g2Connection.Weight;
                    numMatches++;
                }
            }

            return weightSum / numMatches;
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

        // Creates a new genome with no hidden nodes and random connections between inputs and outputs.
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

        private List<Species> speciesList; // The plural of species is species; RIP my list naming convention.
        private List<(Genome, Score)> genomes;
        private int generation;
        private int innovation;
        private List<ConnectionGene> structuralMutations;
        private Random r;
    }
}
