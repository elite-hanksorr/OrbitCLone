using ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OrbitCLone.Components
{
    struct Pcnn : IComponent
    {
        public Pcnn(Genome g)
        {
            Network = new List<(List<(int neuron, float weight)> connections, float output)>(g.NumNodes);
            Connections = new List<List<(int neuron, float weight)>>();
            Results = new List<float>();
            // Initialize the network.
            for (int i = 0; i < g.NumNodes; i++)
            {
                if (i < g.NumInputs)
                {
                    Connections.Add(null);
                    Results.Add(-1);
                }
                else
                {
                    Connections.Add(new List<(int, float)>());
                    Results.Add(-1);
                }
            }

            // Keep track of which neurons are outputs.
            firstOutputNeuron = g.NumInputs;
            lastOutputNeuron = g.NumInputs + g.NumOutputs - 1;
            numInputs = g.NumInputs;

            // Initialize network connections.
            foreach (var connectionGene in g.ConnectionGenes)
            {
                if (connectionGene.Enabled)
                {
                    Connections[connectionGene.Out].Add((connectionGene.In, connectionGene.Weight));
                }
            }
        }

        public List<float> Evaluate(List<float> inputs)
        {
            if (inputs.Count != numInputs)
                return null;

            // Initialize network inputs.
            for (int i = 0; i < inputs.Count; i++)
            {
                Results[i] = inputs[i];
            }
            for (int i = inputs.Count; i < Results.Count; i++)
            {
                Results[i] = -1;
            }

            // Calculate outputs.
            List<float> outputs = new List<float>(lastOutputNeuron - firstOutputNeuron + 1);
            for (int i = firstOutputNeuron; i <= lastOutputNeuron; i++)
            {
                outputs.Add(evalRec(i));
            }

            return outputs;
        }

        private float evalRec(int neuron)
        {
            // Base case.
            if (Connections[neuron] == null)
                return Results[neuron];
            // Recursive case.
            else
            {
                // Memoized case.
                if (Results[neuron] > -1)
                    return Results[neuron];

                // Non-memoized case.
                float runningSum = 0;
                foreach (var (node, weight) in Connections[neuron])
                {
                    runningSum += evalRec(node) * weight;
                }
                float result = sigmoid(runningSum);
                Results[neuron] = result;
                return result;
            }
        }

        private float sigmoid(float x)
        {
            return 1 / (1 + (float)Math.Pow(Math.E, -4.9 * x));
        }

        public List<(List<(int neuron, float weight)> connections, float output)> Network;
        public List<List<(int neuron, float weight)>> Connections;
        public List<float> Results;
        private int firstOutputNeuron, lastOutputNeuron, numInputs;
    }
}
