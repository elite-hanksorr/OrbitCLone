using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitLearner
{
    public class Population
    {
        public Population(int size, List<int> brainShape)
        {
            members = new List<Brain>(size);
            for (int i = 0; i < size; i++)
                members.Add(new Brain(brainShape));
        }


        private List<Brain> members;
    }
}
