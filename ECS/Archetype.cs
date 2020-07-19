using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS
{
    
    public struct Archetype : IEquatable<Archetype>
    {
        public Archetype(params Type[] ts)
        {
            Signature = new HashSet<Type>(ts);
        }

        public HashSet<Type> Signature;

        public bool Contains(Type t)
        {
            return Signature.Contains(t);
        }

        public bool Equals(Archetype other)
        {
            return Signature == other.Signature;
        }
    }
}
