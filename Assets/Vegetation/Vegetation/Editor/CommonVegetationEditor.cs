
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Vegetation.Rendering
{

    [CustomEditor(typeof(CommonVegetation))]
    internal class CommonVegetationEditor : VegetationEditor
    {
        CommonVegetation script;

        public void OnEnable()
        {
            script = (CommonVegetation)target;
            base.OnEnable();
        }


        public override void OnInspectorGUI()
        {
            EditorGUILayout.ObjectField("Script:", MonoScript.FromScriptableObject((CommonVegetation)target), typeof(CommonVegetation), false);

            VegetationCustomGUI();
            
            SaveAsset();
        }
    }
}
#endif