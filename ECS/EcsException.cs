using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS
{
    class EcsException : Exception
    {
        public EcsException()
        {
        }

        public EcsException(string message)
            : base(message)
        {
        }

        public EcsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
