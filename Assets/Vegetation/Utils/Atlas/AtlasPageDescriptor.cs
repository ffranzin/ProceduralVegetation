using UnityEngine;



namespace Utils.Atlas
{
    public class AtlasPageDescriptor
    {
        private AdvancedAtlasMultiResolution atlas;

        public RenderTexture texture => atlas.texture;

        public Vector2Int tl { get; private set; }
        public int size { get; private set; }
        public Vector4 pageDescriptorToGPU { get; private set; }

        public AtlasPageDescriptor(AdvancedAtlasMultiResolution atlas, Vector2Int tl, int size)
        {
            this.tl = tl;
            this.size = size;
            this.atlas = atlas;
            pageDescriptorToGPU = new Vector4(tl.x, tl.y, size, size);
        }

        public void Release()
        {
            if(atlas == null)
            {
                Debug.LogError("Something Wrong.");
            }

            atlas.ReleasePage(this);
        }
    }
}