using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitLearner
{
    public class Matrix
    {
        public Matrix(int width, int height)
        {
            w = width;
            h = height;

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
                for (int i = 0; i < w; i++)
                {
                    row.Add((float)rng.NextDouble() * 10.0f - 5.0f);
                }
            }
        }

        public void Mutate()
        {
            int wChoice = rng.Next(0, w);
            int hChoice = rng.Next(0, h);

            elements[hChoice][wChoice] += (float)rng.NextDouble() * 10.0f - 5.0f;
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

        public Matrix Copy() {
            var copy = new Matrix(w, h);
            copy.elements = new List<List<float>>(h);
            this.elements.ForEach(row => copy.elements.Add(new List<float>(row)));

            return copy;
        }

        //inner list is rows, outer list is columns
        private List<List<float>> elements;
        int w, h;
        static Random rng = new Random();
    }
}
