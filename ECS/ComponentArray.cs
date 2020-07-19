using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS
{
    abstract class BaseComponentArray
    {
        public abstract void AddElement();
        
    }

    class ComponentArray<T> : BaseComponentArray
        where T : struct, IComponent
    {
        public ComponentArray()
        {
            components = new List<T>();
        }

        public override void AddElement()
        {
            components.Add(new T());
        }

        private List<T> components;
    }
}
