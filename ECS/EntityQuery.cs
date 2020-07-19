using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS
{
    public struct EntityQuery
    {
        public EntityQuery(params Type[] ts)
        {
            include_types = new HashSet<Type>(ts);
        }

        public bool Matches(Archetype arch)
        {
            return include_types.IsSubsetOf(arch.Signature);
        }

        private HashSet<Type> include_types;
    }
}
