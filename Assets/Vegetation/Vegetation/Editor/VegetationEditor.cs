
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Vegetation.Rendering
{

    [CustomEditor(typeof(VegetationLayerDescriptor))]
    internal class VegetationEditor : Editor
    {
        VegetationLayerDescriptor script;

        bool showAdaptabilityFields = false;
        bool showNoiseScaleVariables = false;
        bool showRendererFields = false;

        protected GUIStyle buttomHeaderStyle = new GUIStyle();
        protected GUIStyle rendererHeaderStyle = new GUIStyle();

        private SerializedObject serializedObject;


        public void OnEnable()
        {
            script = (VegetationLayerDescriptor)target;

            buttomHeaderStyle.alignment = TextAnchor.MiddleCenter;
            buttomHeaderStyle.fontStyle = FontStyle.Bold;

            rendererHeaderStyle.alignment = TextAnchor.MiddleCenter;
            rendererHeaderStyle.fontStyle = FontStyle.Bold;
        }

        protected void Slider(string text, ref float value, float min, float max)
        {
            value = EditorGUILayout.Slider(text, value, min, max);
        }


        public override void OnInspectorGUI()
        {
            VegetationCustomGUI();

            SaveAsset();
        }


        private void AdaptabilityCurvesFields()
        {
            SerializedProperty curves = serializedObject.FindProperty("modelsRenderer");

            unsafe
            {
                GUILayout.BeginVertical("Box");
                {
                    if (GUILayout.Button("ADAPTABILITY", buttomHeaderStyle))
                    {
                        showAdaptabilityFields = !showAdaptabilityFields;
                    }
                    if (showAdaptabilityFields)
                    {
                        GUILayout.BeginVertical("HelpBox");
                        {
                            GUILayout.Label("(*) - not implemented yet.");
                            script.layerDescriptor.adaptability[FeaturesIndex.GROUND_INDEX] = EditorGUILayout.Slider("Ground", script.layerDescriptor.adaptability[FeaturesIndex.GROUND_INDEX], 0f, 1f);
                            script.layerDescriptor.adaptability[FeaturesIndex.FOREST_INDEX] = EditorGUILayout.Slider("Forest", script.layerDescriptor.adaptability[FeaturesIndex.FOREST_INDEX], 0f, 1f);
                            script.layerDescriptor.adaptability[FeaturesIndex.MOISTURE_INDEX] = EditorGUILayout.Slider("Moisture (*)", script.layerDescriptor.adaptability[FeaturesIndex.MOISTURE_INDEX], 0f, 1f);
                            script.layerDescriptor.adaptability[FeaturesIndex.PRECIPITATION_INDEX] = EditorGUILayout.Slider("Precipitation (*)", script.layerDescriptor.adaptability[FeaturesIndex.PRECIPITATION_INDEX], 0f, 1f);
                            script.layerDescriptor.adaptability[FeaturesIndex.HEIGHT_INDEX] = EditorGUILayout.Slider("Hight (*)", script.layerDescriptor.adaptability[FeaturesIndex.HEIGHT_INDEX], 0f, 1f);
                            script.layerDescriptor.adaptability[FeaturesIndex.SLOPE_INDEX] = EditorGUILayout.Slider("Slope (*)", script.layerDescriptor.adaptability[FeaturesIndex.SLOPE_INDEX], 0f, 1f);
                        }
                        GUILayout.EndVertical();
                    }
                }
                GUILayout.EndVertical();
            }
        }


        private void RendererFields()
        {
            script.NormalizeModelsPlacementPercentage();

            SerializedProperty renderers = serializedObject.FindProperty("m_Plants");
            SerializedProperty renderersProbability = serializedObject.FindProperty("m_PlantsPlacementProbability");

            if (renderers == null || renderersProbability == null)
            {
                Debug.LogError("Invalid Property.");
                return;
            }

            GUILayout.BeginVertical("Box");
            {
                if (GUILayout.Button("RENDERERS", buttomHeaderStyle))
                {
                    showRendererFields = !showRendererFields;
                }
                if (showRendererFields)
                {
                    GUILayout.BeginHorizontal("HelpBox");
                    {
                        GUILayout.Label("RENDERER", rendererHeaderStyle);
                        GUILayout.Label("PROBABILITY", rendererHeaderStyle);
                    }
                    GUILayout.EndHorizontal();

                    for (int i = 0; i < renderers.arraySize; i++)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(i.ToString());

                            if (GUILayout.Button("-"))
                            {
                                renderers.DeleteArrayElementAtIndex(i);
                                renderersProbability.DeleteArrayElementAtIndex(i);
                            }
                            else
                            {
                                SerializedProperty r = renderers.GetArrayElementAtIndex(i);
                                r.objectReferenceValue = (PlantDescriptor)EditorGUILayout.ObjectField(r.objectReferenceValue, typeof(PlantDescriptor), false);

                                SerializedProperty rp = renderersProbability.GetArrayElementAtIndex(i);
                                rp.intValue = EditorGUILayout.IntSlider(rp.intValue, 0, 100);
                            }
                        }
                        GUILayout.EndHorizontal();
                    }

                    Space();

                    if (renderers.arraySize < VegetationConstants.MAX_MODELS_ALLOWED && GUILayout.Button("+"))
                    {
                        renderers.InsertArrayElementAtIndex(renderers.arraySize);
                        renderers.GetArrayElementAtIndex(renderers.arraySize - 1).objectReferenceValue = null;

                        renderersProbability.InsertArrayElementAtIndex(renderersProbability.arraySize);
                    }
                }

                GUILayout.EndVertical();
            }
        }


        private void NoiseScaleFields()
        {
            GUILayout.BeginVertical("Box");
            {
                if (GUILayout.Button("NOISE PLACEMENT", buttomHeaderStyle))
                {
                    showNoiseScaleVariables = !showNoiseScaleVariables;
                }
                if (showNoiseScaleVariables)
                {
                    GUILayout.BeginVertical("HelpBox");
                    {
                        script.layerDescriptor.octaves = EditorGUILayout.IntSlider("Octaves ", script.layerDescriptor.octaves, 1, 6);
                        Slider("Frequency", ref script.layerDescriptor.frequency, 0, 5);
                        Slider("Amplitude", ref script.layerDescriptor.amplitude, 0, 5);
                        Slider("Gain", ref script.layerDescriptor.gain, 0, 5);
                        Slider("Lacunarity", ref script.layerDescriptor.lacunarity, 0, 5);

                        Slider("Border Offset", ref script.layerDescriptor.selfDistance, 0.01f, script.layerDescriptor.placementDistance);
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndVertical();
        }


        public void VegetationCustomGUI()
        {
            serializedObject = new SerializedObject(script);

            Space();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Vegetation Cover");

                unsafe
                {
                    script.layerDescriptor.vegetationCover = (int)(VegetationCover)EditorGUILayout.EnumPopup((VegetationCover)script.layerDescriptor.vegetationCover);
                }
            }
            GUILayout.EndHorizontal();


            AdaptabilityCurvesFields();
            Space();

            NoiseScaleFields();
            Space();

            RendererFields();
            Space();

            serializedObject.ApplyModifiedProperties();
        }

        public void SaveAsset()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(target);

                EditorSceneManager.MarkAllScenesDirty();
            }
        }

        protected void Space(int n = 2)
        {
            for (int i = 0; i < n; i++)
            {
                EditorGUILayout.Space();
            }
        }
    }
#endif
}