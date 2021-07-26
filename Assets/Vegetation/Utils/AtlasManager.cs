using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class Atlas
{
	public class AtlasPageDescriptor
	{
		public Atlas atlas;
		public Vector2 tl;
		public int size;
        public Vector4 pageDescriptorToGPU;

        public AtlasPageDescriptor(Atlas atlas, Vector2 tl, int size)
		{
			this.atlas = atlas;
			this.tl = tl;
			this.size = size;

            pageDescriptorToGPU = new Vector4(tl.x, tl.y, size, size);
        }

		public bool IsValid()
		{
			return atlas != null;
		}

		public void Release()
		{
			atlas.ReleasePage(this);
			atlas = null;
		}
	}

	public RenderTexture texture;

	private Stack<Vector2> freePages;
	public int FreePageCount
	{
		get
		{
			return freePages.Count;
		}
	}

	private int m_PageSize;
	public int PageSize
	{
		get
		{
			return m_PageSize;
		}
	}

	private int m_PageCount;
	public int PageCount
	{
		get
		{
			return m_PageCount;
		}
	}

	private int m_PageCountDim;

	private bool m_Linear;

    private string atlasName;


    public Atlas(RenderTextureFormat format, FilterMode filterMode, int res, int pageSize, bool linear, string name = "Atlas")
	{
		if (res < pageSize)
			throw new InvalidOperationException("Atlas size must fit at least one page.");

		Create(format, filterMode, res, pageSize, linear);

        atlasName = name;
    }

    private void Create(RenderTextureFormat format, FilterMode filterMode, int res, int pageSize, bool linear)
	{
        texture = new RenderTexture(res, res, 0, format, linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
		texture.enableRandomWrite = true;
		texture.autoGenerateMips = false;
		texture.filterMode = filterMode;
		texture.wrapMode = TextureWrapMode.Clamp;

		texture.Create();

		m_Linear = linear;
		m_PageSize = pageSize;
		m_PageCountDim = Mathf.FloorToInt(res / (float)m_PageSize);
		m_PageCount = m_PageCountDim * m_PageCountDim;

		freePages = new Stack<Vector2>(m_PageCount);
		for (int i = m_PageCountDim - 1; i >= 0; i--)
			for (int j = m_PageCountDim - 1; j >= 0; j--)
				freePages.Push(new Vector2(i, j) * m_PageSize);
    }

	public AtlasPageDescriptor GetPage()
	{
		if (freePages.Count == 0)
        {
            Debug.LogError(atlasName + " is empty.");
            return null;
        }

        Vector2 page = freePages.Pop();

		return new AtlasPageDescriptor(this, page, m_PageSize);
	}

	public void ReleasePage(AtlasPageDescriptor page)
	{
		if (page.atlas != this)
			throw new InvalidOperationException("Wrong atlas.");

		freePages.Push(page.tl);
	}

	public bool IsFull()
	{
		return freePages.Count == 0;
	}

	public void Reset()
	{
		RenderTextureFormat format = texture.format;
		FilterMode filterMode = texture.filterMode;
		int res = texture.width;
		Release();
		Create(format, filterMode, res, m_PageSize, m_Linear);
	}

	public void Release()
	{
		texture.Release();
		texture = null;
		freePages.Clear();
	}
}