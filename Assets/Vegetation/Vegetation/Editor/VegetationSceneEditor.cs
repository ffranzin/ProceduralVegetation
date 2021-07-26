
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Vegetation.Rendering
{
    [CustomEditor(typeof(VegetationScene))]
    internal class VegetationSceneEditor : Editor
    {
        private VegetationScene script;
        private string path = "";

        private void OnEnable()
        {
            script = (VegetationScene)target;

            path = AssetDatabase.GetAssetPath(script);

            path = path.Remove(path.LastIndexOf('/') + 1);
        }


        private void VegetationLayerFields()
        {
            GUILayout.BeginVertical("HelpBox");
            {
                EditorGUILayout.LabelField("Vegetation Layers");

                for (int i = 0; i < script.VegetationLayers.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("-"))
                        {
                            script.VegetationLayers.RemoveAt(i);
                        }
                        else
                        {
                            script.VegetationLayers[i] = (VegetationLayerDescriptor)EditorGUILayout.ObjectField(script.VegetationLayers[i], typeof(VegetationLayerDescriptor), false);
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                if (script.VegetationLayers.Count < VegetationConstants.MAX_LAYER_COUNTER && GUILayout.Button("+"))
                {
                    script.VegetationLayers.Add(null);
                }
            }
            GUILayout.EndVertical();
        }


        public override void OnInspectorGUI()
        {
            EditorGUILayout.ObjectField("Script:", MonoScript.FromScriptableObject((VegetationScene)target), typeof(VegetationScene), false);

            VegetationLayerFields();

            GUILayout.BeginVertical("HelpBox");
            {
                GUILayout.BeginHorizontal("HelpBox");
                {
                    EditorGUILayout.LabelField("Plants Library");
                    EditorGUILayout.ObjectField(script.plantsLibrary, typeof(PlantsLibrary));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("HelpBox");
                {
                    EditorGUILayout.LabelField("Billboard Library");
                    EditorGUILayout.ObjectField(script.billboardsLibrary, typeof(BillboardsLibrary));
                }
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal("HelpBox");
                {
                    EditorGUILayout.LabelField("Layers Library");
                    EditorGUILayout.ObjectField(script.layersLibrary, typeof(LayersLibrary));
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            EditorGUILayout.LabelField("Path: " + path);

            if (GUILayout.Button("Save Libraries"))
            {
                SaveAssets();
            }

            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(target);

                EditorSceneManager.MarkAllScenesDirty();
            }
        }


        public void SaveAssets()
        {
            try
            {
                AssetDatabase.RemoveObjectFromAsset(script.plantsLibrary);
                AssetDatabase.RemoveObjectFromAsset(script.billboardsLibrary);
                AssetDatabase.RemoveObjectFromAsset(script.layersLibrary);
            }
            catch { }

            script.ExtractPlantsLibrary();
            script.ExtractBillboardLibrary();
            script.ExtractLayersLibrary();

            AssetDatabase.CreateAsset(script.plantsLibrary, path + "PlantsLibrary.asset");
            AssetDatabase.CreateAsset(script.billboardsLibrary, path + "BillboardsLibrary.asset");
            AssetDatabase.CreateAsset(script.layersLibrary, path + "LayersLibrary.asset");
        }
    }
}
#endif