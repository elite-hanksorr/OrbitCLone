using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS
{
    public struct Entity : IComponent
    {
        public Entity(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}
