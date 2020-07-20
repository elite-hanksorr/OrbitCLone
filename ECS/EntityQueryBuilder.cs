using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS
{
    public class EntityQueryBuilder
    {
        public delegate void ComponentActionDelegate<T0>(ref T0 c0);
        public delegate void ComponentActionDelegate<T0, T1>(ref T0 c0, ref T1 c1);
        public delegate void ComponentActionDelegate<T0, T1, T2>(ref T0 c0, ref T1 c1, ref T2 c2);
        public delegate void ComponentActionDelegate<T0, T1, T2, T3>(ref T0 c0, ref T1 c1, ref T2 c2, ref T3 c3);

        public EntityQueryBuilder(EntityManager entityManager)
        {
            manager = entityManager;
        }

        public void ForEach<T0>(ComponentActionDelegate<T0> action)
            where T0 : struct, IComponent
        {
            var eq = new EntityQuery(typeof(T0));
            var archetypes = manager.GetArchetypes(eq);

            foreach (var archetype in archetypes)
            {
                T0[] comps = manager.GetComponentArray<T0>(archetype);

                for (int i = 0; i < manager.GetArchetypeSize(archetype); i++)
                {
                    action(ref comps[i]);
                }
            }
        }

        public void ForEach<T0, T1>(ComponentActionDelegate<T0, T1> action)
            where T0 : struct, IComponent
            where T1 : struct, IComponent
        {
            var eq = new EntityQuery(typeof(T0), typeof(T1));
            var archetypes = manager.GetArchetypes(eq);

            foreach (var archetype in archetypes)
            {
                T0[] c1s = manager.GetComponentArray<T0>(archetype);
                T1[] c2s = manager.GetComponentArray<T1>(archetype);

                for (int i = 0; i < manager.GetArchetypeSize(archetype); i++)
                {
                    action(ref c1s[i], ref c2s[i]);
                }
            }
        }

        public void ForEach<T0, T1, T2>(ComponentActionDelegate<T0, T1, T2> action)
            where T0 : struct, IComponent
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            var eq = new EntityQuery(typeof(T0), typeof(T1), typeof(T2));
            var archetypes = manager.GetArchetypes(eq);

            foreach (var archetype in archetypes)
            {
                T0[] c1s = manager.GetComponentArray<T0>(archetype);
                T1[] c2s = manager.GetComponentArray<T1>(archetype);
                T2[] c3s = manager.GetComponentArray<T2>(archetype);

                for (int i = 0; i < manager.GetArchetypeSize(archetype); i++)
                {
                    action(ref c1s[i], ref c2s[i], ref c3s[i]);
                }
            }
        }

        public void ForEach<T0, T1, T2, T3>(ComponentActionDelegate<T0, T1, T2, T3> action)
            where T0 : struct, IComponent
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var eq = new EntityQuery(typeof(T0), typeof(T1), typeof(T2), typeof(T3));
            var archetypes = manager.GetArchetypes(eq);

            foreach (var archetype in archetypes)
            {
                T0[] c1s = manager.GetComponentArray<T0>(archetype);
                T1[] c2s = manager.GetComponentArray<T1>(archetype);
                T2[] c3s = manager.GetComponentArray<T2>(archetype);
                T3[] c4s = manager.GetComponentArray<T3>(archetype);

                for (int i = 0; i < manager.GetArchetypeSize(archetype); i++)
                {
                    action(ref c1s[i], ref c2s[i], ref c3s[i], ref c4s[i]);
                }
            }
        }

        private EntityManager manager;
    }
}
