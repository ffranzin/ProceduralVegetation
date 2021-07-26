
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Vegetation.Rendering
{

    [CustomEditor(typeof(GroundVegetationTexturing))]
    internal class GroundVegetationTexturingEditor : Editor
    {
        GroundVegetationTexturing script;

        private bool showTextureFields = false;

        GUIStyle textureWarning = new GUIStyle();

        SerializedProperty albedoMaps;
        SerializedProperty normalMaps;
        SerializedProperty specularMaps;
        SerializedProperty opacityMaps;
        SerializedProperty ambientOcclusionMaps;

        protected GUIStyle buttomHeaderStyle = new GUIStyle();

        public void OnEnable()
        {
            script = (GroundVegetationTexturing)target;

            albedoMaps = serializedObject.FindProperty("m_AlbedoMaps");
            normalMaps = serializedObject.FindProperty("m_NormalMaps");
            specularMaps = serializedObject.FindProperty("m_SpecularMaps");
            opacityMaps = serializedObject.FindProperty("m_OpacityMaps");
            ambientOcclusionMaps = serializedObject.FindProperty("m_AmbientOcclusion");

            if (albedoMaps == null || opacityMaps == null || specularMaps == null || normalMaps == null || ambientOcclusionMaps == null)
            {
                Debug.LogError("Invalid Property.");
                return;
            }

            buttomHeaderStyle.alignment = TextAnchor.MiddleCenter;
            buttomHeaderStyle.fontStyle = FontStyle.Bold;
        }



        private void TextureCheck()
        {
            for (int i = 0; i < albedoMaps.arraySize; i++)
            {
                SerializedProperty albedoMap = albedoMaps.GetArrayElementAtIndex(i);
                SerializedProperty normalMap = normalMaps.GetArrayElementAtIndex(i);
                SerializedProperty specularMap = specularMaps.GetArrayElementAtIndex(i);
                SerializedProperty opacityMap = opacityMaps.GetArrayElementAtIndex(i);

                if (albedoMap.objectReferenceValue == null || normalMap.objectReferenceValue == null || specularMap.objectReferenceValue == null || opacityMap.objectReferenceValue == null)
                {
                    GUILayout.Label($"Warning: Set all textures.", textureWarning);
                    return;
                }

                if (!((Texture2D)albedoMap.objectReferenceValue).isReadable || !((Texture2D)normalMap.objectReferenceValue).isReadable ||
                    !((Texture2D)specularMap.objectReferenceValue).isReadable || !((Texture2D)opacityMap.objectReferenceValue).isReadable)
                {
                    GUILayout.Label($"Warning: Set textures as Readable.", textureWarning);
                    return;
                }

                if (((Texture2D)normalMap.objectReferenceValue).GetPixel(0, 0).r < 0.99f) // != 1.0
                {
                    GUILayout.Label($"Warning: Set --{normalMap.objectReferenceValue.name}-- to NormalMap.", textureWarning);
                    return;
                }
            }
        }


        private void TexturesFields()
        {
            GUILayout.BeginVertical("Box");
            {
                GUILayout.BeginVertical("HelpBox");
                {
                    GUILayout.BeginHorizontal("HelpBox");
                    {
                        GUILayout.Label(" Albedo ", buttomHeaderStyle);
                        GUILayout.Label(" Normal ", buttomHeaderStyle);
                        GUILayout.Label("Opacity ", buttomHeaderStyle);
                        GUILayout.Label("Specular", buttomHeaderStyle);
                        GUILayout.Label("   AO   ", buttomHeaderStyle);
                    }
                    GUILayout.EndHorizontal();

                    for (int i = 0; i < albedoMaps.arraySize; i++)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button("-"))
                            {
                                albedoMaps.DeleteArrayElementAtIndex(i);
                                normalMaps.DeleteArrayElementAtIndex(i);
                                specularMaps.DeleteArrayElementAtIndex(i);
                                opacityMaps.DeleteArrayElementAtIndex(i);
                                ambientOcclusionMaps.DeleteArrayElementAtIndex(i);
                            }
                            else
                            {
                                SerializedProperty albedoMap = albedoMaps.GetArrayElementAtIndex(i);
                                albedoMap.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField(albedoMap.objectReferenceValue, typeof(Texture2D), false);

                                SerializedProperty normalMap = normalMaps.GetArrayElementAtIndex(i);
                                normalMap.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField(normalMap.objectReferenceValue, typeof(Texture2D), false);

                                SerializedProperty specularMap = specularMaps.GetArrayElementAtIndex(i);
                                specularMap.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField(specularMap.objectReferenceValue, typeof(Texture2D), false);

                                SerializedProperty opacityMap = opacityMaps.GetArrayElementAtIndex(i);
                                opacityMap.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField(opacityMap.objectReferenceValue, typeof(Texture2D), false);

                                SerializedProperty ambientOcclusionMap = ambientOcclusionMaps.GetArrayElementAtIndex(i);
                                ambientOcclusionMap.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField(ambientOcclusionMap.objectReferenceValue, typeof(Texture2D), false);
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("New PBR Texture"))
                {
                    albedoMaps.InsertArrayElementAtIndex(albedoMaps.arraySize);
                    albedoMaps.GetArrayElementAtIndex(albedoMaps.arraySize - 1).objectReferenceValue = null;

                    normalMaps.InsertArrayElementAtIndex(normalMaps.arraySize);
                    normalMaps.GetArrayElementAtIndex(normalMaps.arraySize - 1).objectReferenceValue = null;

                    specularMaps.InsertArrayElementAtIndex(specularMaps.arraySize);
                    specularMaps.GetArrayElementAtIndex(specularMaps.arraySize - 1).objectReferenceValue = null;

                    opacityMaps.InsertArrayElementAtIndex(opacityMaps.arraySize);
                    opacityMaps.GetArrayElementAtIndex(opacityMaps.arraySize - 1).objectReferenceValue = null;

                    ambientOcclusionMaps.InsertArrayElementAtIndex(ambientOcclusionMaps.arraySize);
                    ambientOcclusionMaps.GetArrayElementAtIndex(ambientOcclusionMaps.arraySize - 1).objectReferenceValue = null;
                }

                GUILayout.EndVertical();

                TextureCheck();
            }
            GUILayout.EndVertical();
        }


        public override void OnInspectorGUI()
        {
            TexturesFields();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(target);

                EditorSceneManager.MarkAllScenesDirty();
            }
        }
    }
#endif
}