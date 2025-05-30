﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using PWCommon5;
using Gaia.Internal;
using UnityEditorInternal;
using UnityEngine.Rendering;
using System.Linq;
using static Gaia.GaiaConstants;
#if ASSET_INVENTORY
using AssetInventory;
#endif
using System.IO;

namespace Gaia
{

    /// <summary>
    /// Editor for resource manager
    /// </summary>
    [CustomEditor(typeof(GaiaResource))]
    public class GaiaResourceEditor : PWEditor, IPWEditor
    {

        GUIStyle m_boxStyle = null;
        GUIStyle m_wrapStyle;
        GaiaResource m_resource = new GaiaResource();
        private DateTime m_lastSaveDT = DateTime.Now;
        private EditorUtils m_editorUtils = null;
        private bool[] m_resourceProtoFoldOutStatus;
        private bool[] m_resourceProtoMasksExpanded;
        private ReorderableList[] m_resourceProtoReorderableLists;
        private ImageMask[] m_maskListBeingDrawn;
        private CollisionMask[] m_collisionMaskListBeingDrawn;
        private int m_resourceIndexBeingDrawn;
        private int m_resourceMaskIndexBeingDrawn;

        private int GetResourceIndexFromPrototypeIndex(GaiaConstants.SpawnerResourceType resourceType, int prototypeIndex)
        {
            //We have the following Resource types in this order: Textures, Terrain Details, Trees, GameObjects
            //To get the resource index we need to add the amount of other resources on top of the prototype index
            switch (resourceType)
            {
                case GaiaConstants.SpawnerResourceType.TerrainTexture:
                    return prototypeIndex;
                case GaiaConstants.SpawnerResourceType.TerrainDetail:
                    return m_resource.m_texturePrototypes.Length + prototypeIndex;
                case GaiaConstants.SpawnerResourceType.TerrainTree:
                    return m_resource.m_detailPrototypes.Length + m_resource.m_texturePrototypes.Length + prototypeIndex;
                case GaiaConstants.SpawnerResourceType.GameObject:
                    return m_resource.m_gameObjectPrototypes.Length + m_resource.m_detailPrototypes.Length + m_resource.m_texturePrototypes.Length + prototypeIndex;
                default:
                    return prototypeIndex;
            }


        }

        private void DrawTextures(bool showHelp)
        {
            EditorGUI.indentLevel++;
            for (int textureProtoIndex = 0; textureProtoIndex < m_resource.m_texturePrototypes.Length; textureProtoIndex++)
            {
                int resourceIndex = GetResourceIndexFromPrototypeIndex(GaiaConstants.SpawnerResourceType.TerrainTexture, textureProtoIndex);

                m_resourceProtoFoldOutStatus[resourceIndex] = EditorGUILayout.Foldout(m_resourceProtoFoldOutStatus[resourceIndex], m_resource.m_texturePrototypes[textureProtoIndex].m_name);
                if (m_resourceProtoFoldOutStatus[resourceIndex])
                {
                    DrawTexturePrototype(m_resource.m_texturePrototypes[textureProtoIndex], m_editorUtils, showHelp);
                    if (m_editorUtils.Button("DeleteTexture"))
                    {
                        m_resource.m_texturePrototypes = GaiaUtils.RemoveArrayIndexAt<ResourceProtoTexture>(m_resource.m_texturePrototypes, textureProtoIndex);
                        m_resourceProtoFoldOutStatus = GaiaUtils.RemoveArrayIndexAt<bool>(m_resourceProtoFoldOutStatus, resourceIndex);
                        //Correct the index since we just removed one texture
                        textureProtoIndex--;
                    }
                }
            }
            EditorGUI.indentLevel--;
            if (m_editorUtils.Button("AddTexture"))
            {
                m_resource.m_texturePrototypes = GaiaUtils.AddElementToArray<ResourceProtoTexture>(m_resource.m_texturePrototypes, new ResourceProtoTexture());
                m_resource.m_texturePrototypes[m_resource.m_texturePrototypes.Length - 1].m_name = "New Texture Prototype";
                m_resourceProtoFoldOutStatus = GaiaUtils.AddElementToArray<bool>(m_resourceProtoFoldOutStatus, false);
            }
        }


        public static void DrawTexturePrototype(ResourceProtoTexture resourceProtoTexture, EditorUtils editorUtils, bool showHelp, bool CTSProfileConnected = false)
        {
            editorUtils.LabelField("TextureProtoHeadingLayerPrototype", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            resourceProtoTexture.m_name = editorUtils.TextField("TextureProtoName", resourceProtoTexture.m_name, showHelp);
#if SUBSTANCE_PLUGIN_ENABLED
            if (resourceProtoTexture.m_substanceMaterial == null)
            {
                resourceProtoTexture.m_texture = (Texture2D)editorUtils.ObjectField("TextureProtoTexture", resourceProtoTexture.m_texture, typeof(Texture2D), false, showHelp, GUILayout.MaxHeight(16));
                resourceProtoTexture.m_normal = (Texture2D)editorUtils.ObjectField("TextureProtoNormal", resourceProtoTexture.m_normal, typeof(Texture2D), false, showHelp, GUILayout.MaxHeight(16));
                resourceProtoTexture.m_maskmap = (Texture2D)editorUtils.ObjectField("TextureProtoMaskMap", resourceProtoTexture.m_maskmap, typeof(Texture2D), false, showHelp, GUILayout.MaxHeight(16));
            }
            else
            {
                EditorGUILayout.HelpBox(editorUtils.GetTextValue("SubstanceActiveHelp"), MessageType.Info);
                if (resourceProtoTexture.m_substanceMaterial.graphs.Count > 1)
                {
                    resourceProtoTexture.substanceSourceIndex = editorUtils.IntSlider("SubstanceGraphSelection", resourceProtoTexture.substanceSourceIndex, 1, resourceProtoTexture.m_substanceMaterial.graphs.Count, showHelp);
                }
                else
                {
                    resourceProtoTexture.substanceSourceIndex = 1;
                }
            }

            resourceProtoTexture.m_substanceMaterial = (Substance.Game.Substance)editorUtils.ObjectField("TextureProtoSubstance", resourceProtoTexture.m_substanceMaterial, typeof(Substance.Game.Substance), false, showHelp, GUILayout.MaxHeight(16));
#else
            resourceProtoTexture.m_texture = (Texture2D)editorUtils.ObjectField("TextureProtoTexture", resourceProtoTexture.m_texture, typeof(Texture2D), false, showHelp, GUILayout.MaxHeight(16));
#if ASSET_INVENTORY
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (GUILayout.Button(editorUtils.GetContent("SearchAssetInventoryButton")))
                {
                    ResultPickerUI  window = ResultPickerUI.Show(path => { resourceProtoTexture.m_texture = (Texture2D)LoadAssetFromPath(path); }, "Images");
                    window.instantSelection = false;
                    window.hideDetailsPane = false;
                }
            }
            EditorGUILayout.EndHorizontal();
#endif
            resourceProtoTexture.m_normal = (Texture2D)editorUtils.ObjectField("TextureProtoNormal", resourceProtoTexture.m_normal, typeof(Texture2D), false, showHelp, GUILayout.MaxHeight(16));
            resourceProtoTexture.m_maskmap = (Texture2D)editorUtils.ObjectField("TextureProtoMaskMap", resourceProtoTexture.m_maskmap, typeof(Texture2D), false, showHelp, GUILayout.MaxHeight(16));
#if CTS_PRESENT
            GUILayout.Space(5);
            EditorGUILayout.LabelField("CTS Profile Maps");
            EditorGUI.indentLevel++;
            resourceProtoTexture.m_CTSSmoothnessMap = (Texture2D)editorUtils.ObjectField("TextureProtoCTSSmoothness", resourceProtoTexture.m_CTSSmoothnessMap, typeof(Texture2D), false, showHelp, GUILayout.MaxHeight(16));
            resourceProtoTexture.m_CTSRoughnessMap = (Texture2D)editorUtils.ObjectField("TextureProtoCTSRoughness", resourceProtoTexture.m_CTSRoughnessMap, typeof(Texture2D), false, showHelp, GUILayout.MaxHeight(16));
            resourceProtoTexture.m_CTSHeightMap = (Texture2D)editorUtils.ObjectField("TextureProtoCTSHeight", resourceProtoTexture.m_CTSHeightMap, typeof(Texture2D), false, showHelp, GUILayout.MaxHeight(16));
            resourceProtoTexture.m_CTSAmbientOcclusionMap = (Texture2D)editorUtils.ObjectField("TextureProtoCTSAmbientOcclusion", resourceProtoTexture.m_CTSAmbientOcclusionMap, typeof(Texture2D), false, showHelp, GUILayout.MaxHeight(16));
            EditorGUI.indentLevel--;
            GUILayout.Space(5);
#endif
#endif
            if (CTSProfileConnected)
            {
                resourceProtoTexture.m_sizeX = editorUtils.FloatField("TextureProtoTileSize", resourceProtoTexture.m_sizeX, showHelp);
                resourceProtoTexture.m_sizeY = resourceProtoTexture.m_sizeX;
                resourceProtoTexture.m_normalScale = editorUtils.Slider("TextureProtoNormalScale", resourceProtoTexture.m_normalScale, 0f, 10f, showHelp);
                resourceProtoTexture.m_smoothness = editorUtils.Slider("TextureProtoOffsetSmoothness", resourceProtoTexture.m_smoothness, 0f, 5f, showHelp);
            }
            else
            {

                if (resourceProtoTexture.m_sizeX < 2 || resourceProtoTexture.m_sizeX % 1 != 0 || !Mathf.IsPowerOfTwo((int)resourceProtoTexture.m_sizeX) ||
                    resourceProtoTexture.m_sizeY < 2 || resourceProtoTexture.m_sizeY % 1 != 0 || !Mathf.IsPowerOfTwo((int)resourceProtoTexture.m_sizeY))
                {
                    EditorGUILayout.HelpBox(editorUtils.GetTextValue("TextureNotPowerOfTwo"), MessageType.Warning);
                }
                resourceProtoTexture.m_sizeX = editorUtils.DelayedFloatField("TextureProtoSizeX", resourceProtoTexture.m_sizeX, showHelp);
                resourceProtoTexture.m_sizeY = editorUtils.DelayedFloatField("TextureProtoSizeY", resourceProtoTexture.m_sizeY, showHelp);
                resourceProtoTexture.m_offsetX = editorUtils.FloatField("TextureProtoOffsetX", resourceProtoTexture.m_offsetX, showHelp);
                resourceProtoTexture.m_offsetY = editorUtils.FloatField("TextureProtoOffsetY", resourceProtoTexture.m_offsetY, showHelp);
                resourceProtoTexture.m_normalScale = editorUtils.Slider("TextureProtoNormalScale", resourceProtoTexture.m_normalScale, 0f, 10f, showHelp);

#if HDPipeline || UPPipeline
                //The color tint is encoded in the diffuse Remap Max RGB / XYZ
                resourceProtoTexture.m_diffuseRemapMax = editorUtils.ColorField("TextureProtoColorTint", resourceProtoTexture.m_diffuseRemapMax, showHelp);
                //The toggle for "Opacity as Density" is encoded in the alpha channel / w-axis for the diffuse Remap min Vector 4
                bool OaDIsChecked = resourceProtoTexture.m_diffuseRemapMin.w == 1.0f;
                OaDIsChecked = editorUtils.Toggle("TextureProtoOpacityAsDensity", OaDIsChecked, showHelp);
                if (OaDIsChecked)
                {
                    resourceProtoTexture.m_diffuseRemapMin.w = 1.0f;
                }
                else
                {
                    resourceProtoTexture.m_diffuseRemapMin.w = 0.0f;
                }

                resourceProtoTexture.m_channelRemapFoldedOut = editorUtils.Foldout(resourceProtoTexture.m_channelRemapFoldedOut, "TextureProtoChannelRemapFoldout", showHelp);
                if (resourceProtoTexture.m_channelRemapFoldedOut)
                {
                    EditorGUI.indentLevel++;
                    //For HDRP, we have 
                    //R:Metallic
                    editorUtils.MinMaxSliderWithFields("TextureProtoRemapMetallic", ref resourceProtoTexture.m_maskMapRemapMin.x, ref resourceProtoTexture.m_maskMapRemapMax.x, 0f, 1f, showHelp);
                    //G:AO
                    editorUtils.MinMaxSliderWithFields("TextureProtoRemapAO", ref resourceProtoTexture.m_maskMapRemapMin.y, ref resourceProtoTexture.m_maskMapRemapMax.y, 0f, 1f, showHelp);
                    //A:Smoothness
                    editorUtils.MinMaxSliderWithFields("TextureProtoRemapSmoothness", ref resourceProtoTexture.m_maskMapRemapMin.w, ref resourceProtoTexture.m_maskMapRemapMax.w, 0f, 1f, showHelp);
                    EditorGUI.indentLevel--;
                }
#else
                resourceProtoTexture.m_channelRemapFoldedOut = editorUtils.Foldout(resourceProtoTexture.m_channelRemapFoldedOut, "TextureProtoChannelRemapFoldout", showHelp);
                if (resourceProtoTexture.m_channelRemapFoldedOut)
                {
                    EditorGUI.indentLevel++;
                    //For Built-in, we have just the RGBA channels without any further description
                    //R:
                    editorUtils.MinMaxSliderWithFields("TextureProtoBuiltInRemapR", ref resourceProtoTexture.m_maskMapRemapMin.x, ref resourceProtoTexture.m_maskMapRemapMax.x, 0f, 1f, showHelp);
                    //G:
                    editorUtils.MinMaxSliderWithFields("TextureProtoBuiltInRemapG", ref resourceProtoTexture.m_maskMapRemapMin.y, ref resourceProtoTexture.m_maskMapRemapMax.y, 0f, 1f, showHelp);
                    //B:
                    editorUtils.MinMaxSliderWithFields("TextureProtoBuiltInRemapB", ref resourceProtoTexture.m_maskMapRemapMin.z, ref resourceProtoTexture.m_maskMapRemapMax.z, 0f, 1f, showHelp);
                    //A:
                    editorUtils.MinMaxSliderWithFields("TextureProtoBuiltInRemapA", ref resourceProtoTexture.m_maskMapRemapMin.w, ref resourceProtoTexture.m_maskMapRemapMax.w, 0f, 1f, showHelp);
                    EditorGUI.indentLevel--;
                }
                resourceProtoTexture.m_specularColor = editorUtils.ColorField("TextureProtoSpecularColor", resourceProtoTexture.m_specularColor, showHelp);
                resourceProtoTexture.m_metallic = editorUtils.Slider("TextureProtoOffsetMetallic", resourceProtoTexture.m_metallic, 0f, 1f, showHelp);
                resourceProtoTexture.m_smoothness = editorUtils.Slider("TextureProtoOffsetSmoothness", resourceProtoTexture.m_smoothness, 0f, 1f, showHelp);
#endif
            }
            EditorGUI.indentLevel--;
        }


        private void DrawGameObjects(bool showHelp)
        {
            EditorGUI.indentLevel++;
            for (int gameObjectProtoIndex = 0; gameObjectProtoIndex < m_resource.m_gameObjectPrototypes.Length; gameObjectProtoIndex++)
            {
                int resourceIndex = GetResourceIndexFromPrototypeIndex(GaiaConstants.SpawnerResourceType.TerrainTexture, gameObjectProtoIndex);

                m_resourceProtoFoldOutStatus[resourceIndex] = EditorGUILayout.Foldout(m_resourceProtoFoldOutStatus[resourceIndex], m_resource.m_gameObjectPrototypes[gameObjectProtoIndex].m_name);
                if (m_resourceProtoFoldOutStatus[resourceIndex])
                {
                    DrawGameObjectPrototype(m_resource.m_gameObjectPrototypes[gameObjectProtoIndex], m_editorUtils, showHelp);

                    Rect buttonRect = EditorGUILayout.GetControlRect();
                    buttonRect.x += 15 * EditorGUI.indentLevel;
                    buttonRect.width -= 15 * EditorGUI.indentLevel;
                    if (GUI.Button(buttonRect, m_editorUtils.GetContent("DeleteGameObject")))
                    {
                        m_resource.m_gameObjectPrototypes = GaiaUtils.RemoveArrayIndexAt<ResourceProtoGameObject>(m_resource.m_gameObjectPrototypes, gameObjectProtoIndex);
                        m_resourceProtoFoldOutStatus = GaiaUtils.RemoveArrayIndexAt<bool>(m_resourceProtoFoldOutStatus, resourceIndex);
                        //Correct the index since we just removed one texture
                        gameObjectProtoIndex--;
                    }

                    //Rect maskRect;
                    //m_resourceIndexBeingDrawn = gameObjectProtoIndex;
                    //if (m_resourceProtoMasksExpanded[resourceIndex])
                    //{
                    //    m_maskListBeingDrawn = m_resource.m_gameObjectPrototypes[gameObjectProtoIndex].m_imageMasks;
                    //    maskRect = EditorGUILayout.GetControlRect(true, m_resourceProtoReorderableLists[resourceIndex].GetHeight());
                    //    m_resourceProtoReorderableLists[resourceIndex].DoList(maskRect);
                    //}
                    //else
                    //{
                    //    int oldIndent = EditorGUI.indentLevel;
                    //    EditorGUI.indentLevel = 1;
                    //    m_resourceProtoMasksExpanded[resourceIndex] = EditorGUILayout.Foldout(m_resourceProtoMasksExpanded[resourceIndex], ImageMaskListEditor.PropertyCount("MaskSettings", m_resource.m_gameObjectPrototypes[gameObjectProtoIndex].m_imageMasks, m_editorUtils), true);
                    //    maskRect = GUILayoutUtility.GetLastRect();
                    //    EditorGUI.indentLevel = oldIndent;
                    //}
                }
            }
            EditorGUI.indentLevel--;
            if (m_editorUtils.Button("AddGameObject"))
            {
                m_resource.m_gameObjectPrototypes = GaiaUtils.AddElementToArray<ResourceProtoGameObject>(m_resource.m_gameObjectPrototypes, new ResourceProtoGameObject());
                m_resource.m_gameObjectPrototypes[m_resource.m_gameObjectPrototypes.Length - 1].m_name = "New Game Object Prototype";
                m_resourceProtoFoldOutStatus = GaiaUtils.AddElementToArray<bool>(m_resourceProtoFoldOutStatus, false);
            }
        }

        public static void DrawGameObjectPrototype(ResourceProtoGameObject resourceProtoGameObject, EditorUtils editorUtils, bool showHelp)
        {
            resourceProtoGameObject.m_name = editorUtils.TextField("GameObjectProtoName", resourceProtoGameObject.m_name, showHelp);
            EditorGUI.indentLevel++;
            resourceProtoGameObject.m_instancesFoldOut = editorUtils.Foldout("GameObjectInstances", resourceProtoGameObject.m_instancesFoldOut, showHelp);
            //Iterate through instances
            if (resourceProtoGameObject.m_instancesFoldOut)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < resourceProtoGameObject.m_instances.Length; i++)
                {
                    var instance = resourceProtoGameObject.m_instances[i];
                    instance.m_foldedOut = editorUtils.Foldout(instance.m_foldedOut, new GUIContent(instance.m_name), showHelp);
                    if (instance.m_foldedOut)
                    {
                        editorUtils.LabelField("GameObjectProtoHeadingPrefab", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        instance.m_name = editorUtils.TextField("GameObjectProtoName", instance.m_name, showHelp);
                        instance.m_desktopPrefab = (GameObject)editorUtils.ObjectField("GameObjectProtoInstanceDesktop", instance.m_desktopPrefab, typeof(GameObject), false, showHelp);
#if ASSET_INVENTORY
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(EditorGUIUtility.labelWidth);
                            if (GUILayout.Button(editorUtils.GetContent("SearchAssetInventoryButton")))
                            {
                                ResultPickerUI window = ResultPickerUI.Show(path => { instance.m_desktopPrefab = (GameObject)LoadAssetFromPath(path); }, "Prefabs");
                                window.instantSelection = false;
                                window.hideDetailsPane = false;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
#endif
                        //instance.m_mobilePrefab = (GameObject)editorUtils.ObjectField("GameObjectProtoInstanceMobile", instance.m_mobilePrefab, typeof(GameObject), false, showHelp);
                        instance.m_minInstances = editorUtils.IntField("GameObjectProtoInstanceMinInstances", instance.m_minInstances, showHelp);
                        instance.m_maxInstances = editorUtils.IntField("GameObjectProtoInstanceMaxInstances", instance.m_maxInstances, showHelp);
                        //Display failure rate as Probability and with %
                        instance.m_failureRate = 1f - editorUtils.Slider("GameObjectProtoInstanceProbabilityRate", (1f - instance.m_failureRate) * 100, 0, 100f, showHelp) / 100f;
                        EditorGUI.indentLevel--;
                        editorUtils.LabelField("GameObjectProtoHeadingOffset", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        editorUtils.SliderRange("GameObjectProtoInstanceSpawnOffsetX", ref instance.m_minSpawnOffsetX, ref instance.m_maxSpawnOffsetX, -100, 100, showHelp);
                        instance.m_yOffsetBasedOn = (YOffsetBasedOn)editorUtils.EnumPopup("GamObjectProtoInstanceYOffsetBasedOn", instance.m_yOffsetBasedOn, showHelp);
                        if (instance.m_yOffsetBasedOn == YOffsetBasedOn.Custom)
                        {
                            instance.m_customOffset = editorUtils.FloatField("GamObjectProtoInstanceCustomOffsetBase", instance.m_customOffset, showHelp);
                        }
                        editorUtils.SliderRange("GameObjectProtoInstanceSpawnOffsetY", ref instance.m_minSpawnOffsetY, ref instance.m_maxSpawnOffsetY, -100, 100, showHelp);
                        editorUtils.SliderRange("GameObjectProtoInstanceSpawnOffsetZ", ref instance.m_minSpawnOffsetZ, ref instance.m_maxSpawnOffsetZ, -100, 100, showHelp);
                        instance.m_yOffsetToSlope = editorUtils.Toggle("GameObjectProtoInstanceYOffsetToSlope", instance.m_yOffsetToSlope, showHelp);
                        EditorGUI.indentLevel--;
                        editorUtils.LabelField("GameObjectProtoHeadingRotation", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        instance.m_rotateToSlope = editorUtils.Toggle("GameObjectProtoInstanceRotateToSlope", instance.m_rotateToSlope, showHelp);
                        instance.m_alignForwardVectorToSlope = editorUtils.Toggle("GameObjectProtoInstanceAlignForwardVectorToSlope", instance.m_alignForwardVectorToSlope, showHelp);
                        editorUtils.SliderRange("GameObjectProtoInstanceRotationOffsetX", ref instance.m_minRotationOffsetX, ref instance.m_maxRotationOffsetX, 0, 360, showHelp);
                        editorUtils.SliderRange("GameObjectProtoInstanceRotationOffsetY", ref instance.m_minRotationOffsetY, ref instance.m_maxRotationOffsetY, 0, 360, showHelp);
                        editorUtils.SliderRange("GameObjectProtoInstanceRotationOffsetZ", ref instance.m_minRotationOffsetZ, ref instance.m_maxRotationOffsetZ, 0, 360, showHelp);
                        EditorGUI.indentLevel--;
                        editorUtils.LabelField("GameObjectProtoHeadingScale", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        instance.m_spawnScale = (SpawnScale)editorUtils.EnumPopup("ProtoSpawnScale", instance.m_spawnScale, showHelp);
                        EditorGUI.indentLevel++;
                        switch (instance.m_spawnScale)
                        {
                            case SpawnScale.Fixed:
                                instance.m_commonScale = editorUtils.Toggle("ProtoCommonScale", instance.m_commonScale);
                                if (instance.m_commonScale)
                                {
                                    instance.m_minScale = editorUtils.Slider("GameObjectProtoInstanceScale", instance.m_minScale, 0, 100, showHelp);
                                }
                                else
                                {
                                    instance.m_minXYZScale = editorUtils.Vector3Field("GameObjectProtoInstanceScale", instance.m_minXYZScale);
                                }
                                break;
                            case SpawnScale.Random:
                                instance.m_commonScale = editorUtils.Toggle("ProtoCommonScale", instance.m_commonScale);
                                if (instance.m_commonScale)
                                {
                                    instance.m_minScale = editorUtils.Slider("GameObjectProtoInstanceMinScale", instance.m_minScale, 0, 100, showHelp);
                                    instance.m_maxScale = editorUtils.Slider("GameObjectProtoInstanceMaxScale", instance.m_maxScale, 0, 100, showHelp);
                                }
                                else
                                {
                                    instance.m_minXYZScale = editorUtils.Vector3Field("GameObjectProtoInstanceMinScale", instance.m_minXYZScale, showHelp);
                                    instance.m_maxXYZScale = editorUtils.Vector3Field("GameObjectProtoInstanceMaxScale", instance.m_maxXYZScale, showHelp);
                                }
                                break;
                            case SpawnScale.Fitness:
                                instance.m_commonScale = editorUtils.Toggle("ProtoCommonScale", instance.m_commonScale);
                                if (instance.m_commonScale)
                                {
                                    instance.m_minScale = editorUtils.Slider("GameObjectProtoInstanceMinScale", instance.m_minScale, 0, 100, showHelp);
                                    instance.m_maxScale = editorUtils.Slider("GameObjectProtoInstanceMaxScale", instance.m_maxScale, 0, 100, showHelp);
                                }
                                else
                                {
                                    instance.m_minXYZScale = editorUtils.Vector3Field("GameObjectProtoInstanceMinScale", instance.m_minXYZScale, showHelp);
                                    instance.m_maxXYZScale = editorUtils.Vector3Field("GameObjectProtoInstanceMaxScale", instance.m_maxXYZScale, showHelp);
                                }
                                break;
                            case SpawnScale.FitnessRandomized:
                                instance.m_commonScale = editorUtils.Toggle("ProtoCommonScale", instance.m_commonScale);
                                if (instance.m_commonScale)
                                {
                                    instance.m_minScale = editorUtils.Slider("GameObjectProtoInstanceMinScale", instance.m_minScale, 0, 100, showHelp);
                                    instance.m_maxScale = editorUtils.Slider("GameObjectProtoInstanceMaxScale", instance.m_maxScale, 0, 100, showHelp);
                                    instance.m_scaleRandomPercentage = editorUtils.Slider("GameObjectProtoInstanceRandomScalePercentage", instance.m_scaleRandomPercentage * 100f, 0, 100, showHelp) / 100f;
                                }
                                else
                                {
                                    instance.m_minXYZScale = editorUtils.Vector3Field("GameObjectProtoInstanceMinScale", instance.m_minXYZScale, showHelp);
                                    instance.m_maxXYZScale = editorUtils.Vector3Field("GameObjectProtoInstanceMaxScale", instance.m_maxXYZScale, showHelp);
                                    instance.m_XYZScaleRandomPercentage = editorUtils.Vector3Field("GameObjectProtoInstanceRandomScalePercentage", instance.m_XYZScaleRandomPercentage * 100f, showHelp) / 100f;
                                }
                                break;

                        }
                        EditorGUI.indentLevel--;

                        instance.m_scaleByDistance = editorUtils.CurveField("GameObjectProtoInstanceScaleByDistance", instance.m_scaleByDistance);

                        //instance.m_localBounds = editorUtils.FloatField("GameObjectProtoInstanceLocalBounds", instance.m_localBounds);
                        Rect removeButtonRect = EditorGUILayout.GetControlRect();
                        removeButtonRect.x += 15 * EditorGUI.indentLevel;
                        removeButtonRect.width -= 15 * EditorGUI.indentLevel;
                        if (GUI.Button(removeButtonRect, editorUtils.GetContent("GameObjectRemoveInstance")))
                        {
                            resourceProtoGameObject.m_instances = GaiaUtils.RemoveArrayIndexAt<ResourceProtoGameObjectInstance>(resourceProtoGameObject.m_instances, i);
                        }
                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUI.indentLevel--;
                Rect buttonRect = EditorGUILayout.GetControlRect();
                buttonRect.x += 15 * EditorGUI.indentLevel;
                buttonRect.width -= 15 * EditorGUI.indentLevel;
                if (GUI.Button(buttonRect, editorUtils.GetContent("GameObjectAddInstance")))
                {
                    resourceProtoGameObject.m_instances = GaiaUtils.AddElementToArray<ResourceProtoGameObjectInstance>(resourceProtoGameObject.m_instances, new ResourceProtoGameObjectInstance() { m_name = "New Instance" });
                }
            }
            //resourceProtoGameObject.m_dnaFoldedOut = editorUtils.Foldout("GameObjectProtoDNA", resourceProtoGameObject.m_dnaFoldedOut, showHelp);
            //if (resourceProtoGameObject.m_dnaFoldedOut)
            //{
            //    DrawDNA(resourceProtoGameObject.m_dna, editorUtils, showHelp);
            //}
            EditorGUI.indentLevel--;
        }

        public static void DrawSpawnExtensionPrototype(ResourceProtoSpawnExtension resourceProtoSpawnExtension, EditorUtils editorUtils, bool showHelp)
        {
            resourceProtoSpawnExtension.m_name = editorUtils.TextField("SpawnExtensionProtoName", resourceProtoSpawnExtension.m_name, showHelp);
            EditorGUI.indentLevel++;
            resourceProtoSpawnExtension.m_instancesFoldOut = editorUtils.Foldout("SpawnExtensionProtoInstances", resourceProtoSpawnExtension.m_instancesFoldOut, showHelp);
            //Iterate through instances
            if (resourceProtoSpawnExtension.m_instancesFoldOut)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < resourceProtoSpawnExtension.m_instances.Length; i++)
                {
                    var instance = resourceProtoSpawnExtension.m_instances[i];
                    instance.m_foldedOut = editorUtils.Foldout(instance.m_foldedOut, new GUIContent(instance.m_name), showHelp);
                    if (instance.m_foldedOut)
                    {
                        EditorGUI.indentLevel++;
                        instance.m_name = editorUtils.TextField("SpawnExtensionProtoName", instance.m_name, showHelp);

                        GameObject oldPrefab = instance.m_spawnerPrefab;

                        if (instance.m_invalidPrefabSupplied)
                        {
                            EditorGUILayout.HelpBox(editorUtils.GetTextValue("SpawnExtensionNoSpawnExtension"), MessageType.Error);
                        }

                        instance.m_spawnerPrefab = (GameObject)editorUtils.ObjectField("SpawnExtensionProtoPrefab", instance.m_spawnerPrefab, typeof(GameObject), false, showHelp);

                        //New Prefab submitted - check if it actually contains a Spawn Extension
                        if (oldPrefab != instance.m_spawnerPrefab)
                        {
                            if (instance.m_spawnerPrefab.GetComponent<ISpawnExtension>() != null)
                            {
                                instance.m_name = instance.m_spawnerPrefab.name;
                                instance.m_invalidPrefabSupplied = false;
                            }
                            else
                            {
                                instance.m_spawnerPrefab = null;
                                instance.m_invalidPrefabSupplied = true;
                            }
                        }
                        //instance.m_mobilePrefab = (GameObject)editorUtils.ObjectField("GameObjectProtoInstanceMobile", instance.m_mobilePrefab, typeof(GameObject), false, showHelp);
                        instance.m_minSpawnerRuns = editorUtils.IntField("SpawnExtensionProtoMinSpawns", instance.m_minSpawnerRuns, showHelp);
                        instance.m_maxSpawnerRuns = editorUtils.IntField("SpawnExtensionProtoMaxSpawns", instance.m_maxSpawnerRuns, showHelp);
                        instance.m_failureRate = editorUtils.Slider("SpawnExtensionProtoFailureRate", instance.m_failureRate, 0, 1, showHelp);
                        editorUtils.SliderRange("GameObjectProtoInstanceSpawnOffsetX", ref instance.m_minSpawnOffsetX, ref instance.m_maxSpawnOffsetX, -100, 100, showHelp);
                        editorUtils.SliderRange("GameObjectProtoInstanceSpawnOffsetY", ref instance.m_minSpawnOffsetY, ref instance.m_maxSpawnOffsetY, -100, 100, showHelp);
                        editorUtils.SliderRange("GameObjectProtoInstanceSpawnOffsetZ", ref instance.m_minSpawnOffsetZ, ref instance.m_maxSpawnOffsetZ, -100, 100, showHelp);
                        //instance.m_rotateToSlope = editorUtils.Toggle("GameObjectProtoInstanceRotateToSlope", instance.m_rotateToSlope, showHelp);
                        editorUtils.SliderRange("GameObjectProtoInstanceRotationOffsetX", ref instance.m_minRotationOffsetX, ref instance.m_maxRotationOffsetX, 0, 360, showHelp);
                        editorUtils.SliderRange("GameObjectProtoInstanceRotationOffsetY", ref instance.m_minRotationOffsetY, ref instance.m_maxRotationOffsetY, 0, 360, showHelp);
                        editorUtils.SliderRange("GameObjectProtoInstanceRotationOffsetZ", ref instance.m_minRotationOffsetZ, ref instance.m_maxRotationOffsetZ, 0, 360, showHelp);
                        //instance.m_useParentScale = editorUtils.Toggle("GameObjectProtoInstanceRotateToSlope", instance.m_useParentScale, showHelp);
                        instance.m_spawnScale = (SpawnScale)editorUtils.EnumPopup("ProtoSpawnScale", instance.m_spawnScale, showHelp);
                        EditorGUI.indentLevel++;
                        switch (instance.m_spawnScale)
                        {
                            case SpawnScale.Fixed:
                                instance.m_commonScale = editorUtils.Toggle("ProtoCommonScale", instance.m_commonScale);
                                if (instance.m_commonScale)
                                {
                                    instance.m_minScale = editorUtils.Slider("GameObjectProtoInstanceScale", instance.m_minScale, 0, 100, showHelp);
                                }
                                else
                                {
                                    instance.m_minXYZScale = editorUtils.Vector3Field("GameObjectProtoInstanceScale", instance.m_minXYZScale);
                                }
                                break;
                            case SpawnScale.Random:
                                instance.m_commonScale = editorUtils.Toggle("ProtoCommonScale", instance.m_commonScale);
                                if (instance.m_commonScale)
                                {
                                    instance.m_minScale = editorUtils.Slider("GameObjectProtoInstanceMinScale", instance.m_minScale, 0, 100, showHelp);
                                    instance.m_maxScale = editorUtils.Slider("GameObjectProtoInstanceMaxScale", instance.m_maxScale, 0, 100, showHelp);
                                }
                                else
                                {
                                    instance.m_minXYZScale = editorUtils.Vector3Field("GameObjectProtoInstanceMinScale", instance.m_minXYZScale, showHelp);
                                    instance.m_maxXYZScale = editorUtils.Vector3Field("GameObjectProtoInstanceMaxScale", instance.m_maxXYZScale, showHelp);
                                }
                                break;
                            case SpawnScale.Fitness:
                                instance.m_commonScale = editorUtils.Toggle("ProtoCommonScale", instance.m_commonScale);
                                if (instance.m_commonScale)
                                {
                                    instance.m_minScale = editorUtils.Slider("GameObjectProtoInstanceMinScale", instance.m_minScale, 0, 100, showHelp);
                                    instance.m_maxScale = editorUtils.Slider("GameObjectProtoInstanceMaxScale", instance.m_maxScale, 0, 100, showHelp);
                                }
                                else
                                {
                                    instance.m_minXYZScale = editorUtils.Vector3Field("GameObjectProtoInstanceMinScale", instance.m_minXYZScale, showHelp);
                                    instance.m_maxXYZScale = editorUtils.Vector3Field("GameObjectProtoInstanceMaxScale", instance.m_maxXYZScale, showHelp);
                                }
                                break;
                            case SpawnScale.FitnessRandomized:
                                instance.m_commonScale = editorUtils.Toggle("ProtoCommonScale", instance.m_commonScale);
                                if (instance.m_commonScale)
                                {
                                    instance.m_minScale = editorUtils.Slider("GameObjectProtoInstanceMinScale", instance.m_minScale, 0, 100, showHelp);
                                    instance.m_maxScale = editorUtils.Slider("GameObjectProtoInstanceMaxScale", instance.m_maxScale, 0, 100, showHelp);
                                    instance.m_scaleRandomPercentage = editorUtils.Slider("GameObjectProtoInstanceRandomScalePercentage", instance.m_scaleRandomPercentage * 100f, 0, 100f, showHelp) / 100f;
                                }
                                else
                                {
                                    instance.m_minXYZScale = editorUtils.Vector3Field("GameObjectProtoInstanceMinScale", instance.m_minXYZScale, showHelp);
                                    instance.m_maxXYZScale = editorUtils.Vector3Field("GameObjectProtoInstanceMaxScale", instance.m_maxXYZScale, showHelp);
                                    instance.m_XYZScaleRandomPercentage = editorUtils.Vector3Field("GameObjectProtoInstanceRandomScalePercentage", instance.m_XYZScaleRandomPercentage * 100f, showHelp) / 100f;
                                }
                                break;
                        }
                        EditorGUI.indentLevel--;

                        instance.m_scaleByDistance = editorUtils.CurveField("GameObjectProtoInstanceScaleByDistance", instance.m_scaleByDistance);

                        //instance.m_localBounds = editorUtils.FloatField("GameObjectProtoInstanceLocalBounds", instance.m_localBounds);
                        Rect removeButtonRect = EditorGUILayout.GetControlRect();
                        removeButtonRect.x += 15 * EditorGUI.indentLevel;
                        removeButtonRect.width -= 15 * EditorGUI.indentLevel;
                        if (GUI.Button(removeButtonRect, editorUtils.GetContent("SpawnExtensionProtoRemoveInstance")))
                        {
                            resourceProtoSpawnExtension.m_instances = GaiaUtils.RemoveArrayIndexAt<ResourceProtoSpawnExtensionInstance>(resourceProtoSpawnExtension.m_instances, i);
                        }
                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUI.indentLevel--;
                Rect buttonRect = EditorGUILayout.GetControlRect();
                buttonRect.x += 15 * EditorGUI.indentLevel;
                buttonRect.width -= 15 * EditorGUI.indentLevel;
                if (GUI.Button(buttonRect, editorUtils.GetContent("SpawnExtensionProtoAddInstance")))
                {
                    resourceProtoSpawnExtension.m_instances = GaiaUtils.AddElementToArray<ResourceProtoSpawnExtensionInstance>(resourceProtoSpawnExtension.m_instances, new ResourceProtoSpawnExtensionInstance() { m_name = "New Spawn Extension" });
                }
            }
            resourceProtoSpawnExtension.m_dnaFoldedOut = editorUtils.Foldout("GameObjectProtoDNA", resourceProtoSpawnExtension.m_dnaFoldedOut, showHelp);
            if (resourceProtoSpawnExtension.m_dnaFoldedOut)
            {
                resourceProtoSpawnExtension.m_dna.m_boundsRadius = editorUtils.FloatField("GameObjectProtoDNABoundsRadius", resourceProtoSpawnExtension.m_dna.m_boundsRadius, showHelp);
            }
            EditorGUI.indentLevel--;
        }

        public static void DrawStampDistributionPrototype(ResourceProtoStamp resourceProtoStamp, EditorUtils editorUtils, List<string> stampCategoryNames, int[] stampCategoryIDs, ref bool probabilityCurveChanged, bool showHelp)
        {
            EditorGUI.indentLevel--;
            //we need to set the id initially according to the stored string for the category
            //we can't rely on the IDs being the same every time, since the user might have added additional category folders in the meantime
            int selectedStampCategoryID = -99;
            selectedStampCategoryID = stampCategoryNames.IndexOf(resourceProtoStamp.m_featureType);
            selectedStampCategoryID = EditorGUILayout.IntPopup("FeatureType", selectedStampCategoryID, stampCategoryNames.ToArray(), stampCategoryIDs);
            editorUtils.InlineHelp("FeatureType", showHelp);
            resourceProtoStamp.m_spawnProbability = editorUtils.Slider("StampSpawnProbability", resourceProtoStamp.m_spawnProbability * 100f, 0f, 100f, showHelp) / 100f;
            EditorGUI.BeginChangeCheck();
            resourceProtoStamp.m_inputHeightToProbabilityMapping = editorUtils.CurveField("InputHeightToProbabilityMapping", resourceProtoStamp.m_inputHeightToProbabilityMapping, showHelp);
            if (EditorGUI.EndChangeCheck())
            {
                probabilityCurveChanged = true;
            }
            resourceProtoStamp.m_inputHeightToStampHeightMapping = editorUtils.CurveField("InputHeightToStampHeightMapping", resourceProtoStamp.m_inputHeightToStampHeightMapping, showHelp);

            if (selectedStampCategoryID >= 0)
            {
                resourceProtoStamp.m_featureType = stampCategoryNames[selectedStampCategoryID];
            }
            //resourceProtoStamp.m_stampInfluence = (ImageMaskInfluence)editorUtils.EnumPopup("MaskInfluence", resourceProtoStamp.m_stampInfluence, showHelp);
            //resourceProtoStamp.m_borderMaskType = stampCategoryNames[selectedBorderMaskCategoryID];
            resourceProtoStamp.m_invertChance = editorUtils.Slider("FeatureTypeInvertChance", resourceProtoStamp.m_invertChance, 0, 100, showHelp);
            editorUtils.MinMaxSliderWithFields("FeatureTypeWidth", ref resourceProtoStamp.m_minWidth, ref resourceProtoStamp.m_maxWidth, 0, 100, showHelp);
            //resourceProtoStamp.m_tieWidthToStrength = editorUtils.Toggle("FeatureTypeTieWidth", resourceProtoStamp.m_tieWidthToStrength, showHelp);
            if (resourceProtoStamp.m_operation != GaiaConstants.TerrainGeneratorFeatureOperation.MixHeight)
            {
                editorUtils.MinMaxSliderWithFields("FeatureTypeHeight", ref resourceProtoStamp.m_minHeight, ref resourceProtoStamp.m_maxHeight, 0, 20, showHelp);
                resourceProtoStamp.m_tieHeightToStrength = editorUtils.Toggle("FeatureTypeTieHeight", resourceProtoStamp.m_tieHeightToStrength, showHelp);
                editorUtils.MinMaxSliderWithFields("FeatureTypeYOffset", ref resourceProtoStamp.m_minYOffset, ref resourceProtoStamp.m_maxYOffset, -150, 150, showHelp);
            }
            else
            {
                resourceProtoStamp.m_minMixStrength *= 100f;
                resourceProtoStamp.m_maxMixStrength *= 100f;
                editorUtils.MinMaxSliderWithFields("FeatureTypeMixHeightStrength", ref resourceProtoStamp.m_minMixStrength, ref resourceProtoStamp.m_maxMixStrength, 0, 200, showHelp);
                resourceProtoStamp.m_minMixStrength /= 100f;
                resourceProtoStamp.m_maxMixStrength /= 100f;
                //resourceProtoStamp.m_tieHeightToStrength = editorUtils.Toggle("FeatureTypeTieStrength", resourceProtoStamp.m_tieHeightToStrength, showHelp);
                editorUtils.MinMaxSliderWithFields("FeatureTypeMixMidpoint", ref resourceProtoStamp.m_minMixMidPoint, ref resourceProtoStamp.m_maxMixMidPoint, 0, 1, showHelp);
            }


            //}
            EditorGUI.indentLevel++;
        }

        public static void DrawWorldBiomeMaskPrototype(ResourceProtoWorldBiomeMask worldBiomeMaskPrototype, EditorUtils editorUtils, bool showHelp)
        {
            worldBiomeMaskPrototype.m_name = editorUtils.TextField("SpawnExtensionProtoName", worldBiomeMaskPrototype.m_name, showHelp);
            worldBiomeMaskPrototype.m_biomePreset = (BiomePreset)editorUtils.ObjectField("WorldBiomeMaskBiomePreset", worldBiomeMaskPrototype.m_biomePreset, typeof(BiomePreset), false, showHelp);

        }

        public static void DrawTerrainModifierStampPrototype(ResourceProtoTerrainModifierStamp terrainModifierStampPrototype, EditorUtils editorUtils, bool showHelp)
        {
            terrainModifierStampPrototype.m_name = editorUtils.TextField("TerrainModifierStampProtoName", terrainModifierStampPrototype.m_name, showHelp);
            int selectedIndex = EditorGUILayout.Popup(editorUtils.GetContent("TerrainModifierStampProtoOperationType"), GaiaConstants.FeatureOperationNames.Select((x, i) => new { item = x, index = i }).First(x => x.item.Value == (int)terrainModifierStampPrototype.m_operation).index, GaiaConstants.FeatureOperationNames.Select(x => x.Key).ToArray());
            editorUtils.InlineHelp("TerrainModifierStampProtoOperationType", showHelp);
            //The "FeatureOperationNames" are just an array of strings to get a multi-level popup going. To get the actual operation enum
            //we need to select the enum element at the same index as the name array.
            int selectedValue = -99;
            GaiaConstants.FeatureOperationNames.TryGetValue(GaiaConstants.FeatureOperationNames.Select(x => x.Key).ToArray()[selectedIndex], out selectedValue);
            terrainModifierStampPrototype.m_operation = (GaiaConstants.FeatureOperation)selectedValue;

            //Drawing the "special" controls for the respective operation type.
            switch (terrainModifierStampPrototype.m_operation)
            {
                case GaiaConstants.FeatureOperation.RaiseHeight:
                    editorUtils.InlineHelp("RaiseHeightIntro", showHelp);
                    DrawBaseStampSettings(terrainModifierStampPrototype, editorUtils, showHelp);
                    break;
                case GaiaConstants.FeatureOperation.LowerHeight:
                    editorUtils.InlineHelp("LowerHeightIntro", showHelp);
                    DrawBaseStampSettings(terrainModifierStampPrototype, editorUtils, showHelp);
                    break;
                case GaiaConstants.FeatureOperation.BlendHeight:
                    editorUtils.InlineHelp("BlendHeightIntro", showHelp);
                    DrawBaseStampSettings(terrainModifierStampPrototype, editorUtils, showHelp);
                    break;
                case GaiaConstants.FeatureOperation.SetHeight:
                    editorUtils.InlineHelp("SetHeightIntro", showHelp);
                    DrawBaseStampSettings(terrainModifierStampPrototype, editorUtils, showHelp);
                    break;
                case GaiaConstants.FeatureOperation.AddHeight:
                    editorUtils.InlineHelp("AddHeightIntro", showHelp);
                    DrawBaseStampSettings(terrainModifierStampPrototype, editorUtils, showHelp);

                    break;
                case GaiaConstants.FeatureOperation.SubtractHeight:
                    editorUtils.InlineHelp("SubtractHeightIntro", showHelp);
                    DrawBaseStampSettings(terrainModifierStampPrototype, editorUtils, showHelp);
                    break;
                case GaiaConstants.FeatureOperation.HydraulicErosion:
                    editorUtils.InlineHelp("HydraulicErosionIntro", showHelp);
                    //DrawHydraulicErosionControls(showHelp);
                    break;
                case GaiaConstants.FeatureOperation.Contrast:
                    editorUtils.InlineHelp("ContrastIntro", showHelp);
                    terrainModifierStampPrototype.m_contrastFeatureSize = editorUtils.Slider("TerrainModifierStampProtoContrastFeatureSize", terrainModifierStampPrototype.m_contrastFeatureSize, 1.0f, 100.0f, showHelp);
                    terrainModifierStampPrototype.m_contrastStrength = editorUtils.Slider("TerrainModifierStampProtoContrastStrength", terrainModifierStampPrototype.m_contrastStrength, 0.01f, 10.0f, showHelp);
                    break;
                case GaiaConstants.FeatureOperation.Terrace:
                    editorUtils.InlineHelp("TerraceIntro", showHelp);
#if GAIA_2023_PRO
                    terrainModifierStampPrototype.m_terraceCount = editorUtils.Slider("TerrainModifierStampProtoTerraceCount", terrainModifierStampPrototype.m_terraceCount, 2.0f, 1000.0f, showHelp);
                    terrainModifierStampPrototype.m_terraceBevelAmountInterior = editorUtils.Slider("TerrainModifierStampProtoTerraceBevelAmount", terrainModifierStampPrototype.m_terraceBevelAmountInterior, 0.0f, 1.0f, showHelp);
#else
            EditorGUILayout.HelpBox(editorUtils.GetTextValue("GaiaProOperationInfo"), MessageType.Warning);
            GUI.enabled = false;
#endif
                    break;
                case GaiaConstants.FeatureOperation.SharpenRidges:
                    editorUtils.InlineHelp("SharpenRidgesIntro", showHelp);
#if GAIA_2023_PRO
                    terrainModifierStampPrototype.m_sharpenRidgesMixStrength = editorUtils.Slider("TerrainModifierStampProtoSRSharpness", terrainModifierStampPrototype.m_sharpenRidgesMixStrength, 0, 1, showHelp);
                    terrainModifierStampPrototype.m_sharpenRidgesIterations = editorUtils.Slider("TerrainModifierStampProtoSRIterations", terrainModifierStampPrototype.m_sharpenRidgesIterations, 0, 20, showHelp);
#else
            EditorGUILayout.HelpBox(editorUtils.GetTextValue("GaiaProOperationInfo"), MessageType.Warning);
            GUI.enabled = false;
#endif
                    break;
                case GaiaConstants.FeatureOperation.HeightTransform:
                    editorUtils.InlineHelp("HeightTransformIntro", showHelp);
                    terrainModifierStampPrototype.m_heightTransformCurve = editorUtils.CurveField("TerrainModifierStampProtoHeightTransformCurve", terrainModifierStampPrototype.m_heightTransformCurve, showHelp);
                    break;
                case GaiaConstants.FeatureOperation.PowerOf:
                    editorUtils.InlineHelp("PowerOfIntro", showHelp);
                    terrainModifierStampPrototype.m_powerOf = editorUtils.Slider("TerrainModifierStampProtoPowerOf", terrainModifierStampPrototype.m_powerOf, 0.01f, 5.0f, showHelp);
                    break;
                case GaiaConstants.FeatureOperation.Smooth:
                    editorUtils.InlineHelp("SmoothIntro", showHelp);
                    terrainModifierStampPrototype.m_smoothVerticality = editorUtils.Slider("TerrainModifierStampProtoSmoothVerticality", terrainModifierStampPrototype.m_smoothVerticality, -1f, 1f, showHelp);
                    terrainModifierStampPrototype.m_smoothBlurRadius = editorUtils.Slider("TerrainModifierStampProtoSmoothBlurRadius", terrainModifierStampPrototype.m_smoothBlurRadius, 0f, 30f, showHelp);
                    break;
                case GaiaConstants.FeatureOperation.MixHeight:
                    editorUtils.InlineHelp("MixHeightIntro", showHelp);
                    DrawBaseStampSettings(terrainModifierStampPrototype, editorUtils, showHelp);
                    terrainModifierStampPrototype.m_mixMidPoint = editorUtils.Slider("MixMidPoint", terrainModifierStampPrototype.m_mixMidPoint, 0f, 1f, showHelp);
                    terrainModifierStampPrototype.m_mixHeightStrength = editorUtils.Slider("MixHeightStrength", terrainModifierStampPrototype.m_mixHeightStrength * 100f, 0f, 200f, showHelp) / 100f;
                    break;
                default:
                    //m_editorUtils.Panel("StampSettings", DrawStampSettings, true);
                    break;
            }

        }

        public static void DrawBaseStampSettings(ResourceProtoTerrainModifierStamp terrainModifierStampPrototype, EditorUtils editorUtils, bool showHelp)
        {
            if (terrainModifierStampPrototype.m_stamperInputImageMask.ImageMaskTexture != null && !GaiaConstants.Valid16BitFormats.Contains(terrainModifierStampPrototype.m_stamperInputImageMask.ImageMaskTexture.format))
            {
                EditorGUILayout.HelpBox("Supplied texture is not in 16-bit color format. For optimal quality use 16+ bit color images.", MessageType.Warning);
            }
            GUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField(editorUtils.GetContent("StampImage"), GUILayout.Width(EditorGUIUtility.labelWidth));
            terrainModifierStampPrototype.m_stamperInputImageMask.ImageMaskTexture = (Texture2D)editorUtils.ObjectField("StampImage", terrainModifierStampPrototype.m_stamperInputImageMask.ImageMaskTexture, typeof(Texture2D), false, showHelp, GUILayout.Height(EditorGUIUtility.singleLineHeight));

            if (GUILayout.Button(editorUtils.GetContent("MaskImageOpenStampButton"), GUILayout.Width(70)))
            {
                ImageMaskListEditor.OpenStampBrowser(terrainModifierStampPrototype.m_stamperInputImageMask);
            }
            GUILayout.EndHorizontal();
            editorUtils.InlineHelp("StampImage", showHelp);
            GUILayout.BeginHorizontal();
            terrainModifierStampPrototype.m_stamperInputImageMask.m_strengthTransformCurve = editorUtils.CurveField("MaskStrengthTransformCurve", terrainModifierStampPrototype.m_stamperInputImageMask.m_strengthTransformCurve);
            if (GUILayout.Button(editorUtils.GetContent("MaskInvert"), GUILayout.Width(70)))
            {
                GaiaUtils.InvertAnimationCurve(ref terrainModifierStampPrototype.m_stamperInputImageMask.m_strengthTransformCurve);
            }
            GUILayout.EndHorizontal();
            editorUtils.InlineHelp("MaskStrengthTransformCurve", showHelp);

            GUILayout.BeginHorizontal();
            editorUtils.LabelField("StampSpace", GUILayout.Width(EditorGUIUtility.labelWidth));
            terrainModifierStampPrototype.m_stamperInputImageMask.m_imageMaskSpace = (ImageMaskSpace)EditorGUILayout.EnumPopup(terrainModifierStampPrototype.m_stamperInputImageMask.m_imageMaskSpace, GUILayout.Width(EditorGUIUtility.labelWidth * 0.68f));
            GUILayout.Space(10);
            editorUtils.LabelField("StampTiling", GUILayout.Width(75));
            terrainModifierStampPrototype.m_stamperInputImageMask.m_tiling = EditorGUILayout.Toggle(terrainModifierStampPrototype.m_stamperInputImageMask.m_tiling);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            editorUtils.InlineHelp("StampSpace", showHelp);

            GUILayout.BeginHorizontal();
            editorUtils.LabelField("StampOffset", GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUIUtility.labelWidth = 10;
            terrainModifierStampPrototype.m_stamperInputImageMask.m_xOffSet = editorUtils.FloatField("MaskXOffset", terrainModifierStampPrototype.m_stamperInputImageMask.m_xOffSet);
            GUILayout.Space(10);
            terrainModifierStampPrototype.m_stamperInputImageMask.m_zOffSet = editorUtils.FloatField("MaskZOffset", terrainModifierStampPrototype.m_stamperInputImageMask.m_zOffSet);
            EditorGUIUtility.labelWidth = 0;
            GUILayout.EndHorizontal();
            editorUtils.InlineHelp("StampOffset", showHelp);

            GUILayout.BeginHorizontal();
            editorUtils.LabelField("StampScale", GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUIUtility.labelWidth = 10;
            terrainModifierStampPrototype.m_stamperInputImageMask.m_xScale = editorUtils.FloatField("MaskXScale", terrainModifierStampPrototype.m_stamperInputImageMask.m_xScale);
            GUILayout.Space(10);
            terrainModifierStampPrototype.m_stamperInputImageMask.m_zScale = editorUtils.FloatField("MaskZScale", terrainModifierStampPrototype.m_stamperInputImageMask.m_zScale);
            EditorGUIUtility.labelWidth = 0;
            GUILayout.EndHorizontal();
            editorUtils.InlineHelp("StampScale", showHelp);
            terrainModifierStampPrototype.m_stamperInputImageMask.m_rotationDegrees = editorUtils.Slider("StampRotation", terrainModifierStampPrototype.m_stamperInputImageMask.m_rotationDegrees, -180f, 180f, showHelp);

            if (terrainModifierStampPrototype.m_operation == GaiaConstants.FeatureOperation.RaiseHeight ||
                terrainModifierStampPrototype.m_operation == GaiaConstants.FeatureOperation.LowerHeight ||
                terrainModifierStampPrototype.m_operation == GaiaConstants.FeatureOperation.BlendHeight ||
                terrainModifierStampPrototype.m_operation == GaiaConstants.FeatureOperation.SetHeight
                )
            {
                terrainModifierStampPrototype.m_y = editorUtils.FloatField("TerrainModifierStampProtoYPosition", (float)terrainModifierStampPrototype.m_y, showHelp);
                terrainModifierStampPrototype.m_height = editorUtils.FloatField("TerrainModifierStampProtoHeight", terrainModifierStampPrototype.m_height, showHelp);
            }
            if (terrainModifierStampPrototype.m_operation == GaiaConstants.FeatureOperation.AddHeight || terrainModifierStampPrototype.m_operation == GaiaConstants.FeatureOperation.SubtractHeight)
            {
                float maxHeight = 1000;
                Terrain terrain = null; // m_stamper.GetCurrentTerrain();
                if (terrain != null)
                {
                    maxHeight = terrain.terrainData.size.y;
                }
                EditorGUI.BeginChangeCheck();
                terrainModifierStampPrototype.m_absoluteHeightValue = editorUtils.Slider("AbsoluteHeightMeter", terrainModifierStampPrototype.m_absoluteHeightValue, -maxHeight, maxHeight, showHelp);
                //m_absoluteHeightOPSwitch = false;
                if (EditorGUI.EndChangeCheck())
                {
                    if (terrainModifierStampPrototype.m_absoluteHeightValue < 0 && terrainModifierStampPrototype.m_operation != GaiaConstants.FeatureOperation.SubtractHeight)
                    {
                        terrainModifierStampPrototype.m_operation = GaiaConstants.FeatureOperation.SubtractHeight;
                        //      m_absoluteHeightOPSwitch = true;
                    }
                    if (terrainModifierStampPrototype.m_absoluteHeightValue > 0 && terrainModifierStampPrototype.m_operation != GaiaConstants.FeatureOperation.AddHeight)
                    {
                        terrainModifierStampPrototype.m_operation = GaiaConstants.FeatureOperation.AddHeight;
                        //    m_absoluteHeightOPSwitch = true;
                    }
                    //m_stamper.SetStampScaleByMeter(terrainModifierStampPrototype.m_absoluteHeightValue);
                }
                else
                {
                    //terrainModifierStampPrototype.m_absoluteHeightValue = m_stamper.CurrentStampScaleToMeter();
                }
            }
            if (terrainModifierStampPrototype.m_operation == GaiaConstants.FeatureOperation.BlendHeight)
            {
                //Value displayed in % on the UI
                terrainModifierStampPrototype.m_blendStrength = editorUtils.Slider("BlendStrength", terrainModifierStampPrototype.m_blendStrength * 100f, 0f, 100f, showHelp) / 100f;
            }
        }

        public static void DrawProbePrototype(ResourceProtoProbe probePrototype, EditorUtils editorUtils, GaiaConstants.EnvironmentRenderer currentPipeline, bool showHelp)
        {
            probePrototype.m_name = editorUtils.TextField("ProbeProtoName", probePrototype.m_name, showHelp);
            probePrototype.m_probeType = (ProbeType)editorUtils.EnumPopup("ProbeType", probePrototype.m_probeType, showHelp);
            if (probePrototype.m_probeType == ProbeType.ReflectionProbe)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                editorUtils.Heading("Probe Rendering Settings");
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                probePrototype.m_reflectionProbeData.reflectionProbeMode = (ReflectionProbeMode)editorUtils.EnumPopup("ReflectionProbeMode", probePrototype.m_reflectionProbeData.reflectionProbeMode, showHelp);
                probePrototype.m_reflectionProbeData.reflectionProbeRefresh = (GaiaConstants.ReflectionProbeRefreshModePW)editorUtils.EnumPopup("ReflectionProbeRefresh", probePrototype.m_reflectionProbeData.reflectionProbeRefresh, showHelp);
                EditorGUI.indentLevel--;
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                editorUtils.Heading("Probe Optimization Settings");
                GUILayout.EndHorizontal();

                EditorGUI.indentLevel++;
                if (probePrototype.m_reflectionProbeData.reflectionProbeRefresh == GaiaConstants.ReflectionProbeRefreshModePW.ViaScripting)
                {
                    probePrototype.m_reflectionProbeData.reflectionProbeTimeSlicingMode = (ReflectionProbeTimeSlicingMode)editorUtils.EnumPopup("ReflectionProbeTimeSlicing", probePrototype.m_reflectionProbeData.reflectionProbeTimeSlicingMode, showHelp);
                }
                if (currentPipeline != GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
                    probePrototype.m_reflectionProbeData.reflectionProbeResolution = (GaiaConstants.ReflectionProbeResolution)editorUtils.EnumPopup("ReflectionProbeResolution", probePrototype.m_reflectionProbeData.reflectionProbeResolution, showHelp);
                    probePrototype.m_reflectionProbeData.reflectionCubemapCompression = (ReflectionCubemapCompression)editorUtils.EnumPopup("ReflectionProbeCompression", probePrototype.m_reflectionProbeData.reflectionCubemapCompression, showHelp);
                }
                probePrototype.m_reflectionProbeData.reflectionProbeClipPlaneDistance = editorUtils.Slider("ReflectionProbeRenderDistance", probePrototype.m_reflectionProbeData.reflectionProbeClipPlaneDistance, 0.1f, 10000f, showHelp);
                probePrototype.m_reflectionProbeData.reflectionProbeShadowDistance = editorUtils.Slider("ReflectionProbeShadowDistance", probePrototype.m_reflectionProbeData.reflectionProbeShadowDistance, 0.1f, 3000f, showHelp);
                probePrototype.m_reflectionProbeData.reflectionprobeCullingMask = GaiaEditorUtils.LayerMaskField(new GUIContent(editorUtils.GetTextValue("ReflectionProbeCullingMask"), editorUtils.GetTooltip("ReflectionProbeCullingMask")), probePrototype.m_reflectionProbeData.reflectionprobeCullingMask);
                editorUtils.InlineHelp("ReflectionProbeCullingMask", showHelp);
                EditorGUI.indentLevel--;
            }

        }

        //private void DrawTrees(bool showHelp)
        //{
        //    EditorGUI.indentLevel++;
        //    for (int treeProtoIndex = 0; treeProtoIndex < m_resource.m_treePrototypes.Length; treeProtoIndex++)
        //    {
        //        int resourceIndex = GetResourceIndexFromPrototypeIndex(GaiaConstants.SpawnerResourceType.TerrainTree, treeProtoIndex);

        //        m_resourceProtoFoldOutStatus[resourceIndex] = EditorGUILayout.Foldout(m_resourceProtoFoldOutStatus[resourceIndex], m_resource.m_treePrototypes[treeProtoIndex].m_name);
        //        if (m_resourceProtoFoldOutStatus[resourceIndex])
        //        {
        //            DrawTreePrototype(m_resource.m_treePrototypes[treeProtoIndex], m_editorUtils, showHelp);

        //            if (m_editorUtils.Button("DeleteTree"))
        //            {
        //                m_resource.m_treePrototypes = GaiaUtils.RemoveArrayIndexAt<ResourceProtoTree>(m_resource.m_treePrototypes, treeProtoIndex);
        //                m_resourceProtoFoldOutStatus = GaiaUtils.RemoveArrayIndexAt<bool>(m_resourceProtoFoldOutStatus, resourceIndex);
        //                //Correct the index since we just removed one texture
        //                treeProtoIndex--;
        //            }

        //            //Rect maskRect;
        //            //m_resourceIndexBeingDrawn = treeProtoIndex;
        //            //if (m_resourceProtoMasksExpanded[resourceIndex])
        //            //{
        //            //    m_maskListBeingDrawn = m_resource.m_treePrototypes[treeProtoIndex].m_imageMasks;
        //            //    maskRect = EditorGUILayout.GetControlRect(true, m_resourceProtoReorderableLists[resourceIndex].GetHeight());
        //            //    m_resourceProtoReorderableLists[resourceIndex].DoList(maskRect);
        //            //}
        //            //else
        //            //{
        //            //    int oldIndent = EditorGUI.indentLevel;
        //            //    EditorGUI.indentLevel = 1;
        //            //    m_resourceProtoMasksExpanded[resourceIndex] = EditorGUILayout.Foldout(m_resourceProtoMasksExpanded[resourceIndex], ImageMaskListEditor.PropertyCount("MaskSettings", m_resource.m_treePrototypes[treeProtoIndex].m_imageMasks, m_editorUtils), true);
        //            //    maskRect = GUILayoutUtility.GetLastRect();
        //            //    EditorGUI.indentLevel = oldIndent;
        //            //}
        //        }
        //    }
        //    EditorGUI.indentLevel--;
        //    if (m_editorUtils.Button("AddTree"))
        //    {
        //        m_resource.m_treePrototypes = GaiaUtils.AddElementToArray<ResourceProtoTree>(m_resource.m_treePrototypes, new ResourceProtoTree());
        //        m_resource.m_treePrototypes[m_resource.m_treePrototypes.Length - 1].m_name = "New Tree Prototype";
        //        m_resourceProtoFoldOutStatus = GaiaUtils.AddElementToArray<bool>(m_resourceProtoFoldOutStatus, false);
        //    }
        //}

        public static void DrawTreePrototype(ResourceProtoTree resourceProtoTree, Spawner spawner, EditorUtils editorUtils, bool showHelp)
        {
            editorUtils.LabelField("TreeProtoHeadingPrefab", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            resourceProtoTree.m_name = editorUtils.TextField("GameObjectProtoName", resourceProtoTree.m_name, showHelp);
            resourceProtoTree.m_desktopPrefab = (GameObject)editorUtils.ObjectField("TreeProtoDesktopPrefab", resourceProtoTree.m_desktopPrefab, typeof(GameObject), false, showHelp);
            //AssetInventoryButton<GameObject>(resourceProtoTree.m_desktopPrefab);
#if ASSET_INVENTORY
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (GUILayout.Button(editorUtils.GetContent("SearchAssetInventoryButton")))
                {
                    ResultPickerUI window = ResultPickerUI.Show(path => { resourceProtoTree.m_desktopPrefab = (GameObject)LoadAssetFromPath(path); }, "Prefabs");
                    window.instantSelection = false;
                    window.hideDetailsPane = false;
                }
            }
            EditorGUILayout.EndHorizontal();
#endif

            //resourceProtoTree.m_mobilePrefab = (GameObject)editorUtils.ObjectField("TreeProtoMobilePrefab", resourceProtoTree.m_mobilePrefab, typeof(GameObject), false, showHelp);
            resourceProtoTree.m_bendFactor = editorUtils.Slider("TreeProtoBendFactor", resourceProtoTree.m_bendFactor, 0, 100, showHelp);
            resourceProtoTree.m_healthyColour = editorUtils.ColorField("TreeProtoHealthyColour", resourceProtoTree.m_healthyColour, showHelp);
            resourceProtoTree.m_dryColour = editorUtils.ColorField("TreeProtoDryColour", resourceProtoTree.m_dryColour, showHelp);
            EditorGUI.indentLevel--;
            editorUtils.LabelField("TreeProtoHeadingPosition", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            resourceProtoTree.m_snapToTerrain = editorUtils.Toggle("TreeProtoSnapToTerrain", resourceProtoTree.m_snapToTerrain, showHelp);
            bool currentGUIState = GUI.enabled;
            if (resourceProtoTree.m_snapToTerrain)
            {
                GUI.enabled = false;
            }
            resourceProtoTree.m_yOffsetBasedOn = (YOffsetBasedOn)editorUtils.EnumPopup("TreeProtoyOffsetBasedOn", resourceProtoTree.m_yOffsetBasedOn, showHelp);
            if (resourceProtoTree.m_yOffsetBasedOn == YOffsetBasedOn.Custom)
            {
                resourceProtoTree.m_customOffset = editorUtils.FloatField("TreePrototypeCustomOffsetBase", resourceProtoTree.m_customOffset, showHelp);
            }
            editorUtils.MinMaxSliderWithFields("TreeProtoMinMaxYOffset", ref resourceProtoTree.m_minYOffset, ref resourceProtoTree.m_maxYOffset, -50f, 50f, showHelp);
            GUI.enabled = currentGUIState;
            EditorGUI.indentLevel--;
            editorUtils.LabelField("TreeProtoHeadingScale", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            resourceProtoTree.m_spawnScale = (SpawnScale)editorUtils.EnumPopup("ProtoSpawnScale", resourceProtoTree.m_spawnScale, showHelp);
            EditorGUI.indentLevel++;
            switch (resourceProtoTree.m_spawnScale)
            {
                case SpawnScale.Fixed:
                    resourceProtoTree.m_minWidth = editorUtils.FloatField("TreeProtoWidth", resourceProtoTree.m_minWidth, showHelp);
                    resourceProtoTree.m_minHeight = editorUtils.FloatField("TreeProtoHeight", resourceProtoTree.m_minHeight, showHelp);
                    break;
                case SpawnScale.Random:
                    editorUtils.MinMaxSliderWithFields("TreeProtoMinMaxWidth", ref resourceProtoTree.m_minWidth, ref resourceProtoTree.m_maxWidth, 0f, 10f, showHelp);
                    editorUtils.MinMaxSliderWithFields("TreeProtoMinMaxHeight", ref resourceProtoTree.m_minHeight, ref resourceProtoTree.m_maxHeight, 0f, 10f, showHelp);
                    break;
                case SpawnScale.Fitness:
                    editorUtils.MinMaxSliderWithFields("TreeProtoMinMaxWidth", ref resourceProtoTree.m_minWidth, ref resourceProtoTree.m_maxWidth, 0f, 10f, showHelp);
                    editorUtils.MinMaxSliderWithFields("TreeProtoMinMaxHeight", ref resourceProtoTree.m_minHeight, ref resourceProtoTree.m_maxHeight, 0f, 10f, showHelp);
                    break;
                case SpawnScale.FitnessRandomized:
                    editorUtils.MinMaxSliderWithFields("TreeProtoMinMaxWidth", ref resourceProtoTree.m_minWidth, ref resourceProtoTree.m_maxWidth, 0f, 10f, showHelp);
                    resourceProtoTree.m_widthRandomPercentage = editorUtils.Slider("TreeProtoWidthRandomPercentage", resourceProtoTree.m_widthRandomPercentage * 100f, 0f, 100f) / 100f;
                    editorUtils.MinMaxSliderWithFields("TreeProtoMinMaxHeight", ref resourceProtoTree.m_minHeight, ref resourceProtoTree.m_maxHeight, 0f, 10f, showHelp);
                    resourceProtoTree.m_heightRandomPercentage = editorUtils.Slider("TreeProtoHeightRandomPercentage", resourceProtoTree.m_heightRandomPercentage * 100f, 0f, 100f) / 100f;
                    break;
            }
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }


        //private void DrawTerrainDetails(bool showHelp)
        //{
        //    EditorGUI.indentLevel++;
        //    for (int terrainDetailProtoIndex = 0; terrainDetailProtoIndex < m_resource.m_detailPrototypes.Length; terrainDetailProtoIndex++)
        //    {
        //        int resourceIndex = GetResourceIndexFromPrototypeIndex(GaiaConstants.SpawnerResourceType.TerrainDetail, terrainDetailProtoIndex);

        //        m_resourceProtoFoldOutStatus[resourceIndex] = EditorGUILayout.Foldout(m_resourceProtoFoldOutStatus[resourceIndex], m_resource.m_detailPrototypes[terrainDetailProtoIndex].m_name);
        //        if (m_resourceProtoFoldOutStatus[resourceIndex])
        //        {
        //            DrawTerrainDetailPrototype(m_resource.m_detailPrototypes[terrainDetailProtoIndex], m_editorUtils, showHelp);
        //            if (m_editorUtils.Button("DeleteTerrainDetail"))
        //            {
        //                m_resource.m_detailPrototypes = GaiaUtils.RemoveArrayIndexAt<ResourceProtoDetail>(m_resource.m_detailPrototypes, terrainDetailProtoIndex);
        //                m_resourceProtoFoldOutStatus = GaiaUtils.RemoveArrayIndexAt<bool>(m_resourceProtoFoldOutStatus, resourceIndex);
        //                //Correct the index since we just removed one texture
        //                terrainDetailProtoIndex--;
        //            }

        //            //Rect maskRect;
        //            //m_resourceIndexBeingDrawn = terrainDetailProtoIndex;
        //            //if (m_resourceProtoMasksExpanded[resourceIndex])
        //            //{
        //            //    m_maskListBeingDrawn = m_resource.m_detailPrototypes[terrainDetailProtoIndex].m_imageMasks;
        //            //    maskRect = EditorGUILayout.GetControlRect(true, m_resourceProtoReorderableLists[resourceIndex].GetHeight());
        //            //    m_resourceProtoReorderableLists[resourceIndex].DoList(maskRect);
        //            //}
        //            //else
        //            //{
        //            //    int oldIndent = EditorGUI.indentLevel;
        //            //    EditorGUI.indentLevel = 1;
        //            //    m_resourceProtoMasksExpanded[resourceIndex] = EditorGUILayout.Foldout(m_resourceProtoMasksExpanded[resourceIndex], ImageMaskListEditor.PropertyCount("MaskSettings", m_resource.m_detailPrototypes[terrainDetailProtoIndex].m_imageMasks, m_editorUtils), true);
        //            //    maskRect = GUILayoutUtility.GetLastRect();
        //            //    EditorGUI.indentLevel = oldIndent;
        //            //}
        //        }
        //    }
        //    EditorGUI.indentLevel--;

        //    if (m_editorUtils.Button("AddTerrainDetail"))
        //    {
        //        m_resource.m_detailPrototypes = GaiaUtils.AddElementToArray<ResourceProtoDetail>(m_resource.m_detailPrototypes, new ResourceProtoDetail());
        //        m_resource.m_detailPrototypes[m_resource.m_detailPrototypes.Length - 1].m_name = "New Terrain Detail Prototype";
        //        m_resourceProtoFoldOutStatus = GaiaUtils.AddElementToArray<bool>(m_resourceProtoFoldOutStatus, false);
        //    }
        //}

        public static void DrawTerrainDetailPrototype(ResourceProtoDetail resourceProtoDetail, EditorUtils editorUtils, Spawner spawner, bool showHelp)
        {
            editorUtils.LabelField("TreeProtoHeadingPrefab", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            //string oldName = resourceProtoDetail.m_name;
            resourceProtoDetail.m_name = editorUtils.TextField("GameObjectProtoName", resourceProtoDetail.m_name, showHelp);
            //if (oldName != resourceProtoDetail.m_name)
            //{
            //    if (resourceProtoDetail.m_PWGrassSettingsObject != null)
            //    {
            //        resourceProtoDetail.m_PWGrassSettingsObject.name = resourceProtoDetail.m_name;
            //    }
            //}
            resourceProtoDetail.m_useInstancing = editorUtils.Toggle("DetailProtoUseInstancing", resourceProtoDetail.m_useInstancing, showHelp);

            if (!resourceProtoDetail.m_useInstancing)
            {
                resourceProtoDetail.m_renderMode = (DetailRenderMode)editorUtils.EnumPopup("DetailProtoRenderMode", resourceProtoDetail.m_renderMode, showHelp);

                if (resourceProtoDetail.m_renderMode == DetailRenderMode.VertexLit || resourceProtoDetail.m_renderMode == DetailRenderMode.Grass)
                {
                    resourceProtoDetail.m_detailProtoype = (GameObject)editorUtils.ObjectField("DetailProtoModel", resourceProtoDetail.m_detailProtoype, typeof(GameObject), false, showHelp);
#if ASSET_INVENTORY
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(EditorGUIUtility.labelWidth);
                        if (GUILayout.Button(editorUtils.GetContent("SearchAssetInventoryButton")))
                        {
                            ResultPickerUI window = ResultPickerUI.Show(path => { resourceProtoDetail.m_detailProtoype = (GameObject)LoadAssetFromPath(path); }, "Models");
                            window.instantSelection = false;
                            window.hideDetailsPane = false;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
#endif
                }
                if (resourceProtoDetail.m_renderMode != DetailRenderMode.VertexLit)
                {
                    resourceProtoDetail.m_detailTexture = (Texture2D)editorUtils.ObjectField("DetailProtoTexture", resourceProtoDetail.m_detailTexture, typeof(Texture2D), false, showHelp, GUILayout.MaxHeight(16));
#if ASSET_INVENTORY
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(EditorGUIUtility.labelWidth);
                        if (GUILayout.Button(editorUtils.GetContent("SearchAssetInventoryButton")))
                        {
                            ResultPickerUI window = ResultPickerUI.Show(path => { resourceProtoDetail.m_detailTexture = (Texture2D)LoadAssetFromPath(path); }, "Images");
                            window.instantSelection = false;
                            window.hideDetailsPane = false;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
#endif
                }
            }
            else
            {
                resourceProtoDetail.m_detailProtoype = (GameObject)editorUtils.ObjectField("DetailProtoModel", resourceProtoDetail.m_detailProtoype, typeof(GameObject), false, showHelp);
#if ASSET_INVENTORY
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(EditorGUIUtility.labelWidth);
                    if (GUILayout.Button(editorUtils.GetContent("SearchAssetInventoryButton")))
                    {
                        ResultPickerUI window = ResultPickerUI.Show(path => { resourceProtoDetail.m_detailProtoype = (GameObject)LoadAssetFromPath(path); }, "Prefabs");
                        window.instantSelection = false;
                        window.hideDetailsPane = false;
                    }
                }
                EditorGUILayout.EndHorizontal();
#endif
            }
            resourceProtoDetail.m_alignToGround = editorUtils.Slider("ProtoAlignToGround", resourceProtoDetail.m_alignToGround * 100, 0.0f, 100.0f) / 100.0f;
            resourceProtoDetail.m_positionJitter = editorUtils.Slider("ProtoPositionJitter", resourceProtoDetail.m_positionJitter, 0.0f, 100.0f);
            editorUtils.LabelField("ProtoSpawnScale", "ProtoSpawnScaleRandom", showHelp);
            editorUtils.MinMaxSliderWithFields("DetailProtoMinMaxWidth", ref resourceProtoDetail.m_minWidth, ref resourceProtoDetail.m_maxWidth, 0, 20, showHelp);
            editorUtils.MinMaxSliderWithFields("DetailProtoMinMaxHeight", ref resourceProtoDetail.m_minHeight, ref resourceProtoDetail.m_maxHeight, 0, 20, showHelp);
            resourceProtoDetail.m_noiseSeed = editorUtils.IntField("ProtoNoiseSeed", resourceProtoDetail.m_noiseSeed);
            resourceProtoDetail.m_noiseSpread = editorUtils.FloatField("DetailProtoNoiseSpread", resourceProtoDetail.m_noiseSpread, showHelp);
            resourceProtoDetail.m_density = editorUtils.Slider("ProtoDetailDensity", resourceProtoDetail.m_density, 0.0f, 5.0f);
            resourceProtoDetail.m_holeEdgePadding = editorUtils.Slider("ProtoHoleEdgePadding", resourceProtoDetail.m_holeEdgePadding, 0.0f, 100.0f);
            //resourceProtoDetail.m_bendFactor = editorUtils.FloatField("DetailProtoBendFactor", resourceProtoDetail.m_bendFactor, showHelp);
            resourceProtoDetail.m_healthyColour = editorUtils.ColorField("DetailProtoHealthyColour", resourceProtoDetail.m_healthyColour, showHelp);
            resourceProtoDetail.m_dryColour = editorUtils.ColorField("DetailProtoDryColour", resourceProtoDetail.m_dryColour, showHelp);
            resourceProtoDetail.m_useDensityScaling = editorUtils.Toggle("DetailProtoUseDensityScaling", resourceProtoDetail.m_useDensityScaling, showHelp);
            EditorGUI.indentLevel--;
        }

        public void DropAreaGUI()
        {
            //Drop out if no resource selected
            if (m_resource == null)
            {
                return;
            }

            //Ok - set up for drag and drop
            Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(drop_area, "Drop Game Objects / Prefabs Here", m_boxStyle);

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        //Work out if we have prefab instances or prefab objects
                        bool havePrefabInstances = false;
                        foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                        {
                            PrefabAssetType pt = PrefabUtility.GetPrefabAssetType(dragged_object);

                            if (pt == PrefabAssetType.Regular || pt == PrefabAssetType.Model)
                            {
                                havePrefabInstances = true;
                                break;
                            }
                        }

                        if (havePrefabInstances)
                        {
                            List<GameObject> prototypes = new List<GameObject>();

                            foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                            {
                                PrefabAssetType pt = PrefabUtility.GetPrefabAssetType(dragged_object);

                                if (pt == PrefabAssetType.Regular || pt == PrefabAssetType.Model)
                                {
                                    prototypes.Add(dragged_object as GameObject);
                                }
                                else
                                {
                                    Debug.LogWarning("You may only add prefab instances!");
                                }
                            }

                            //Same them as a single entity
                            if (prototypes.Count > 0)
                            {
                                m_resource.AddGameObject(prototypes);
                            }
                        }
                        else
                        {
                            foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                            {
                                if (PrefabUtility.GetPrefabAssetType(dragged_object) == PrefabAssetType.Regular)
                                {
                                    m_resource.AddGameObject(dragged_object as GameObject);
                                }
                                else
                                {
                                    Debug.LogWarning("You may only add prefabs or game objects attached to prefabs!");
                                }
                            }
                        }
                    }
                    break;
            }
        }


        /// <summary>
        /// Get the range from the terrain
        /// </summary>
        /// <returns>Range from currently active terrain or 1024f</returns>
        private float GetRangeFromTerrain()
        {
            float range = 1024f;
            Terrain t = Gaia.TerrainHelper.GetActiveTerrain();
            if (t != null)
            {
                range = Mathf.Max(t.terrainData.size.x, t.terrainData.size.z) / 2f;
            }
            return range;
        }

        /// <summary>
        /// Get texture increment from terrain
        /// </summary>
        /// <returns></returns>
        private float GetTextureIncrementFromTerrain()
        {
            float increment = 1f;
            Terrain t = Gaia.TerrainHelper.GetActiveTerrain();
            if (t != null)
            {
                if (t.terrainData != null)
                {
                    increment = Mathf.Max(t.terrainData.size.x, t.terrainData.size.z) / (float)t.terrainData.alphamapResolution;
                }
            }
            return increment;
        }

        /// <summary>
        /// Get detail increment from terrain
        /// </summary>
        /// <returns></returns>
        private float GetDetailIncrementFromTerrain()
        {
            float increment = 1f;
            Terrain t = Gaia.TerrainHelper.GetActiveTerrain();
            if (t != null)
            {
                if (t.terrainData != null)
                {
                    increment = Mathf.Max(t.terrainData.size.x, t.terrainData.size.z) / (float)t.terrainData.detailResolution;
                }
            }
            return increment;
        }


        /// <summary>
        /// Get a content label - look the tooltip up if possible
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        GUIContent GetLabel(string name)
        {
            string tooltip = "";
            if (m_tooltips.TryGetValue(name, out tooltip))
            {
                return new GUIContent(name, tooltip);
            }
            else
            {
                return new GUIContent(name);
            }
        }

        /// <summary>
        /// Tries to return an object of the given type from the given path. Will log an error and return null if not succesful.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        private static object LoadAssetFromPath(string path)
        {
            try
            {
                return AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Could not load the asset at the path {path}! Are object type and path correct? The Exception was: {ex.Message}, Stack Trace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// The tooltips
        /// </summary>
        static Dictionary<string, string> m_tooltips = new Dictionary<string, string>
        {
            { "Get From Terrain", "Get or update the resource prototypes from the current terrain." },
            { "Apply To Terrains", "Apply the resource prototypes into all existing terrains." },
            { "Visualise", "Visualise the fitness of resource prototypes." },
        };


    }
}
