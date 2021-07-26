
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Utils.Analysis
{
    public class PerformanceAnalysis : Analysis
    {
        private CustomSampler sampler;
        private string prefix = "";
        private string sufix = "";
        private bool enableCPUSampler;
        private bool enableGPUSampler;
        private bool allowEmptyTimer;

        private const double NANOSECONDS_TO_MILLISECONDS = 0.000001f;

        public PerformanceAnalysis(string logName, bool enableCPUSampler = true, bool enableGPUSampler = true, bool allowEmptyTimer = false) : base(logName)
        {
            //sampler = CustomSampler.Create(logName.Replace(".txt", ""), enableGPUSampler);
            //sampler.GetRecorder().enabled = true;
            
            //this.enableGPUSampler = enableGPUSampler;
            //this.enableCPUSampler = enableCPUSampler;
            //this.allowEmptyTimer = allowEmptyTimer;
        }

        public void Begin(string prefix = "", string sufix = "")
        {
            //sampler.Begin();
            //this.prefix = prefix;
            //this.sufix = sufix;
        }

        public void End()
        {
            //sampler.End();

            //const int unityProfilerDelay = 5;//used to synchronize with unity's profiler interface

            //if (enableCPUSampler && (allowEmptyTimer || sampler.GetRecorder().elapsedNanoseconds > 1) && enableGPUSampler && (allowEmptyTimer || sampler.GetRecorder().gpuElapsedNanoseconds > 1))
            //{
            //    AddData($"\n Frame:{Time.frameCount - unityProfilerDelay} {prefix}\t" + (sampler.GetRecorder().elapsedNanoseconds * NANOSECONDS_TO_MILLISECONDS).ToString("0.0000") + "\t" + (sampler.GetRecorder().gpuElapsedNanoseconds * NANOSECONDS_TO_MILLISECONDS).ToString("0.0000") + "  \t" + sufix);
            //}
            //else if(enableCPUSampler && (allowEmptyTimer || sampler.GetRecorder().elapsedNanoseconds > 1))
            //{
            //    AddData($"\n Frame:{Time.frameCount - unityProfilerDelay} {prefix}\t" + (sampler.GetRecorder().elapsedNanoseconds * NANOSECONDS_TO_MILLISECONDS).ToString("0.0000") + "  \t" + sufix);
            //}
            //else if (enableGPUSampler && (allowEmptyTimer || sampler.GetRecorder().gpuElapsedNanoseconds > 1))
            //{
            //    AddData($"\n Frame:{Time.frameCount - unityProfilerDelay} {prefix}\t" + (sampler.GetRecorder().gpuElapsedNanoseconds * NANOSECONDS_TO_MILLISECONDS).ToString("0.0000") + " \t" + sufix);
            //}
        }


        public override void SaveAnalysis()
        {
            outputData = outputData.Replace('.', ',');

            base.SaveAnalysis();
        }
    }
}