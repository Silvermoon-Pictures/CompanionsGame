﻿using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gaia
{
    public class WorldMap : MonoBehaviour
    {
        public Terrain m_worldMapTerrain;
        public GaiaConstants.EnvironmentSize m_worldMapTileSize = GaiaConstants.EnvironmentSize.Is2048MetersSq;
        public GaiaConstants.HeightmapResolution m_heightmapResolution = GaiaConstants.HeightmapResolution._2049;

        public void DeactivateWorldMap()
        {
            foreach (Transform t in transform)
            {
                t.gameObject.SetActive(false);
            }
        }

        public void ActivateWorldMap()
        {
            foreach (Transform t in transform)
            {
                t.gameObject.SetActive(true);
            }
        }

        public void LookAtWorldMap()
        {
#if UNITY_EDITOR
            //Adjust the scene view so you can see the terrain
            if (SceneView.lastActiveSceneView != null)
            {
                if (m_worldMapTerrain != null)
                {
                    SceneView.lastActiveSceneView.LookAtDirect(new Vector3(m_worldMapTerrain.transform.position.x + (m_worldMapTerrain.terrainData.size.x / 2f), 300f, -1f * (m_worldMapTerrain.terrainData.size.x / 2f)), Quaternion.Euler(30f, 0f, 0f));

                }
            }
#endif
        }

        /// <summary>
        /// Syncs the heightmap of the world map to the local terrain tiles, preserving correct height scale, heightmap resolution, etc.
        /// </summary>
        /// <param name="validLocalTerrainNames">A list of local terrain tile names that are valid to change for the sync operation. If the list is null, all tiles will be assumed valid.</param>
        public void SyncWorldMapToLocalMap(List<string> validLocalTerrainNames = null)
        {
            BoundsDouble bounds = new BoundsDouble();
            TerrainHelper.GetTerrainBounds(ref bounds);
            if (GaiaUtils.HasDynamicLoadedTerrains())
            {
                Action<Terrain> act = (t) => CopyWorldMapToLocalMap(bounds, t);
                GaiaUtils.CallFunctionOnDynamicLoadedTerrains(act, false, validLocalTerrainNames);
            }
            else
            {
                foreach (Terrain t in Terrain.activeTerrains)
                {
                    if (t != m_worldMapTerrain)
                    {
                        if (validLocalTerrainNames == null || validLocalTerrainNames.Contains(t.name))
                        {
                            CopyWorldMapToLocalMap(bounds, t);
                        }
                    }
                }
            }
        }


        public void SyncLocalMapToWorldMap()
        {
            BoundsDouble bounds = new BoundsDouble();
            TerrainHelper.GetTerrainBounds(ref bounds);
            if (GaiaUtils.HasDynamicLoadedTerrains())
            {
                Action<Terrain> act = (t) => CopyLocalMapToWorldMap(bounds, t);
                GaiaUtils.CallFunctionOnDynamicLoadedTerrains(act, false);
            }
            else
            {
                foreach (Terrain t in Terrain.activeTerrains)
                {
                    if (t != m_worldMapTerrain)
                    {
                        CopyLocalMapToWorldMap(bounds, t);
                    }
                }
            }
        }

        private void CopyWorldMapToLocalMap(BoundsDouble bounds, Terrain t)
        {
            //make sure we have a world map terrain first
            if (m_worldMapTerrain == null)
            {
                m_worldMapTerrain = TerrainHelper.GetWorldMapTerrain();
            }

            if (m_worldMapTerrain == null)
            {
                Debug.LogError("Can't export world map to local terrains - world map terrain is missing!");
                return;
            }

            RenderTexture chunkContent = RenderTexture.GetTemporary(t.terrainData.heightmapTexture.descriptor);
            //FilterMode oldFilterMode = m_worldMapTerrain.terrainData.heightmapTexture.filterMode;
            //m_worldMapTerrain.terrainData.heightmapTexture.filterMode = FilterMode.Trilinear;
            //t.terrainData.heightmapTexture.filterMode = FilterMode.Trilinear;
            //chunkContent.filterMode = FilterMode.Trilinear;
            int maxTilesX = Mathf.RoundToInt((float)bounds.size.x / t.terrainData.size.x);
            int maxTilesZ = Mathf.RoundToInt((float)bounds.size.z / t.terrainData.size.z);
            int currentTileX = Mathf.RoundToInt((t.transform.position.x - (float)bounds.min.x) / t.terrainData.size.x);
            int currentTileZ = Mathf.RoundToInt((t.transform.position.z - (float)bounds.min.z) / t.terrainData.size.z);
            float res = (t.terrainData.heightmapResolution) / ((float)bounds.size.x / t.terrainData.bounds.size.x * (t.terrainData.heightmapResolution-1));
        

            Bounds worldSpaceBounds = t.terrainData.bounds;
            worldSpaceBounds.center = new Vector3(worldSpaceBounds.center.x + t.transform.position.x, worldSpaceBounds.center.y + t.transform.position.y, worldSpaceBounds.center.z + t.transform.position.z);
            float xPos = ((float)currentTileX * t.terrainData.heightmapResolution) / (maxTilesX * t.terrainData.heightmapResolution);
            float zPos = ((float)currentTileZ * t.terrainData.heightmapResolution) / (maxTilesZ * t.terrainData.heightmapResolution);
            Vector2 pos = new Vector2(xPos, zPos);

            
            
            Graphics.Blit(m_worldMapTerrain.terrainData.heightmapTexture, chunkContent, new Vector2(res, res), pos);
            //m_worldMapTerrain.terrainData.heightmapTexture.filterMode = oldFilterMode;
            RenderTexture previousRT = RenderTexture.active;
            RenderTexture.active = chunkContent;
            t.terrainData.CopyActiveRenderTextureToHeightmap(new RectInt(0, 0, t.terrainData.heightmapResolution, t.terrainData.heightmapResolution), new Vector2Int(0, 0), t.drawInstanced ? TerrainHeightmapSyncControl.None : TerrainHeightmapSyncControl.HeightOnly);
            RenderTexture.active = previousRT;
            t.terrainData.SyncHeightmap();
            t.editorRenderFlags = TerrainRenderFlags.All;

            RenderTexture.ReleaseTemporary(chunkContent);
            //chunkContent = null;

        }


        private void CopyLocalMapToWorldMap(BoundsDouble bounds, Terrain t)
        {
            RenderTextureDescriptor rtDesc = t.terrainData.heightmapTexture.descriptor;
            rtDesc.width = Mathf.CeilToInt(m_worldMapTerrain.terrainData.heightmapResolution / ((float)bounds.size.x / t.terrainData.bounds.size.x));
            rtDesc.height = rtDesc.width;

            RenderTexture chunkContent = RenderTexture.GetTemporary(rtDesc);
            float res = t.terrainData.heightmapResolution / rtDesc.width;

            Bounds worldSpaceBounds = t.terrainData.bounds;
            worldSpaceBounds.center = new Vector3(worldSpaceBounds.center.x + t.transform.position.x, worldSpaceBounds.center.y + t.transform.position.y, worldSpaceBounds.center.z + t.transform.position.z);
            Vector2 pos = new Vector2(Mathf.InverseLerp(0, (float)bounds.size.x, Mathf.Abs((float)bounds.min.x - worldSpaceBounds.min.x)), Mathf.InverseLerp(0, (float)bounds.size.z, Mathf.Abs((float)bounds.min.z - worldSpaceBounds.min.z)));

            Graphics.Blit(t.terrainData.heightmapTexture, chunkContent, new Vector2(1, 1), new Vector2(0, 0));

            RenderTexture previousRT = RenderTexture.active;
            RenderTexture.active = chunkContent;
            m_worldMapTerrain.terrainData.CopyActiveRenderTextureToHeightmap(new RectInt(0, 0, rtDesc.width, rtDesc.height), new Vector2Int(Mathf.FloorToInt(pos.x * m_worldMapTerrain.terrainData.heightmapResolution), Mathf.FloorToInt(pos.y * m_worldMapTerrain.terrainData.heightmapResolution)), t.drawInstanced ? TerrainHeightmapSyncControl.None : TerrainHeightmapSyncControl.HeightOnly);
            RenderTexture.active = previousRT;
            m_worldMapTerrain.terrainData.SyncHeightmap();
            m_worldMapTerrain.editorRenderFlags = TerrainRenderFlags.All;

            RenderTexture.ReleaseTemporary(chunkContent);
            //chunkContent = null;

        }

        public static void ShowWorldMapStampSpawner()
        {

            GaiaSessionManager gsm = GaiaSessionManager.GetSessionManager(false);

            //Get the Gaia Settings
            GaiaSettings settings = GaiaUtils.GetGaiaSettings();

            //Create or find the stamper
            GameObject spawnerObj = GaiaUtils.GetOrCreateWorldDesigner();
            Spawner spawner = spawnerObj.GetComponent<WorldDesigner>();
            if (spawner == null)
            {
                spawner = spawnerObj.AddComponent<WorldDesigner>();

                if (settings != null)
                {
                    //set up the export settings with the current Gaia Settings / defaults
                    spawner.m_worldCreationSettings.m_targetSizePreset = settings.m_targeSizePreset;
                    spawner.m_worldCreationSettings.m_xTiles = settings.m_tilesX;
                    spawner.m_worldCreationSettings.m_zTiles = settings.m_tilesZ;
                    spawner.m_worldTileSize = GaiaUtils.IntToEnvironmentSize(settings.m_currentDefaults.m_terrainSize);
                    spawner.m_worldCreationSettings.m_tileSize = settings.m_currentDefaults.m_terrainSize;
                    spawner.m_worldCreationSettings.m_tileHeight = settings.m_currentDefaults.m_terrainHeight;
                    spawner.m_worldCreationSettings.m_createInScene = settings.m_createTerrainScenes;
                    spawner.m_worldCreationSettings.m_autoUnloadScenes = settings.m_unloadTerrainScenes;
                    spawner.m_worldCreationSettings.m_addLoadingScreen = settings.m_addLoadingScreen;
                    spawner.m_worldCreationSettings.m_applyFloatingPointFix = settings.m_floatingPointFix;
                    spawner.m_worldCreationSettings.m_qualityPreset = settings.m_currentEnvironment;
                    if (spawner.m_worldCreationSettings.m_gaiaDefaults == null)
                    {
                        spawner.m_worldCreationSettings.m_gaiaDefaults = settings.m_currentDefaults;
                    }

                    spawner.StoreWorldSize();
                    spawner.m_worldDesignerBiomePresetSelection = settings.m_biomePresetSelection;
                    spawner.m_BiomeSpawnersToCreate = settings.m_BiomeSpawnersToCreate;

                    if (settings.m_defaultStampSpawnSettings != null)
                    {
                        spawner.LoadSettings(settings.m_defaultStampSpawnSettings);
                    }
                }
                spawner.m_showBoundingBox = false;
                spawner.m_settings.m_isWorldmapSpawner = true;
                //spawner.m_worldMapTerrain = worldMapTerrain;


                //spawner.StoreHeightmapResolution();

                TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewHeightmapResolution = spawner.m_worldCreationSettings.m_gaiaDefaults.m_heightmapResolution;
                TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewRange = Mathf.RoundToInt(spawner.m_worldCreationSettings.m_xTiles * spawner.m_worldCreationSettings.m_tileSize);
                TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewTerrainHeight = spawner.m_worldCreationSettings.m_tileHeight;

                //Check if we do have an existing terrain already, if yes, we would want to re-use it for the world map -> terrain export
                if (GaiaUtils.HasTerrains())
                {
                    spawner.m_useExistingTerrainForWorldMapExport = true;
                }
                else
                {
                    //No terrains yet - set up the terrain tiles in the session according to the current world creation settings
                    //so that the stamp previews will come out right.
                    TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainTilesX = settings.m_tilesX;
                    TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainTilesZ = settings.m_tilesZ;

                }

                //spawner.FitToTerrain();
                spawner.UpdateMinMaxHeight();
            }
            else
            {
                spawner.m_settings.m_isWorldmapSpawner = true;
                //spawner.m_worldMapTerrain = worldMapTerrain;
                //spawner.FitToTerrain();
                spawner.UpdateMinMaxHeight();
            }
            gsm.m_session.m_worldBiomeMaskSettings = spawner.m_settings;
#if UNITY_EDITOR
            spawner.SetRecommendedStampSizes(false);
            spawner.FocusSceneViewOnWorldDesignerPreview();
            //For the pre-defined sizes we can do a spawn right away so the terrain is populated
            if (spawner.m_worldCreationSettings.m_targetSizePreset != GaiaConstants.EnvironmentSizePreset.Custom)
            {
                spawner.SpawnStamps();
            }
            else
            {
                spawner.SetDefaultWorldDesignerPreviewSettings();
            }
            spawner.StoreWorldSize();
#endif
        }

           public static GameObject GetOrCreateWorldMapStamper()
        {
            //Create or find the stamper
            GameObject stamperObj = GameObject.Find(GaiaConstants.worldMapStamper);
            if (stamperObj == null)
            {
                stamperObj = new GameObject(GaiaConstants.worldMapStamper);
                GameObject worldMapObj = GaiaUtils.GetOrCreateWorldDesigner();
                stamperObj.transform.parent = worldMapObj.transform;
                Stamper stamper = stamperObj.AddComponent<Stamper>();
                stamper.m_settings = ScriptableObject.CreateInstance<StamperSettings>();
                stamper.m_settings.m_operation = GaiaConstants.FeatureOperation.RaiseHeight;
                stamper.m_settings.m_isWorldmapStamper = true;
                stamper.m_recordUndo = false;
                //stamper.m_seaLevel = m_settings.m_currentDefaults.m_seaLevel;
                stamper.FitToTerrain();
            }
            else
            {
                Stamper stamper = stamperObj.GetComponent<Stamper>();
                stamper.m_settings.m_isWorldmapStamper = true;
                stamper.m_recordUndo = false;
                //stamper.m_seaLevel = m_settings.m_currentDefaults.m_seaLevel;
                stamper.FitToTerrain();
            }
            Terrain worldMapTerrain = TerrainHelper.GetWorldMapTerrain();
            if (worldMapTerrain != null)
            {
                stamperObj.transform.position = worldMapTerrain.transform.position + worldMapTerrain.terrainData.size / 2f;
            }
            return stamperObj;
        }

    }
}
