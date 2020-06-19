using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitLearner
{
    class Matrix
    {
        public Matrix(int width, int height)
        {
            w = width;
            h = height;

            rng = new Random();

            elements = new List<List<float>>();

            for (int i = 0; i < height; i++)
            {
                elements.Add(new List<float>(width));
            }
        }

        public void Randomize()
        {
            foreach (var row in elements)
            {
                float alternate = 1.0f;
                for (int i = 0; i < w; i++)
                {
                    row.Add(alternate * (float)rng.NextDouble());
                    alternate *= -1.0f;
                }
            }
        }

        public List<float> Multiply(List<float> vector)
        {
            if (vector.Count != w)
                return new List<float>();

            var output = new List<float>(h);
            
            foreach (var row in elements)
            {
                float sum = 0.0f;
                for (int i = 0; i < w; i++)
                {
                    sum += row[i] * vector[i];
                }

                output.Add(sum);
            }

            return output;
        }

        //inner list is rows, outer list is columns
        private List<List<float>> elements;
        int w, h;
        Random rng;
    }
}
