
#if UNITY_EDITOR
using UnityEditor;

namespace Vegetation.Rendering
{

    [CustomEditor(typeof(SpeedTree))]
    internal class SpeedTreeEditor : VegetationEditor
    {
        SpeedTree script;

        public void OnEnable()
        {
            script = (SpeedTree)target;
            base.OnEnable();
        }


        public override void OnInspectorGUI()
        {
            EditorGUILayout.ObjectField("Script:", MonoScript.FromScriptableObject((SpeedTree)target), typeof(SpeedTree), false);

            VegetationCustomGUI();
            
            SaveAsset();
        }
    }
}
#endif