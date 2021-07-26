using UnityEngine;

namespace Vegetation
{
    public interface IVegetationAreaCollectable
    {
        int Count { get; }
        Vector2[] Positions { get; }
        Vector3[] PositionsWithHeight { get; }

        void Release();
    }
}