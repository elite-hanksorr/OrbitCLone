using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS
{
    // ArchetypeStorage stores components belonging to a given Archetype.
    class ArchetypeStorage
    {
        // Create an ArchetypeStorage to store components based on the specified Archetype.
        public ArchetypeStorage(Archetype arch)
        {
            Size = 0;
            capacity = 64;
            component_arrays = new Dictionary<Type, Array>();
            archetype = arch;
            // Initialize storage.
            Initialize();
        }

        // Add an entity to the archetype. Its components will have default values.
        public int AddEntity()
        {
            if (Size == capacity) { Grow(); }

            // Return the index of the added entity.
            return Size++;
        }

        // Set the data of the specified component at the specified index.
        public void SetComponentData<T>(T comp, int index)
            where T : struct, IComponent
        {
            component_arrays[typeof(T)].SetValue(comp, index);
        }

        public T GetComponent<T>(int index)
            where T : struct, IComponent
        {
            T[] arr = (T[])component_arrays[typeof(T)];
            return arr[index];
        }

        public ref T GetComponentRef<T>(int index)
            where T : struct, IComponent
        {
            T[] arr = (T[])component_arrays[typeof(T)];
            return ref arr[index];
        }

        public T[] GetComponentArray<T>()
        {
            return (T[])component_arrays[typeof(T)];
        }

        public static void CopyComponents(ArchetypeStorage copy_from_storage, ArchetypeStorage copy_to_storage, int copy_from_index, int copy_to_index)
        {
            foreach(var kvp in copy_from_storage.component_arrays)
            {
                var item = kvp.Value.GetValue(copy_from_index);
                copy_to_storage.component_arrays[kvp.Key].SetValue(item, copy_to_index);
            }
        }

        public void SwapComponents(int index1, int index2)
        {
            foreach (var comp_array in component_arrays.Values)
            {
                var comp1 = comp_array.GetValue(index1);
                var comp2 = comp_array.GetValue(index2);
                comp_array.SetValue(comp2, index1);
                comp_array.SetValue(comp1, index2);
            }
        }

        public void Pop()
        {
            Size--;
        }

        // Returns index of entity which overwrote deleted entity or -1 if no swapping was needed.
        public int RemoveEntity(int index)
        {
            Size--;

            if(Size == index)
            {
                foreach(Array array in component_arrays.Values)
                {
                    Array.Clear(array, index, 1);
                }

                return -1;
            }

            foreach(Array array in component_arrays.Values)
            {
                var value = array.GetValue(Size);
                array.SetValue(value, index);
                Array.Clear(array, Size, 1);
            }

            return Size;
        }

        private void Initialize()
        {
            foreach(var type in archetype.Signature)
            {
                component_arrays.Add(type, Array.CreateInstance(type, capacity));
                component_arrays[type].Initialize();
            }
        }

        private void Grow()
        {
            capacity *= 2;
            var new_comp_arrays = new Dictionary<Type, Array>();
            foreach(var kvp in component_arrays)
            {
                Array old_array = kvp.Value;
                var new_array = Array.CreateInstance(kvp.Key, capacity);
                new_array.Initialize();
                Array.Copy(old_array, new_array, old_array.Length);
                new_comp_arrays.Add(kvp.Key, new_array);
            }
            component_arrays = new_comp_arrays;
        }

        private Archetype archetype;

        private Dictionary<Type, Array> component_arrays;

        public int Size { get; private set; }
        private int capacity;

    }
}
