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

    struct ConnectionGene : IEquatable<ConnectionGene>
    {
        public int In { get; set; }
        public int Out { get; set; }
        public float Weight { get; set; }
        public bool Enabled { get; set; }
        public int Innovation { get; set; }

        public bool Equals(ConnectionGene other)
        {
            if (other == null) return false;

            return (In == other.In) && (Out == other.Out);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is ConnectionGene g)
                return Equals(g);
            return false;
        }

        public static bool operator ==(ConnectionGene a, ConnectionGene b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ConnectionGene a, ConnectionGene b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return (int)(Weight * (In + Out + Innovation));
        }
    }

    struct Genome : IComponent
    {
        public Genome(int numIns, int numOuts, int numNodes)
        {
            NumInputs = numIns;
            NumOutputs = numOuts;
            NumNodes = numNodes;
            NodeGenes = new List<NodeType>(numNodes);
            ConnectionGenes = new List<ConnectionGene>();
        }

        public int NumInputs, NumOutputs, NumNodes;
        public List<NodeType> NodeGenes { get; set; }
        public List<ConnectionGene> ConnectionGenes { get; set; }
    }
}
