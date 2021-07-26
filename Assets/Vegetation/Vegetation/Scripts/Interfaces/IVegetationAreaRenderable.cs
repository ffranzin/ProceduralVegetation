namespace Vegetation.Rendering
{
    public interface IVegetationAreaRenderable
    {
        void Render(Atlas.AtlasPageDescriptor heightMap = null);

        void Release();
    }
}