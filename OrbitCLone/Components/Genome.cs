using ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitCLone.Components
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

    struct Genome : IComponent
    {
        public int NumInputs, NumOutputs, NumNodes;
        public List<NodeType> NodeGenes { get; set; }
        public List<ConnectionGene> ConnectionGenes { get; set; }
    }
}
