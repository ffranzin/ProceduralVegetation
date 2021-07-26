using System.Collections.Generic;
using UnityEngine;

namespace Vegetation.Rendering
{
    public interface IVegetationAreaRenderable
    {
        void Render(Atlas.AtlasPageDescriptor heightMap = null);

        void Release();
    }
}