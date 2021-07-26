
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Vegetation.Rendering;

namespace Vegetation
{
    [CustomEditor(typeof(PlantDescriptor))]
    public class PlantDescriptorEditor : Editor
    {
        PlantDescriptor script;

        bool showLODVariables = false;
        bool showLOD_ShadowVariables = false;
        bool showDescriptorVariables = false;

        public GUIStyle buttomHeaderStyle = new GUIStyle();
        public GUIStyle rendererHeaderStyle = new GUIStyle();


        public void OnEnable()
        {
            script = (PlantDescriptor)target;

            buttomHeaderStyle.alignment = TextAnchor.MiddleCenter;
            buttomHeaderStyle.fontStyle = FontStyle.Bold;

            rendererHeaderStyle.alignment = TextAnchor.MiddleCenter;
            rendererHeaderStyle.fontStyle = FontStyle.Bold;
        }

        private void Toggle(string text, ref int value)
        {
            value = EditorGUILayout.Toggle(text, value == 1 ? true : false) == true ? 1 : 0;
        }


        private void LODFields(CustomLODSettings lod, int LODCount, string label, ref bool showVariables)
        {
            unsafe
            {
                GUILayout.BeginVertical("Box");
                {
                    if (GUILayout.Button(label, buttomHeaderStyle))
                    {
                        showVariables = !showVariables;
                    }
                    if (showVariables)
                    {
                        GUILayout.BeginVertical("HelpBox");
                        {
                            float maxCullDistance = VegetationSettings.GetVegetationViewDistance(script.vegetationCover);

                            lod.cullDistance = EditorGUILayout.Slider("CullDistance", lod.cullDistance, 0, maxCullDistance);

                            if (LODCount > 0)
                            {
                                lod.LODDistances = lod.LODDistances ?? new List<float>();

                                while (lod.LODDistances.Count > LODCount) lod.LODDistances.RemoveAt(lod.LODDistances.Count - 1);
                                while (lod.LODDistances.Count < LODCount) lod.LODDistances.Add(0);

                                for (int i = 0; i < LODCount; i++)
                                {
                                    lod.LODDistances[i] = EditorGUILayout.Slider("LOD " + i, lod.LODDistances[i], 0, 100);
                                    if (i > 0)
                                    {
                                        lod.LODDistances[i] = Mathf.Max(lod.LODDistances[i], lod.LODDistances[i - 1]);
                                    }
                                }
                                lod.LODDistances[lod.LODDistances.Count - 1] = 100;
                            }

                        }
                        GUILayout.EndVertical();
                    }
                }
                GUILayout.EndVertical();
            }
        }


        private void DescriptorFields()
        {
            GUILayout.BeginVertical("Box");
            {
                if (GUILayout.Button("Descriptor", buttomHeaderStyle))
                {
                    showDescriptorVariables = !showDescriptorVariables;
                }
                if (showDescriptorVariables)
                {
                    GUILayout.BeginVertical("HelpBox");
                    {
                        GUILayout.BeginVertical("Box");
                        {
                            EditorGUILayout.MinMaxSlider("Scale", ref script.descriptor.minScale, ref script.descriptor.maxScale, 0, 10f);
                            script.descriptor.minScale = Mathf.Clamp(EditorGUILayout.FloatField("Min scale", script.descriptor.minScale), 0, 10);
                            script.descriptor.maxScale = Mathf.Clamp(EditorGUILayout.FloatField("Max scale", script.descriptor.maxScale), 0, 10);
                        }
                        GUILayout.EndVertical();

                        script.descriptor.frustumCullingDistance = EditorGUILayout.Slider("Frustum Culling Distance", script.descriptor.frustumCullingDistance, 1, 16);

                        Toggle("Enable Render", ref script.descriptor.enableRender);
                        Toggle("Enable Shadow", ref script.descriptor.enableShadow);
                        Toggle("Debug LOD", ref script.descriptor.debugLOD);
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndVertical();
        }


        public override void OnInspectorGUI()
        {
            script.vegetationCover = (VegetationCover)EditorGUILayout.EnumPopup("Vegetation Cover", script.vegetationCover);

            if (script.vegetationCover != VegetationCover.NO_VEGETATION)
            {
                DescriptorFields();

                LODFields(script.geometryLOD, script.GetComponent<LODGroup>().lodCount, "Geometry LOD", ref showLODVariables);

                LODFields(script.shadowLOD, script.GetComponent<LODGroup>().lodCount, "Shadow LOD", ref showLOD_ShadowVariables);
            }
        }


        private void OnDisable()
        {
            if (!Application.isPlayer)
            {
                EditorUtility.SetDirty(script);

                EditorSceneManager.MarkAllScenesDirty();
            }
        }

    }
#endif
}