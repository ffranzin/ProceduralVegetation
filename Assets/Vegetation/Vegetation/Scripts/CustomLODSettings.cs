using System;
using System.Collections.Generic;
using UnityEngine;


namespace Vegetation.Rendering
{
    [Serializable]
    public class CustomLODSettings
    {
        public int LODCount { get { return LODDistances.Count; } }

        public float cullDistance;

        [Range(0, 100)] public List<float> LODDistances;

        public List<int> DiscretizeLODIntoSamples(int samplesCount)
        {
            List<int> lod = new List<int>();

            float step = 100f / (float)samplesCount;

            for (float i = 0; i < 100; i += step)
            {
                float lodRangeMin = i;
                float lodRangeMax = i + step;

                for (int j = 0; j < LODCount; j++)
                {
                    if ((lodRangeMin <= LODDistances[j] && lodRangeMax <= LODDistances[j]) || (Mathf.Approximately(lodRangeMin, LODDistances[j]) && Mathf.Approximately(lodRangeMax, LODDistances[j])))
                    {
                        lod.Add(j);
                        break;
                    }
                }
            }

            return lod;
        }
        
        public float GetAreaAtLODLevel(int LODLevel)
        {
            if (LODLevel >= LODDistances.Count || LODLevel < 0)
            {
                return 0;
            }

            float radius1 = LODLevel == 0 ? 0.0001f : (LODDistances[LODLevel - 1] * 0.01f * cullDistance);
            float radius2 = LODDistances[LODLevel] * 0.01f * cullDistance;

            return CircleConcentricArea(radius1, radius2);


            float CircleConcentricArea(float r1, float r2)
            {
                return Mathf.Abs((Mathf.PI * r1 * r1) - (Mathf.PI * r2 * r2));
            }
        }
    }
}