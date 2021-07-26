using System.Collections.Generic;
using UnityEngine;
using Vegetation.Rendering;

namespace Vegetation
{
    public interface IVegetation
    {
        IVegetationAreaRenderable VegetationAreaRenderer(Bounds bounds, int heightmapAtlasPageDescriptor);
        IVegetationAreaCollectable VegetationAreaCollectable(Bounds bounds);
    }
}