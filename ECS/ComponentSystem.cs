using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace ECS
{
    public abstract class ComponentSystem
    {
        public void SetManager(EntityManager em)
        {
            entityManager = em;
            Entities = new EntityQueryBuilder(em);
        }

        public abstract void OnUpdate(GameTime gt);
        public virtual void Initialize() { }

        protected EntityManager entityManager;
        protected EntityQueryBuilder Entities;
    }
}
