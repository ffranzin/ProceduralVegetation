using UnityEngine;

namespace Vegetation.InternalInterfaces
{
    internal interface ILibrary<T>
    {
        /// <summary>
        /// Quantidade de elementos na Library.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Indice de um determinado elemento na Library.
        /// </summary>
        int IndexOf(T item);

        /// <summary>
        /// Retorna um determinado elemento da Library conforme um indice.
        /// </summary>
        T Get(int index);
    }
    
    internal interface IGPULibrary<T> : ILibrary<T>
    {
        /// <summary>
        /// Envia os dados da Library para GPU.
        /// </summary>
        void UpdateLibraryOnGPU(Material material);

        /// <summary>
        /// Envia os dados da Library para GPU.
        /// </summary>
        void UpdateLibraryOnGPU(ComputeShader computeShader, int kernel);
    }
    

    internal interface ILibraryUpdate<T>
    {
        /// <summary>
        /// Adiciona um novo elemento na Library.
        /// </summary>
        void Add(T item);

        /// <summary>
        /// Remove um elemento da Library.
        /// </summary>
        bool Remove(T item);

        /// <summary>
        /// Atualiza um elemento dado um indice na Library.
        /// </summary>
        void SetElementAt(T item, int index);

        /// <summary>
        /// Reset da Library.
        /// </summary>
        void Clear();
    }
}