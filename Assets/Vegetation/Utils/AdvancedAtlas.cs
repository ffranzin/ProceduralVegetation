
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AtlasUtils
{
    public static float GetClosestMultipleOf(float value, float multiple)
    {
        float rest = (value % multiple) / multiple;

        if (rest > 0.5f)
        {
            return Mathf.Floor(value / multiple) * multiple + multiple;
        }
        return Mathf.Floor(value / multiple) * multiple;
    }

    public static float GetNextMultipleOf(float value, float multiple)
    {
        if ((value % multiple) == 0)
        {
            return value;
        }

        return Mathf.Floor(value / multiple) * multiple + multiple;
    }
}


public class AtlasMultiLevel
{
    public class AdvancedAtlasPageDescriptor
    {
        public AtlasMultiLevel Atlas { get; private set; }
        public Vector2 Pos { get; private set; }
        public int Size { get; private set; }

        public int Level { get; private set; }

        public Vector4 pageDescriptorToGPU;

        public AdvancedAtlasPageDescriptor(AtlasMultiLevel atlas, Vector2 pos, int size, int level)
        {
            this.Atlas = atlas;
            this.Pos = pos;
            this.Size = size;
            this.Level = level;
            pageDescriptorToGPU = new Vector4(pos.x, pos.y, size, level);
        }
    }

    public RenderTexture AtlasTexture { get; private set; }
    
    private List<Stack<AdvancedAtlasPageDescriptor>> freePages = new List<Stack<AdvancedAtlasPageDescriptor>>();
    private List<(int pageResolution, int pageCounter)> atlasSetting;


    public AtlasMultiLevel(RenderTextureFormat format, FilterMode filterMode, bool linear, List<(int pageResolution, int pageCounter)> atlasSetting)
    {
        int width = 0;
        int texMaxSize = SystemInfo.maxTextureSize;

        for (int i = 0; i < atlasSetting.Count; i++)
        {
            freePages.Add(new Stack<AdvancedAtlasPageDescriptor>());

            //we assume that it is possible to have a empty levels
            if (atlasSetting[i].pageCounter == 0)
            {
                continue;
            }

            int pageRes = Mathf.NextPowerOfTwo(atlasSetting[i].pageResolution);
            int pageCounter = (int)AtlasUtils.GetNextMultipleOf((float)atlasSetting[i].pageCounter, texMaxSize / pageRes);

            InitializePagesAtLevel(i, pageCounter, pageRes, width);

            width += (pageCounter / (texMaxSize / pageRes))/*Collumns*/ * pageRes;
        }

        Create(format, filterMode, width, SystemInfo.maxTextureSize, linear);

        this.atlasSetting = atlasSetting;
    }

    protected void Create(RenderTextureFormat format, FilterMode filterMode, int width, int height, bool linear)
    {
        if (height > SystemInfo.maxTextureSize || width > SystemInfo.maxTextureSize)
        {
            Debug.LogError("Unsupported RenderTexture resolution.");
            return;
        }

        AtlasTexture = new RenderTexture(width, height, 0, format, linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
        AtlasTexture.enableRandomWrite = true;
        AtlasTexture.autoGenerateMips = false;
        AtlasTexture.filterMode = filterMode;
        AtlasTexture.wrapMode = TextureWrapMode.Clamp;
        AtlasTexture.Create();
    }


    public void CreateNewPagesAtLevel(int atlasLevel, bool copyContent = true)
    {
        int pageRes = Mathf.NextPowerOfTwo(atlasSetting[atlasLevel].pageResolution);
        int pageCounter = Mathf.RoundToInt(SystemInfo.maxTextureSize / pageRes);

        atlasSetting[atlasLevel] = (atlasSetting[atlasLevel].pageResolution, atlasSetting[atlasLevel].pageCounter + pageCounter);

        InitializePagesAtLevel(atlasLevel, pageCounter, pageRes, AtlasTexture.width);

        RenderTexture atlas = AtlasTexture;

        Create(atlas.format, atlas.filterMode, atlas.width + pageRes, SystemInfo.maxTextureSize, false);

        if (copyContent)
        {
            Graphics.CopyTexture(atlas, 0, 0, 0, 0, atlas.width, atlas.height, AtlasTexture, 0, 0, 0, 0);
        }

        atlas.Release();
        atlas = null;
        Debug.Log($"Positions buffer Size {AtlasTexture.width}x{AtlasTexture.height}  " +
                 $"({(AtlasTexture.width * AtlasTexture.height * sizeof(short) / 1024f / 1024f)}MB)");
    }


    private void InitializePagesAtLevel(int atlasLevel, int pageCounter, int pageRes, int startAtWidth)
    {
        int collumns = Mathf.RoundToInt(pageCounter / (SystemInfo.maxTextureSize / pageRes));
        int rows = Mathf.RoundToInt(SystemInfo.maxTextureSize / pageRes);

        Stack<AdvancedAtlasPageDescriptor> pages = freePages[atlasLevel];

        for (int i = 0; i < collumns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Vector2 pos = new Vector2(startAtWidth + i * pageRes, j * pageRes);

                pages.Push(new AdvancedAtlasPageDescriptor(this, pos, pageRes, atlasLevel));
            }
        }
    }


    public AdvancedAtlasPageDescriptor GetPageAtLevel(int level)
    {
        if (freePages == null || freePages.Count == 0 || freePages[level] == null || freePages[level].Count == 0)
        {
            CreateNewPagesAtLevel(level);
        }

        return freePages[level].Pop();
    }


    public void ReleasePage(AdvancedAtlasPageDescriptor page)
    {
        if (page != null && freePages != null && freePages[page.Level] != null)
        {
            freePages[page.Level].Push(page);
        }
    }


    public bool IsLevelEmpty(int level)
    {
        return freePages == null || freePages[level] == null || freePages[level].Count == 0;
    }


    public int FreePageCountAtLevel(int level)
    {
        if (freePages == null || freePages[level] == null)
        {
            return 0;
        }

        return freePages[level].Count;
    }

    public void Reset()
    {
        Release();

        AtlasTexture = new RenderTexture(AtlasTexture.descriptor);
    }

    public virtual void Release()
    {
        AtlasTexture?.Release();
        AtlasTexture = null;
    }

}
