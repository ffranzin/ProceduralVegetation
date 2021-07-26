using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

namespace Utils.Analysis
{
    public class Analysis
    {
        protected string filename = "";
        protected string outputData = "";

        public Analysis(string filename)
        {
            this.filename = filename;
        }

        public virtual void SaveAnalysis()
        {
            string path = (Application.dataPath + "\\..\\Analysis");

            Directory.CreateDirectory(path);

            StreamWriter streamWriter = new StreamWriter(path + "\\" + filename);
            streamWriter.WriteLine(outputData);
            streamWriter.Close();
        }

        public void AddData(string data)
        {
            outputData += data;
        }

        public void CleanData()
        {
            outputData = "";
        }
    }
}