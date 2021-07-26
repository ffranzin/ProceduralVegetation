using System.Collections.Generic;
using UnityEngine;

namespace Utils.Atlas
{



    public class AdvancedAtlasMultiResolution
    {
        protected string atlasName;
        protected Dictionary<int, Queue<AtlasPageDescriptor>> atlasPages;
        protected RenderTextureFormat renderTextureFormat;
        protected FilterMode filterMode;
        protected RenderTextureReadWrite renderTextureReadWrite;


        private RenderTexture m_texture;
        public RenderTexture texture
        {
            get
            {
                return m_texture;
            }
        }

        private bool IsNullTexture => m_texture.height == 1;


        public AdvancedAtlasMultiResolution(RenderTextureFormat renderTextureFormat, FilterMode filterMode, RenderTextureReadWrite renderTextureReadWrite, string name = "Default")
        {
            this.renderTextureFormat = renderTextureFormat;
            this.filterMode = filterMode;
            this.renderTextureReadWrite = renderTextureReadWrite;

            atlasName = name;

            atlasPages = new Dictionary<int, Queue<AtlasPageDescriptor>>();

            m_texture = new RenderTexture(1, 1, 0, renderTextureFormat, renderTextureReadWrite);
            m_texture.enableRandomWrite = true;
            m_texture.autoGenerateMips = false;
            m_texture.filterMode = filterMode;
            m_texture.wrapMode = TextureWrapMode.Clamp;
            m_texture.Create();
        }


        public AtlasPageDescriptor GetPageResolution(int resolution)
        {
            if (!atlasPages.ContainsKey(resolution) || atlasPages[resolution].Count == 0)
            {
                AddPageResolution(resolution);
            }

            return atlasPages[resolution].Dequeue();
        }



        public void ReleasePage(AtlasPageDescriptor page)
        {
            if (atlasPages == null || page == null)
            {
                return;
            }

            if (!atlasPages.ContainsKey(page.size))
            {
                Debug.LogError("Something Wrong.");
                return;
            }

            atlasPages[page.size].Enqueue(page);
        }


        public void Release()
        {
            atlasPages = null;
            m_texture.Release();
            m_texture = null;
        }


        private void ResizeAtlas(int width)
        {
            RenderTexture auxAtlas = new RenderTexture(width, SystemInfo.maxTextureSize, 0, renderTextureFormat, renderTextureReadWrite);
            auxAtlas.enableRandomWrite = true;
            auxAtlas.autoGenerateMips = false;
            auxAtlas.filterMode = filterMode;
            auxAtlas.wrapMode = TextureWrapMode.Clamp;
            auxAtlas.Create();

            if (!IsNullTexture)
            {
                Graphics.CopyTexture(m_texture, 0, 0, 0, 0, m_texture.width, texture.height, auxAtlas, 0, 0, 0, 0);
            }

            m_texture.Release();
            m_texture = null;
            m_texture = auxAtlas;
        }


        private void AddPageResolution(int resolution)
        {
            ResizeAtlas(texture != null ? resolution + texture.width : resolution);

            if (!atlasPages.ContainsKey(resolution))
            {
                atlasPages.Add(resolution, new Queue<AtlasPageDescriptor>());
            }

            for (int i = 0; i < texture.height; i += resolution)
            {
                Vector2Int tl = new Vector2Int(texture.width - resolution, i);

                atlasPages[resolution].Enqueue(new AtlasPageDescriptor(this, tl, resolution));
            }
        }
    }
}