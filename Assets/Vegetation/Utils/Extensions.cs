using System.Collections.Generic;
using UnityEngine;

namespace Utils.Extensions
{
    public static class Extensions
    {
        public static bool ContainsXZ(this Bounds b, Vector3 pos)
        {
            return pos.x >= b.min.x && pos.z >= b.min.z && pos.x <= b.max.x && pos.z <= b.max.z;
        }

        /// <summary>
        /// Retorna a quantidade de bytes por formato de RenderTexture.
        /// </summary>
        public static int GetRequiredBytesPerPixel(this RenderTexture texture)
        {
            switch (texture.format)
            {
                case RenderTextureFormat.ARGBFloat:
                    return 16;
                case RenderTextureFormat.ARGBHalf:
                    return 8;
                case RenderTextureFormat.RGHalf:
                    return 4;
            }

            Debug.LogError("Invalid format");
            return 0;
        }


        /// <summary>
        /// Retorna o formato equivalente para um Texture.
        /// </summary>
        public static TextureFormat RenderTextureFormatToTextureFormat(this RenderTexture texture)
        {
            switch (texture.format)
            {
                case RenderTextureFormat.ARGBFloat:
                    return TextureFormat.RGBAFloat;
                case RenderTextureFormat.ARGBHalf:
                    return TextureFormat.RGBAHalf;
            }

            Debug.LogError("Invalid format");
            return 0;
        }


        public static void GetKernelAndThreadGroupSize(this ComputeShader compute, string kernelName, ref int kernel, ref uint[] threadGroup)
        {
            if (!compute.HasKernel(kernelName))
            {
                Debug.LogError("Invalid kernel or threadGroup out of bounds.");
                return;
            }

            threadGroup = null;
            threadGroup = new uint[3];

            kernel = compute.FindKernel(kernelName);
            compute.GetKernelThreadGroupSizes(kernel, out threadGroup[0], out threadGroup[1], out threadGroup[2]);
        }



        public static T RemoveAndGetItem<T>(this IList<T> list, int index)
        {
            if (index < 0 || index > list.Count)
                return default(T);

            T item = list[index];
            list.RemoveAt(index);
            return item;
        }



        public static string UsedMemory(this ComputeBuffer computebuffer)
        {
            float memory = Mathf.Max(0.01f, (computebuffer.count * computebuffer.stride) / 1024f / 1024f);

            return memory.ToString("0.00MB");
        }



        private static Vector2 minMax = new Vector2();
        private static Vector2 maxMin = new Vector2();
        public static bool IsContainedIn(this Rect rect, Rect other)
        {
            Vector2 min = other.min;
            Vector2 max = other.max;
            minMax.x = min.x; minMax.y = max.y;
            maxMin.x = max.x; maxMin.y = min.y;

            return (rect.Contains(min) && rect.Contains(max) && rect.Contains(minMax) && rect.Contains(maxMin));
        }
    }
}