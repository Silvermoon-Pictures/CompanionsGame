﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PWCommon5;
using Gaia.Internal;
using System;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Profiling;

namespace Gaia
{
    [CustomEditor(typeof(TerrainLoaderManager))]
    public class TerrainLoaderManagerEditor : PWEditor, IPWEditor
    {
        private TerrainLoaderManager m_terrainLoaderManager;
        #if GAIA_2023_PRO
        private TerrainLoader[] m_terrainLoaders;
        #endif
        private EditorUtils m_editorUtils;
        private GUIStyle m_terrainBoxStyle;
        private Terrain m_ingestTerrain;
        private static List<Texture2D> m_tempTextureList = new List<Texture2D>();
        private GUIStyle redStyle;
        private GUIStyle greenStyle;

        private GaiaSettings m_gaiaSettings;

        private GaiaSettings GaiaSettings
        {
            get
            {
                if (m_gaiaSettings == null)
                {
                    m_gaiaSettings = GaiaUtils.GetGaiaSettings();
                }
                return m_gaiaSettings;
            }
        }

        public void OnEnable()
        {
            m_terrainLoaderManager = (TerrainLoaderManager)target;
            m_terrainLoaderManager.m_assembliesAreReloading = false;
            m_terrainLoaderManager.SubscribeToAssemblyReloadEvents();
#if GAIA_2023_PRO
            m_terrainLoaders = Resources.FindObjectsOfTypeAll<TerrainLoader>();
            #endif
            //m_placeHolders = Resources.FindObjectsOfTypeAll<GaiaTerrainPlaceHolder>();
            //m_placeHolders = m_placeHolders.OrderBy(x => x.name).ToArray();
            //foreach (GaiaTerrainPlaceHolder placeHolder in m_placeHolders)
            //{
            //    placeHolder.UpdateLoadState();
            //}

            //Init editor utils
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }
            if (m_terrainLoaderManager.TerrainSceneStorage == null)
            {
                m_terrainLoaderManager.LoadStorageData();
            }
            m_terrainLoaderManager.LookUpLoadingScreen();
        }


        private void OnDestroy()
        {
            for (int i = 0; i < m_tempTextureList.Count; i++)
            {
                UnityEngine.Object.DestroyImmediate(m_tempTextureList[i]);
            }
        }


        public override void OnInspectorGUI()
        {
            if (redStyle == null || redStyle.normal.background == null || greenStyle == null || greenStyle.normal.background == null)
            {
                redStyle = new GUIStyle();
                redStyle.normal.background = GaiaUtils.GetBGTexture(Color.red, m_tempTextureList);

                greenStyle = new GUIStyle();
                greenStyle.normal.background = GaiaUtils.GetBGTexture(Color.green, m_tempTextureList);
            }


            //Init editor utils
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }
            m_editorUtils.Initialize(); // Do not remove this!
            
            if (GaiaUtils.HasDynamicLoadedTerrains())
            {
                m_editorUtils.Panel("GeneralSettings", DrawGeneralSettings, true);
                m_editorUtils.Panel("LoaderPanel", DrawLoaders, false);
                m_editorUtils.Panel("PlaceholderPanel", DrawTerrains, false);
            }
            else
            {
                EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("NoTerrainLoadingMessage"),MessageType.Info);
                m_editorUtils.Panel("GeneralSettings", DrawReducedGeneralSettings, true);
            }
            m_editorUtils.Panel("FloatingPointFix", DrawFloatingPointFix, false);
        }

        private void DrawReducedGeneralSettings(bool helpEnabled)
        {
            m_terrainLoaderManager.m_autoTerrainStitching = m_editorUtils.Toggle("AutoTerrainStitching", m_terrainLoaderManager.m_autoTerrainStitching, helpEnabled);
        }


        private void DrawFloatingPointFix(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            GaiaSettings.m_floatingPointFix = m_editorUtils.Toggle("FloatingPointFixEnabled", GaiaSettings.m_floatingPointFix, helpEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                if (GaiaSettings.m_floatingPointFix)
                {
                    if (EditorUtility.DisplayDialog("Add Floating Point Fix?", "This will activate a fix for floating point imprecisions in your scene. This will add aditional components to your player, and other objects in your scene. Continue?", "OK", "Cancel"))
                    {
                       TerrainLoaderManager.FloatingPointFix_Add();
                    }
                    else
                    {
                        GaiaSettings.m_floatingPointFix = false;
                    }
                }
                else
                {
                    if (EditorUtility.DisplayDialog("Remove Floating Point Fix?", "This will remove all floating point fix components in the scene. Continue?", "OK", "Cancel"))
                    {
                        TerrainLoaderManager.FloatingPointFix_Remove();
                    }
                    else
                    {
                        GaiaSettings.m_floatingPointFix = true;
                    }
                }
            }

        }

        private void DrawGeneralSettings(bool helpEnabled)
        {

            bool originalGUIState = GUI.enabled;

            m_terrainLoaderManager.TerrainSceneStorage = (TerrainSceneStorage)m_editorUtils.ObjectField("TerrainSceneStorage", m_terrainLoaderManager.TerrainSceneStorage, typeof(TerrainSceneStorage), false, helpEnabled);

            bool oldLoadingEnabled = m_terrainLoaderManager.TerrainSceneStorage.m_terrainLoadingEnabled;
            m_terrainLoaderManager.TerrainSceneStorage.m_terrainLoadingEnabled = m_editorUtils.Toggle("TerrainLoadingEnabled", m_terrainLoaderManager.TerrainSceneStorage.m_terrainLoadingEnabled, helpEnabled);
            if (!oldLoadingEnabled && m_terrainLoaderManager.TerrainSceneStorage.m_terrainLoadingEnabled)
            {
                //User re-enabled the loaders
                m_terrainLoaderManager.UpdateTerrainLoadState();
            }
            if (oldLoadingEnabled != m_terrainLoaderManager.TerrainSceneStorage.m_terrainLoadingEnabled)
            {
                //Value was changed, dirty the object to make sure the value is being saved
                EditorUtility.SetDirty(m_terrainLoaderManager.TerrainSceneStorage);
            }

#if !PW_ADDRESSABLES
            GUI.enabled = false;
#endif
            m_terrainLoaderManager.TerrainSceneStorage.m_useAddressables = m_editorUtils.Toggle("UseAddressables", m_terrainLoaderManager.TerrainSceneStorage.m_useAddressables, helpEnabled);
            if (m_terrainLoaderManager.TerrainSceneStorage.m_useAddressables)
            {
                EditorGUI.indentLevel++;
                m_terrainLoaderManager.TerrainSceneStorage.m_preloadAddressablesWithImpostors = m_editorUtils.Toggle("PreloadAddressablesWithImpostors", m_terrainLoaderManager.TerrainSceneStorage.m_preloadAddressablesWithImpostors, helpEnabled);
                EditorGUI.indentLevel--;
            }
            GUI.enabled = originalGUIState;

            m_terrainLoaderManager.m_terrainLoadingTresholdMS = m_editorUtils.IntField("LoadingTimeTreshold", m_terrainLoaderManager.m_terrainLoadingTresholdMS, helpEnabled);
            m_terrainLoaderManager.m_trackLoadingProgressTimeOut = m_editorUtils.LongField("LoadingProgressTimeout", m_terrainLoaderManager.m_trackLoadingProgressTimeOut, helpEnabled);
            Application.backgroundLoadingPriority = (ThreadPriority)m_editorUtils.EnumPopup("ApplicationLoadingPriority", Application.backgroundLoadingPriority, helpEnabled);
            GUILayout.Space(10);
            m_editorUtils.Heading("SceneViewLoading");
            m_terrainLoaderManager.CenterSceneViewLoadingOn = (CenterSceneViewLoadingOn)m_editorUtils.EnumPopup("CenterSceneViewLoadingOn", m_terrainLoaderManager.CenterSceneViewLoadingOn, helpEnabled);
            double loadingRange = m_editorUtils.DoubleField("SceneViewLoadingRange", m_terrainLoaderManager.GetLoadingRange(), helpEnabled);
            double impostorLoadingRange = m_terrainLoaderManager.GetImpostorLoadingRange();
            if (GaiaUtils.HasImpostorTerrains())
            {
                impostorLoadingRange = m_editorUtils.DoubleField("SceneViewImpostorLoadingRange", m_terrainLoaderManager.GetImpostorLoadingRange(), helpEnabled);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                m_editorUtils.Label("SceneViewImpostorLoadingRange");
                //offer button to create impostors setup
                if (m_editorUtils.Button("OpenTerrainMeshExporterForImpostors", GUILayout.Width(EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - 38)))
                {
                    TerrainConverterEditorWindow.OpenWithPreset("Create Impostors");
                }
                EditorGUILayout.EndHorizontal();

            }

            if (loadingRange != m_terrainLoaderManager.GetLoadingRange() || impostorLoadingRange != m_terrainLoaderManager.GetImpostorLoadingRange())
            {
                m_terrainLoaderManager.SetLoadingRange(loadingRange, impostorLoadingRange);
            }
            EditorGUI.BeginChangeCheck();
            m_terrainLoaderManager.m_autoToggleTerrainBoxes = m_editorUtils.Toggle("AutoShowTerrainBoxes", m_terrainLoaderManager.m_autoToggleTerrainBoxes);
            EditorGUI.BeginChangeCheck();
            m_terrainLoaderManager.m_showOriginTerrainBoxes = m_editorUtils.Toggle("ShowTerrainBoxes",m_terrainLoaderManager.m_showOriginTerrainBoxes);
            EditorGUI.indentLevel++;
            if (!m_terrainLoaderManager.m_showOriginTerrainBoxes)
            {
                GUI.enabled = false;
            }
            m_terrainLoaderManager.m_showOriginTerrainBoxesLoaded = m_editorUtils.Toggle("ShowTerrainBoxesLoaded", m_terrainLoaderManager.m_showOriginTerrainBoxesLoaded);
            m_terrainLoaderManager.m_showOriginTerrainBoxesUnLoaded = m_editorUtils.Toggle("ShowTerrainBoxesUnloaded", m_terrainLoaderManager.m_showOriginTerrainBoxesUnLoaded);
            GUI.enabled = originalGUIState;
            EditorGUI.indentLevel--;
            m_terrainLoaderManager.m_autoToggleGoToTerrainButton = m_editorUtils.Toggle("AutoToggleGoToTerrainButton", m_terrainLoaderManager.m_autoToggleGoToTerrainButton);
            m_terrainLoaderManager.m_showGoToTerrainButton = m_editorUtils.Toggle("ShowGoToTerrainButton", m_terrainLoaderManager.m_showGoToTerrainButton);

            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }


            GUILayout.Space(10);
            m_editorUtils.Heading("CachingSettings");
            m_editorUtils.InlineHelp("CachingSettings", helpEnabled);
            EditorGUI.BeginChangeCheck();
            m_terrainLoaderManager.m_cacheInRuntime = m_editorUtils.Toggle("CacheInRuntime", m_terrainLoaderManager.m_cacheInRuntime, helpEnabled);
            m_terrainLoaderManager.m_cacheInEditor = m_editorUtils.Toggle("CacheInEditor", m_terrainLoaderManager.m_cacheInEditor, helpEnabled);
            m_terrainLoaderManager.m_unloadUnusedAssetsRuntime = m_editorUtils.Toggle("UnloadAssetsRuntime", m_terrainLoaderManager.m_unloadUnusedAssetsRuntime, helpEnabled);
            m_terrainLoaderManager.m_unloadUnusedAssetsEditor = m_editorUtils.Toggle("UnloadAssetsEditor", m_terrainLoaderManager.m_unloadUnusedAssetsEditor, helpEnabled);
            string allocatedMegabytes = (Math.Round(Profiler.GetTotalAllocatedMemoryLong() / Math.Pow(1024, 2))).ToString();
            string availableMegabytes = SystemInfo.systemMemorySize.ToString();
            allocatedMegabytes = allocatedMegabytes.PadLeft(allocatedMegabytes.Length + (availableMegabytes.Length - allocatedMegabytes.Length) * 3 , ' ');

            GUIStyle style = new GUIStyle(GUI.skin.label);

            if (m_terrainLoaderManager.m_cacheMemoryThreshold < Profiler.GetTotalAllocatedMemoryLong())
            {
                style = redStyle;
            }
            else
            {
                style = greenStyle;
            }
            m_editorUtils.LabelField("MemoryAvailable", new GUIContent(availableMegabytes), helpEnabled);
            m_editorUtils.LabelField("MemoryAllocated", new GUIContent(allocatedMegabytes), style, helpEnabled);

            //GUI.color = originalGUIColor;

            if (m_terrainLoaderManager.m_cacheInRuntime || m_terrainLoaderManager.m_cacheInEditor)
            {
                m_terrainLoaderManager.m_cacheMemoryThresholdPreset = (CacheSizePreset)m_editorUtils.EnumPopup("CacheMemoryThreshold", m_terrainLoaderManager.m_cacheMemoryThresholdPreset, helpEnabled);
                if (m_terrainLoaderManager.m_cacheMemoryThresholdPreset == CacheSizePreset.Custom)
                {
                    EditorGUI.indentLevel++;
                    m_terrainLoaderManager.m_cacheMemoryThreshold = m_editorUtils.LongField("ThresholdInBytes", m_terrainLoaderManager.m_cacheMemoryThreshold, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                int cacheKeepAliveSeconds = (int)m_terrainLoaderManager.m_cacheKeepAliveTime / 1000;
                cacheKeepAliveSeconds = m_editorUtils.IntField("CacheKeepAliveSeconds", cacheKeepAliveSeconds, helpEnabled);
                m_terrainLoaderManager.m_cacheKeepAliveTime = cacheKeepAliveSeconds * 1000;
            }
            if (EditorGUI.EndChangeCheck())
            {
                if (m_terrainLoaderManager.m_cacheMemoryThresholdPreset != CacheSizePreset.Custom)
                {
                    m_terrainLoaderManager.m_cacheMemoryThreshold = (long)((int)m_terrainLoaderManager.m_cacheMemoryThresholdPreset * Math.Pow(1024, 3));
                }
                m_terrainLoaderManager.UpdateCaching();
            }



            
            GUILayout.Space(10);

            if (!GaiaUtils.HasColliderTerrains())
            {
                GUI.enabled = false;
            }

            m_editorUtils.Heading("ColliderOnlyLoadingHeader");
            //This flag is special in so far as that when the user switches it, we must first perform a scene unload while the old value is still active
            //then change the flag in the terrain scene storage and then do a refresh with the new setting applied.
            bool colliderLoadingEnabled = m_terrainLoaderManager.TerrainSceneStorage.m_colliderOnlyLoading;
            colliderLoadingEnabled = m_editorUtils.Toggle("ColliderOnlyLoadingEnabled", colliderLoadingEnabled, helpEnabled);
            if (colliderLoadingEnabled != m_terrainLoaderManager.TerrainSceneStorage.m_colliderOnlyLoading)
            {
                //User changed the flag, do an unload with the old setting
                m_terrainLoaderManager.UnloadAll(true);
                //then change the actual flag in storage
                m_terrainLoaderManager.TerrainSceneStorage.m_colliderOnlyLoading = colliderLoadingEnabled;
                //now do a refresh under the new setting
                m_terrainLoaderManager.RefreshSceneViewLoadingRange();

                //Add the required scenes to build settings
                if (colliderLoadingEnabled)
                {
                    GaiaSessionManager.AddOnlyColliderScenesToBuildSettings(TerrainLoaderManager.TerrainScenes);
                }
                else
                {
                    GaiaSessionManager.AddTerrainScenesToBuildSettings(TerrainLoaderManager.TerrainScenes);
                }
            }

            GUI.enabled = originalGUIState;

            if (colliderLoadingEnabled)
            {
                EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("ColliderOnlyLoadingInfo"), MessageType.Info);
                m_editorUtils.Heading("DeactivateRuntimeHeader");
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                m_terrainLoaderManager.TerrainSceneStorage.m_deactivateRuntimePlayer = m_editorUtils.Toggle("DeactivateRuntimePlayer", m_terrainLoaderManager.TerrainSceneStorage.m_deactivateRuntimePlayer, helpEnabled);
                m_terrainLoaderManager.TerrainSceneStorage.m_deactivateRuntimeLighting = m_editorUtils.Toggle("DeactivateRuntimeLighting", m_terrainLoaderManager.TerrainSceneStorage.m_deactivateRuntimeLighting, helpEnabled);
                m_terrainLoaderManager.TerrainSceneStorage.m_deactivateRuntimeAudio = m_editorUtils.Toggle("DeactivateRuntimeAudio", m_terrainLoaderManager.TerrainSceneStorage.m_deactivateRuntimeAudio, helpEnabled);
                m_terrainLoaderManager.TerrainSceneStorage.m_deactivateRuntimeWeather = m_editorUtils.Toggle("DeactivateRuntimeWeather", m_terrainLoaderManager.TerrainSceneStorage.m_deactivateRuntimeWeather, helpEnabled);
                m_terrainLoaderManager.TerrainSceneStorage.m_deactivateRuntimeWater = m_editorUtils.Toggle("DeactivateRuntimeWater", m_terrainLoaderManager.TerrainSceneStorage.m_deactivateRuntimeWater, helpEnabled);
                m_terrainLoaderManager.TerrainSceneStorage.m_deactivateRuntimeScreenShotter = m_editorUtils.Toggle("DeactivateRuntimeScreenShotter", m_terrainLoaderManager.TerrainSceneStorage.m_deactivateRuntimeScreenShotter, helpEnabled);
                if (EditorGUI.EndChangeCheck())
                {
                    m_terrainLoaderManager.SaveStorageData();
                }
                EditorGUI.indentLevel--;
            }
            else
            {
                //offer button to create collider setup
                if (m_editorUtils.Button("OpenTerrainMeshExporterForColliders"))
                {
                    ExportTerrain exportTerrainWindow = EditorWindow.GetWindow<ExportTerrain>();
                    exportTerrainWindow.FindAndSetPreset("Collider");
                    exportTerrainWindow.m_settings.m_customSettingsFoldedOut = false;
                }
            }
        }

        private void DrawTerrains(bool helpEnabled)
        {
            bool originalGUIState = GUI.enabled;

#if GAIA_2023_PRO
            EditorGUILayout.BeginHorizontal();
            m_editorUtils.Label("IngestTerrain", GUILayout.Width(100));
            m_ingestTerrain = (Terrain)EditorGUILayout.ObjectField(m_ingestTerrain, typeof(Terrain), true);
            if (m_editorUtils.Button("Ingest"))
            {
                ////Try to find the X and Z coordinate for the scene name
                //double minX = m_terrainLoaderManager.TerrainSceneStorage.m_terrainScenes.Select(x => x.m_pos.x).Min();
                //double maxX = m_terrainLoaderManager.TerrainSceneStorage.m_terrainScenes.Select(x => x.m_pos.x).Max();
                //double minZ = m_terrainLoaderManager.TerrainSceneStorage.m_terrainScenes.Select(x => x.m_pos.z).Min();
                //double maxZ = m_terrainLoaderManager.TerrainSceneStorage.m_terrainScenes.Select(x => x.m_pos.z).Max();
                GaiaSessionManager sessionManager = GaiaSessionManager.GetSessionManager();
                WorldCreationSettings worldCreationSettings = new WorldCreationSettings()
                {
                    m_autoUnloadScenes = false,
                    m_addLoadingScreen = false,
                    m_applyFloatingPointFix = TerrainLoaderManager.Instance.TerrainSceneStorage.m_useFloatingPointFix,
                    m_isWorldMap = false
                };
                TerrainScene newScene = TerrainSceneCreator.CreateTerrainScene(m_ingestTerrain.gameObject.scene, TerrainLoaderManager.Instance.TerrainSceneStorage, sessionManager.m_session, m_ingestTerrain.gameObject, worldCreationSettings);
                TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainScenes.Add(newScene);
                TerrainLoaderManager.Instance.SaveStorageData();
                GaiaSessionManager.AddTerrainScenesToBuildSettings(new List<TerrainScene>() { newScene});
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
#endif

            EditorGUILayout.BeginHorizontal();
            if (m_editorUtils.Button("AddToBuildSettings"))
            {
                if (TerrainLoaderManager.ColliderOnlyLoadingActive)
                {
                    if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("AddColliderScenesToBuildSettingsPopupTitle"), m_editorUtils.GetTextValue("AddColliderScenesToBuildSettingsPopupText"), m_editorUtils.GetTextValue("Continue"), m_editorUtils.GetTextValue("Cancel")))
                    {
#if GAIA_2023_PRO
                        GaiaSessionManager.AddOnlyColliderScenesToBuildSettings(m_terrainLoaderManager.TerrainSceneStorage.m_terrainScenes);
#endif
                        EditorGUIUtility.ExitGUI();
                    }
                }
                else
                {
                    if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("AddToBuildSettingsPopupTitle"), m_editorUtils.GetTextValue("AddToBuildSettingsPopupText"), m_editorUtils.GetTextValue("Continue"), m_editorUtils.GetTextValue("Cancel")))
                    {
#if GAIA_2023_PRO
                        GaiaSessionManager.AddTerrainScenesToBuildSettings(m_terrainLoaderManager.TerrainSceneStorage.m_terrainScenes);
#endif
                        EditorGUIUtility.ExitGUI();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (m_editorUtils.Button("AddOnlyLoadedToBuildSettings"))
            {
                if (TerrainLoaderManager.ColliderOnlyLoadingActive)
                {
                    if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("AddColliderScenesToBuildSettingsPopupTitle"), m_editorUtils.GetTextValue("AddOnlyLoadedColliderScenesToBuildSettingsPopupText"), m_editorUtils.GetTextValue("Continue"), m_editorUtils.GetTextValue("Cancel")))
                    {
#if GAIA_2023_PRO
                        GaiaSessionManager.AddOnlyColliderScenesToBuildSettings(m_terrainLoaderManager.TerrainSceneStorage.m_terrainScenes.Where(x=>x.m_regularLoadState == LoadState.Loaded).ToList());
#endif
                        EditorGUIUtility.ExitGUI();
                    }
                }
                else
                {
                    if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("AddToBuildSettingsPopupTitle"), m_editorUtils.GetTextValue("AddOnlyLoadedToBuildSettingsPopupText"), m_editorUtils.GetTextValue("Continue"), m_editorUtils.GetTextValue("Cancel")))
                    {
#if GAIA_2023_PRO
                        GaiaSessionManager.AddTerrainScenesToBuildSettings(m_terrainLoaderManager.TerrainSceneStorage.m_terrainScenes.Where(x => x.m_regularLoadState == LoadState.Loaded).ToList());
#endif
                        EditorGUIUtility.ExitGUI();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (m_editorUtils.Button("UnloadAll"))
            {
                m_terrainLoaderManager.SetLoadingRange(0, 0);
                m_terrainLoaderManager.UnloadAll(true);
            }
            if (m_editorUtils.Button("LoadAll"))
            {
                if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("LoadAllPopupTitle"), m_editorUtils.GetTextValue("LoadAllPopupText"), m_editorUtils.GetTextValue("Continue"), m_editorUtils.GetTextValue("Cancel")))
                {
                    foreach (TerrainScene terrainScene in m_terrainLoaderManager.TerrainSceneStorage.m_terrainScenes)
                    {
                        m_terrainLoaderManager.SetLoadingRange(100000, m_terrainLoaderManager.GetImpostorLoadingRange());
                        terrainScene.AddRegularReference(m_terrainLoaderManager.gameObject);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            if (!GaiaUtils.HasImpostorTerrains())
            {
                GUI.enabled = false;            
            }
            EditorGUILayout.BeginHorizontal();
            if (m_editorUtils.Button("UnloadAllImpostors"))
            {
                m_terrainLoaderManager.SetLoadingRange(m_terrainLoaderManager.GetLoadingRange(), 0);
                m_terrainLoaderManager.UnloadAllImpostors(true);
            }
            if (m_editorUtils.Button("LoadAllImpostors"))
            {
                if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("LoadAllPopupTitle"), m_editorUtils.GetTextValue("LoadAllImpostorsPopupText"), m_editorUtils.GetTextValue("Continue"), m_editorUtils.GetTextValue("Cancel")))
                {
                    m_terrainLoaderManager.SetLoadingRange(m_terrainLoaderManager.GetLoadingRange(), 100000);
                    foreach (TerrainScene terrainScene in m_terrainLoaderManager.TerrainSceneStorage.m_terrainScenes)
                    {
                        terrainScene.AddImpostorReference(m_terrainLoaderManager.gameObject);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            GUI.enabled = originalGUIState;

            GUILayout.Space(EditorGUIUtility.singleLineHeight);

            float buttonWidth1 = 110;
            float buttonWidth2 = 60;

            if (m_terrainBoxStyle == null || m_terrainBoxStyle.normal.background == null)
            {
                m_terrainBoxStyle = new GUIStyle(EditorStyles.helpBox);
                m_terrainBoxStyle.margin = new RectOffset(0, 0, 0, 0);
                m_terrainBoxStyle.padding = new RectOffset(3, 3, 3, 3);
            }

            int removeIndex = -99;
            int removeImpostorIndex = -99;
            int currentIndex = 0;

            foreach (TerrainScene terrainScene in m_terrainLoaderManager.TerrainSceneStorage.m_terrainScenes)
            {
                EditorGUILayout.BeginVertical(m_terrainBoxStyle);
                {
                    EditorGUILayout.LabelField(terrainScene.GetTerrainName());
                    EditorGUILayout.BeginHorizontal();
                    bool isLoaded = terrainScene.m_regularLoadState == LoadState.Loaded && terrainScene.TerrainObj != null && terrainScene.TerrainObj.activeInHierarchy;
                    bool isImpostorLoaded = terrainScene.m_impostorLoadState == LoadState.Loaded || terrainScene.m_impostorLoadState == LoadState.Cached;

                    bool currentGUIState = GUI.enabled;
                    GUI.enabled = isLoaded;
                    if (m_editorUtils.Button("SelectPlaceholder", GUILayout.Width(buttonWidth1)))
                    {
                        Selection.activeGameObject = GameObject.Find(terrainScene.GetTerrainName());
                        EditorGUIUtility.PingObject(Selection.activeObject);
                    }
                    GUI.enabled = currentGUIState;
                    if (isLoaded)
                    {
                        if (m_editorUtils.Button("UnloadPlaceholder", GUILayout.Width(buttonWidth2)))
                        {
                            if (ResetToWorldOriginLoading())
                            {
                                terrainScene.RemoveAllReferences(true);
                            }
                        }
                    }
                    else
                    {
                        if (m_editorUtils.Button("LoadPlaceholder", GUILayout.Width(buttonWidth2)))
                        {
                            if (ResetToWorldOriginLoading())
                            {
                                terrainScene.AddRegularReference(m_terrainLoaderManager.gameObject);
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(terrainScene.m_impostorScenePath))
                    {
                        GUI.enabled = false;
                    }

                    if (isImpostorLoaded)
                    {
                        if (m_editorUtils.Button("UnLoadImpostor", GUILayout.Width(buttonWidth1)))
                        {
                            if (ResetToWorldOriginLoading())
                            {
                                terrainScene.RemoveImpostorReference(m_terrainLoaderManager.gameObject, 0, true);
                            }
                        }
                    }
                    else
                    {
                        if (m_editorUtils.Button("LoadImpostor", GUILayout.Width(buttonWidth1)))
                        {
                            if (ResetToWorldOriginLoading())
                            {
                                terrainScene.AddImpostorReference(m_terrainLoaderManager.gameObject);
                            }
                        }
                    }

                    GUI.enabled = originalGUIState;

                    if (m_editorUtils.Button("RemoveScene"))
                    {
                        if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("RemoveSceneTitle"), m_editorUtils.GetTextValue("RemoveSceneText"), m_editorUtils.GetTextValue("Continue"), m_editorUtils.GetTextValue("Cancel")))
                        {
                            removeIndex = currentIndex;
                        }
                    }

                    if (string.IsNullOrEmpty(terrainScene.m_impostorScenePath))
                    {
                        GUI.enabled = false;
                    }

                        if (m_editorUtils.Button("RemoveImpostor"))
                    {
                        if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("RemoveImpostorTitle"), m_editorUtils.GetTextValue("RemoveImpostorText"), m_editorUtils.GetTextValue("Continue"), m_editorUtils.GetTextValue("Cancel")))
                        {
                            removeImpostorIndex = currentIndex;
                        }
                    }

                    GUI.enabled = originalGUIState;

                    EditorGUILayout.EndHorizontal();
                    if (terrainScene.RegularReferences.Count > 0)
                    {
                        EditorGUI.indentLevel++;
                        terrainScene.m_isFoldedOut = m_editorUtils.Foldout(terrainScene.m_isFoldedOut, "ShowTerrainReferences");
                        if (terrainScene.m_isFoldedOut)
                        {

                            foreach (GameObject go in terrainScene.RegularReferences)
                            {
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(20);
                                m_editorUtils.Label(new GUIContent(go.name, m_editorUtils.GetTextValue("TerrainReferenceToolTip")));
                                if (m_editorUtils.Button("TerrainReferenceSelect", GUILayout.Width(buttonWidth1)))
                                {
                                    Selection.activeObject = go;
                                    SceneView.lastActiveSceneView.FrameSelected();
                                }
                                if (m_editorUtils.Button("TerrainReferenceRemove", GUILayout.Width(buttonWidth2)))
                                {
                                    terrainScene.RemoveRegularReference(go);
                                }
                                GUILayout.Space(100);
                                EditorGUILayout.EndHorizontal();
                            }

                        }
                        EditorGUI.indentLevel--;
                    }
                    if (terrainScene.ImpostorReferences.Count > 0)
                    {
                        EditorGUI.indentLevel++;
                        terrainScene.m_isFoldedOut = m_editorUtils.Foldout(terrainScene.m_isFoldedOut, "ShowImpostorReferences");
                        if (terrainScene.m_isFoldedOut)
                        {

                            foreach (GameObject go in terrainScene.ImpostorReferences)
                            {
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(20);
                                m_editorUtils.Label(new GUIContent(go.name, m_editorUtils.GetTextValue("TerrainReferenceToolTip")));
                                if (m_editorUtils.Button("TerrainReferenceSelect", GUILayout.Width(buttonWidth1)))
                                {
                                    Selection.activeObject = go;
                                    SceneView.lastActiveSceneView.FrameSelected();
                                }
                                if (m_editorUtils.Button("TerrainReferenceRemove", GUILayout.Width(buttonWidth2)))
                                {
                                    terrainScene.RemoveImpostorReference(go);
                                }
                                GUILayout.Space(100);
                                EditorGUILayout.EndHorizontal();
                            }

                        }
                        EditorGUI.indentLevel--;
                    }
                }
                GUILayout.EndVertical();
                GUILayout.Space(5f);
                currentIndex++;
            }

            if (removeIndex != -99)
            {
                m_terrainLoaderManager.TerrainSceneStorage.m_terrainScenes[removeIndex].RemoveAllReferences(true);
                AssetDatabase.DeleteAsset(m_terrainLoaderManager.TerrainSceneStorage.m_terrainScenes[removeIndex].m_scenePath);
                m_terrainLoaderManager.TerrainSceneStorage.m_terrainScenes.RemoveAt(removeIndex);
                m_terrainLoaderManager.SaveStorageData();
            }

            if (removeImpostorIndex != -99)
            {
                m_terrainLoaderManager.TerrainSceneStorage.m_terrainScenes[removeImpostorIndex].RemoveAllImpostorReferences(true);
                AssetDatabase.DeleteAsset(m_terrainLoaderManager.TerrainSceneStorage.m_terrainScenes[removeImpostorIndex].m_impostorScenePath);
                m_terrainLoaderManager.TerrainSceneStorage.m_terrainScenes[removeImpostorIndex].m_impostorScenePath = "";
                m_terrainLoaderManager.SaveStorageData();
            }



        }

        /// <summary>
        /// Checks if the scene view loading is currently set to center on world origin, if not, prompts the user to switch.
        /// Used when loading in and out single terrains which would clash with the scene view camera loading mode.
        /// </summary>
        /// <returns>True if the scene was already on World Origin mode, or if the switch was performed.</returns>
        private bool ResetToWorldOriginLoading()
        {
            if (m_terrainLoaderManager.CenterSceneViewLoadingOn != CenterSceneViewLoadingOn.WorldOrigin)
            {
                if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("ManualLoadPopUpTitle"), m_editorUtils.GetTextValue("ManualLoadPopUpText"), m_editorUtils.GetTextValue("Continue"), m_editorUtils.GetTextValue("Cancel")))
                {
                    m_terrainLoaderManager.CenterSceneViewLoadingOn = CenterSceneViewLoadingOn.WorldOrigin;
                    m_terrainLoaderManager.SetLoadingRange(0,0);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private void DrawLoaders(bool helpEnabled)
        {
            bool originalGUIState = GUI.enabled;
            EditorGUIUtility.labelWidth = 20;
#if GAIA_2023_PRO
            foreach (TerrainLoader terrainLoader in m_terrainLoaders)
            {
                if (terrainLoader != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(terrainLoader.name);
                    terrainLoader.LoadMode = (LoadMode)EditorGUILayout.EnumPopup(terrainLoader.LoadMode);
                    if (m_editorUtils.Button("SelectLoader", GUILayout.MaxWidth(100)))
                    {
                        Selection.activeGameObject = terrainLoader.gameObject;
                        EditorGUIUtility.PingObject(Selection.activeObject);

                        //Try to find out which kind of Gaia Tool that is, and open / highlight the terrain loading settings where appropiate
                        Stamper stamper = terrainLoader.gameObject.GetComponent<Stamper>();
                        if (stamper != null)
                        {
                            stamper.HighlightLoadingSettings();
                        }

                        BiomeController biomeController = terrainLoader.gameObject.GetComponent<BiomeController>();
                        if (biomeController != null)
                        {
                            biomeController.HighlightLoadingSettings();
                        }

                        Spawner spawner = terrainLoader.gameObject.GetComponent<Spawner>();
                        if (spawner != null)
                        {
                            spawner.HighlightLoadingSettings();
                        }

                        MaskMapExport maskMapExport = terrainLoader.gameObject.GetComponent<MaskMapExport>();
                        if (maskMapExport != null)
                        {
                            maskMapExport.HighlightLoadingSettings();
                        }

                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
#endif
            EditorGUIUtility.labelWidth = 0;
        }
    }
}
