using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace ECS
{
    public class EntityManager
    {
        public EntityManager()
        {
            entities = new List<(Archetype, int)>();
            available_ids = new List<int>();
            archetype_stores = new Dictionary<Archetype, ArchetypeStorage>();
            systems = new List<ComponentSystem>();
            createEntityQueue = new ConcurrentQueue<(Func<Archetype, Entity>, Archetype)>();
            takeEntityQueue = new ConcurrentQueue<(Action<Entity>, Entity)>();
            running = true;
            haltRequest = false;
        }

        public Entity CreateEntity(Archetype archetype)
        {
            archetype.Signature.Add(typeof(Entity));

            if (!archetype_stores.ContainsKey(archetype))
                archetype_stores.Add(archetype, new ArchetypeStorage(archetype));
            
            if (available_ids.Count == 0)
            {
                int index = archetype_stores[archetype].AddEntity();
                entities.Add((archetype, index));
                Entity e = new Entity(entities.Count - 1);
                archetype_stores[archetype].SetComponentData(e, index);
                return e;
            }
            else
            {
                int index = archetype_stores[archetype].AddEntity();
                int new_id = available_ids.Last();
                available_ids.RemoveAt(available_ids.Count - 1);
                entities[new_id] = (archetype, index);
                Entity e = new Entity(new_id);
                archetype_stores[archetype].SetComponentData(e, index);
                return e;
            }
        }

        // Optimization target.
        public List<Entity> CreateEntity(Archetype archetype, int num_entities)
        {
            var entities = new List<Entity>(num_entities);
            for (int i = 0; i < num_entities; i++)
                entities[i] = CreateEntity(archetype);
            return entities;
        }

        public void DeleteEntity(Entity e)
        {
            RemoveEntityFromArchetype(e);
            available_ids.Add(e.Id);
            entities[e.Id] = (new Archetype(), -1);
        }

        public void AddComponent<T>(Entity e)
            where T : struct, IComponent
        {
            var old_index = entities[e.Id].arch_index;
            var old_arch = entities[e.Id].archetype;
            var new_arch = old_arch;
            new_arch.Signature.Add(typeof(T));

            ArchetypeStorage storage;

            if (archetype_stores.ContainsKey(new_arch))
            {
                storage = archetype_stores[new_arch];
            }
            else
            {
                storage = new ArchetypeStorage(new_arch);
                archetype_stores.Add(new_arch, storage);
            }

            int new_index = storage.AddEntity();
            CopyComponents(old_arch, new_arch, old_index, new_index);
            RemoveEntityFromArchetype(e);
            entities[e.Id] = (new_arch, new_index);
        }

        public void SetComponentData<T>(T component, Entity e)
            where T : struct, IComponent
        {
            var arch = entities[e.Id].archetype;

            if (!arch.Contains(typeof(T)))
                throw new EcsException("Entity " + e.Id + " does not have component " + typeof(T));

            archetype_stores[arch].SetComponentData(component, entities[e.Id].arch_index);
        }

        public void RemoveComponent<T>(Entity e)
        {

        }

        public T GetComponentData<T>(Entity e)
            where T : struct, IComponent
        {
            var arch = entities[e.Id].archetype;
            var index = entities[e.Id].arch_index;

            return archetype_stores[arch].GetComponent<T>(index);
        }

        public ref T GetComponent<T>(Entity e)
            where T : struct, IComponent
        {
            var arch = entities[e.Id].archetype;
            var index = entities[e.Id].arch_index;

            return ref archetype_stores[arch].GetComponentRef<T>(index);
        }

        public void RegisterSystem<T>()
            where T : ComponentSystem, new()
        {
            T system = new T();
            system.SetManager(this);
            system.Initialize();
            systems.Add(system);
        }

        public void RegisterSystem<T>(T system)
            where T : ComponentSystem
        {
            system.SetManager(this);
            system.Initialize();
            systems.Add(system);
        }

        public bool UpdateSystems(GameTime gt)
        {
            foreach (var system in systems)
            {
                system.OnUpdate(gt);
            }

            while (createEntityQueue.Any())
            {
                (Func<Archetype, Entity>, Archetype) item;
                createEntityQueue.TryDequeue(out item);

                var f = item.Item1;
                var a = item.Item2;
                f(a);
            }

            while (takeEntityQueue.Any())
            {
                (Action<Entity>, Entity) item;
                takeEntityQueue.TryDequeue(out item);

                var f = item.Item1;
                var e = item.Item2;
                f(e);
            }

            if (haltRequest)
                running = false;

            return running;
        }

        public Archetype CreateArchetype(params Type[] componentTypes)
        {
            return new Archetype(componentTypes);
        }

        public List<Archetype> GetArchetypes(EntityQuery eq)
        {
            var result = new List<Archetype>();
            foreach (var archetype in archetype_stores.Keys)
            {
                if(eq.Matches(archetype))
                {
                    result.Add(archetype);
                }
            }
            return result;
        }

        public int GetArchetypeSize(Archetype archetype)
        {
            return archetype_stores[archetype].Size;
        }

        public T[] GetComponentArray<T>(Archetype arch)
            where T : struct, IComponent
        {
            return archetype_stores[arch].GetComponentArray<T>();
        }

        public void RequestAction(Func<Archetype, Entity> f, Archetype a)
        {
            createEntityQueue.Enqueue((f, a));
        }

        public void RequestAction(Action<Entity> f, Entity e)
        {
            takeEntityQueue.Enqueue((f, e));
        }

        public void RequestHalt()
        {
            haltRequest = true;
        }

        // Copies the components from a1[index1] to a2[index2]. a1 must be a subset of a2.
        private void CopyComponents(Archetype a1, Archetype a2, int index1, int index2)
        {
            var storage1 = archetype_stores[a1];
            var storage2 = archetype_stores[a2];

            ArchetypeStorage.CopyComponents(storage1, storage2, index1, index2);
        }

        private void RemoveEntityFromArchetype(Entity e)
        {
            var arch = entities[e.Id].archetype;
            var index = entities[e.Id].arch_index;
            var storage = archetype_stores[arch];

            if (index != storage.Size - 1)
            {
                var swap_index = storage.Size - 1;
                var swap_id = storage.GetComponent<Entity>(swap_index).Id;
                // Copy comps from storage.Size - 1 to index.
                storage.SwapComponents(index, swap_index);
                storage.Pop();
                entities[swap_id] = (arch, index);
            }
            else
            {
                storage.Pop();
            }
        }

        private List<(Archetype archetype, int arch_index)> entities;
        private List<int> available_ids;
        private Dictionary<Archetype, ArchetypeStorage> archetype_stores;
        private List<ComponentSystem> systems;
        private ConcurrentQueue<(Func<Archetype, Entity>, Archetype)> createEntityQueue;
        private ConcurrentQueue<(Action<Entity>, Entity)> takeEntityQueue;
        private bool running;
        private bool haltRequest;
    }
}
