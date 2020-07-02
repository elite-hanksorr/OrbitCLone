using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitLearner
{
    public class Brain
    {
        //shape input describes the shape of the nn
        //e.g. {5, 3, 2, 1} means a nn with 5 inputs, 3 and 2 neuron hidden layers, and 1 output
        public Brain(List<int> shape)
        {
            //set up weight and bias augmented matrices
            Weights = new List<Matrix>();

            for (int i = 0; i < shape.Count - 1; i++)
            {
                Weights.Add(new Matrix(shape[i] + 1, shape[i + 1]));
            }

            foreach (var matrix in Weights)
                matrix.Randomize();
        }

        public void Mutate(float mutationRate)
        {
            /*if ((float)rng.NextDouble() <= mutationRate)
            {
                int weightChoice = rng.Next(0, Weights.Count);
                Weights[weightChoice].Mutate();
            }*/

            int numMutations = rng.Next(0, 100);
            for (int i = 0; i < numMutations; i++)
            {
                int weightChoice = rng.Next(0, Weights.Count);
                Weights[weightChoice].Mutate();
            }
        }

        public List<float> FeedFoward(List<float> inputs)
        {
            List<float> result = inputs;
            result.Add(1.0f);

            foreach (var matrix in Weights)
            {
                result = matrix.Multiply(result);
                for (int i = 0; i < result.Count; i++)
                {
                    /*if (result[i] > 0.0f)
                        result[i] = 1.0f;
                    else
                        result[i] = 0.0f;*/

                    result[i] = (float)(1 / (1 + Math.Pow(Math.E, -1.0 * result[i])));
                }
                result.Add(1.0f);
            }

            result.RemoveAt(result.Count - 1);
            return result;
        }

        public List<Matrix> Weights { get; set; }
        private static Random rng = new Random();
    }
}
