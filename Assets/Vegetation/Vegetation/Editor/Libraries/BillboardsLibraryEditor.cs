
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Vegetation.Rendering
{
    [CustomEditor(typeof(BillboardsLibrary))]
    internal class BillboardsLibraryEditor : Editor
    {
        private BillboardsLibrary script;
        private int ElementsCountPerCollumn = 2;
        private void OnEnable()
        {
            script = (BillboardsLibrary)target;
        }

        private void ShowBillboardLibrary()
        {
            if (script.Count > 0)
            {
                //TODO fix more textures than exists
                ElementsCountPerCollumn = EditorGUILayout.IntSlider("Collumns ", ElementsCountPerCollumn, 1, 5);

                GUILayout.BeginVertical("HelpBox");
                {
                    EditorGUILayout.LabelField("Billboards Library");

                    GUILayout.BeginHorizontal("HelpBox");
                    {
                        for (int i = 0; i < script.Count; i++)
                        {
                            if (i > 0 && i % ElementsCountPerCollumn == 0)
                            {
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal("HelpBox");
                            }

                            EditorGUILayout.ObjectField(script.Get(i).Albedo, typeof(Texture2D));
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
        }


        public override void OnInspectorGUI()
        {
            EditorGUILayout.ObjectField("Script:", MonoScript.FromScriptableObject((BillboardsLibrary)target), typeof(BillboardsLibrary), false);

            script.MipmapBias = EditorGUILayout.Slider("MipMap ", script.MipmapBias, -3, 3);

            script.MaxTextureSize = EditorGUILayout.IntSlider("Max Texture Size ", script.MaxTextureSize, 256, SystemInfo.maxTextureSize);

            ShowBillboardLibrary();

            serializedObject.ApplyModifiedProperties();

            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(target);

                EditorSceneManager.MarkAllScenesDirty();
            }
        }

        private void OnDisable()
        {
           // script.Release();
        }
    }
}
#endif