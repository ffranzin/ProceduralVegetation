using UnityEngine;


namespace Utils.Atlas
{
    public class AdvancedAtlas : AdvancedAtlasMultiResolution
    {
        private int pageResolution = 0;

        public AdvancedAtlas(RenderTextureFormat renderTextureFormat, FilterMode filterMode, RenderTextureReadWrite renderTextureReadWrite, int pageResolution, string name = "Default") : base(renderTextureFormat, filterMode, renderTextureReadWrite, name)
        {
            Debug.Assert(pageResolution > 0, "Page Resolution must be greater than zero.");

            this.pageResolution = pageResolution;
        }


        public AtlasPageDescriptor GetPage()
        {
            return GetPageResolution(pageResolution);
        }
    }
}