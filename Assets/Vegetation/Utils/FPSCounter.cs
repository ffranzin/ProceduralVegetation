using UnityEngine;
using Utils.Analysis;

namespace UnityStandardAssets.Utility
{
    public class FPSCounter : Singleton<FPSCounter>
    {
        private const float fpsMeasurePeriod = 0.25f;
        private int m_FpsAccumulator = 0;
        private float m_FpsNextPeriod = 0;
        private Analysis analisys;

        private int m_CurrentFps;
        public int currentFps => m_CurrentFps;


        private void Start()
        {
            m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;

            analisys = new Analysis("FPSCounter.txt");
        }


        private void Update()
        {
            m_FpsAccumulator++;
            if (Time.realtimeSinceStartup > m_FpsNextPeriod)
            {
                m_CurrentFps = (int)(m_FpsAccumulator / fpsMeasurePeriod);

                analisys.AddData($"Frame:{Time.frameCount}\t {m_CurrentFps} \n");

                m_FpsAccumulator = 0;
                m_FpsNextPeriod += fpsMeasurePeriod;
            }
        }

        private void OnDestroy()
        {
            analisys.SaveAnalysis();
        }

    }
}