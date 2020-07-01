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
            weights = new List<Matrix>();

            for (int i = 0; i < shape.Count - 1; i++)
            {
                weights.Add(new Matrix(shape[i] + 1, shape[i + 1]));
            }

            foreach (var matrix in weights)
                matrix.Randomize();
        }

        public void Mutate()
        {
            if ((float)rng.NextDouble() <= mutationChance)
            {
                int weightChoice = rng.Next(0, weights.Count);
                weights[weightChoice].Mutate();
            }
        }

        public List<float> FeedFoward(List<float> inputs)
        {
            List<float> result = inputs;
            result.Add(1.0f);

            foreach (var matrix in weights)
            {
                result = matrix.Multiply(result);
                for (int i = 0; i < result.Count; i++)
                {
                    if (result[i] > 0.0f)
                        result[i] = 1.0f;
                    else
                        result[i] = 0.0f;
                }
                result.Add(1.0f);
            }

            result.RemoveAt(result.Count - 1);
            return result;
        }

        private List<Matrix> weights;
        private static float mutationChance = 0.02f;
        private static Random rng = new Random();
    }
}
