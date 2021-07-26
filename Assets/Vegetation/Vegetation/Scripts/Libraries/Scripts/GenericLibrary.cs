using System.Collections.Generic;
using UnityEngine;
using Vegetation.InternalInterfaces;

namespace Vegetation.Utilities
{
    /// <summary>
    /// Classe generica para estruturar elementos da vegetacao, como modelos 3D e texturas.
    /// </summary>
    /// <remarks>
    /// O proposito principal, dentro do contexto da vegetação, é evitar duplicidade de dados. 
    /// Por exemplo, dois modelos de billboard que sao texturizados com a mesma textura causaria, 
    /// sem o auxilio de uma library, a duplicidade da textura em memoria.
    /// </remarks>
    internal class GenericLibrary<T> : ScriptableObject, ILibrary<T>, ILibraryUpdate<T>
    {
        protected List<T> library = new List<T>();

        public int Count => library == null ? 0 : library.Count;

        public virtual void Initialize() { }
        public virtual void Release() { }

        public virtual void Add(T item)
        {
            library = library ?? new List<T>();

            if (!library.Contains(item))
            {
                library.Add(item);
            }
        }


        public void Clear()
        {
            library?.Clear();
        }


        public int IndexOf(T item)
        {
            return library == null ? -1 : library.IndexOf(item);
        }


        public virtual bool Remove(T item)
        {
            return library == null ? false : library.Remove(item);
        }


        public void SetElementAt(T item, int index)
        {
            if (library != null && Count > index && !library.Contains(item))
            {
                library[index] = item;
            }
        }


        public T Get(int index)
        {
            if (library != null && Count > index)
            {
                return library[index];
            }

            return default(T);
        }
    }
}