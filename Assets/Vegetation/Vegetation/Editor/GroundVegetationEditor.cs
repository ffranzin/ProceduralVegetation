
#if UNITY_EDITOR
using UnityEditor;

namespace Vegetation.Rendering
{

    [CustomEditor(typeof(GroundVegetation))]
    internal class GroundVegetationEditor : VegetationEditor
    {
        private GroundVegetation script;

        public void OnEnable()
        {
            script = (GroundVegetation)target;
            base.OnEnable();
        }


        public override void OnInspectorGUI()
        {
            EditorGUILayout.ObjectField("Script:", MonoScript.FromScriptableObject((GroundVegetation)target), typeof(GroundVegetation), false);

            VegetationCustomGUI();

            SaveAsset();
        }
    }
}
#endif