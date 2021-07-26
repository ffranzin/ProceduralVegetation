
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Vegetation.Rendering
{
    [CustomEditor(typeof(LayersLibrary))]
    internal class LayersLibraryEditor : Editor
    {
        private LayersLibrary script;

        private void OnEnable()
        {
            script = (LayersLibrary)target;
        }


        private void ShowVegetationLibrary()
        {
            if (script.Count > 0)
            {
                GUILayout.BeginVertical("HelpBox");
                {
                    EditorGUILayout.LabelField("Layers Library");

                    for (int i = 0; i < script.Count; i++)
                    {
                        EditorGUILayout.ObjectField(script.VegetationLayers[i], typeof(VegetationLayerDescriptor));
                    }
                }
                GUILayout.EndVertical();
            }
        }


        public override void OnInspectorGUI()
        {
            EditorGUILayout.ObjectField("Script:", MonoScript.FromScriptableObject((LayersLibrary)target), typeof(LayersLibrary), false);

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