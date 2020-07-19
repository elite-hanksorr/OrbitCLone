using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitLearner
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
    }

    class Agent
    {
        public Genome Genotype { get; set; }
        public PCNN Phenotype { get; set; }
        public float Fitness { get; set; }
    }

    class Population
    {
        public List<Agent> Agents { get; set; }

        public void Initialize(int popSize)
        {

        }

        private List<ConnectionGene> structuralInnovations;
        private int innovationNumber = 1;
    }
}
