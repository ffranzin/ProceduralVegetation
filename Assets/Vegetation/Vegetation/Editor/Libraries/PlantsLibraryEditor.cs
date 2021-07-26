
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Vegetation.Rendering
{
    [CustomEditor(typeof(PlantsLibrary))]
    internal class PlantsLibraryEditor : Editor
    {
        private PlantsLibrary script;

        private void OnEnable()
        {
            script = (PlantsLibrary)target;
        }


        private void ShowVegetationLibrary()
        {
            if (script.Count > 0)
            {
                GUILayout.BeginVertical("HelpBox");
                {
                    EditorGUILayout.LabelField("Plants Library");

                    for (int i = 0; i < script.Count; i++)
                    {
                        EditorGUILayout.ObjectField(script.Get(i), typeof(LODGroup));
                    }
                }
                GUILayout.EndVertical();
            }
        }


        public override void OnInspectorGUI()
        {
            EditorGUILayout.ObjectField("Script:", MonoScript.FromScriptableObject((PlantsLibrary)target), typeof(PlantsLibrary), false);

            ShowVegetationLibrary();

            serializedObject.ApplyModifiedProperties();

            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(target);

                EditorSceneManager.MarkAllScenesDirty();
            }
        }
    }
}
#endif