﻿
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace Gaia
{
    [System.Serializable]
    public class WorldCreationSettings : ScriptableObject
    {
        #region public input variables

        /// <summary>
        /// The preset the user chose for world creation. Stored mainly to transfer this setting between UIs easily.
        /// </summary>
        public GaiaConstants.EnvironmentSizePreset m_targetSizePreset = GaiaConstants.EnvironmentSizePreset.Large;
        /// <summary>
        /// The preset the user chose for world quality. Stored mainly to transfer this setting between UIs easily.
        /// </summary>
        public GaiaConstants.EnvironmentTarget m_qualityPreset = GaiaConstants.EnvironmentTarget.Desktop;
        /// <summary>
        /// The number of terrain tiles on the X-axis
        /// </summary>
        public int m_xTiles = 1;
        /// <summary>
        /// The number of terrain tiles on the Z-axis
        /// </summary>
        public int m_zTiles = 1;
        /// <summary>
        /// The size of a single terrain tile in unity units.
        /// </summary>
        public int m_tileSize = 1024;
        /// <summary>
        /// The height of a single terrain tile in unity units.
        /// </summary>
        public float m_tileHeight = 1000;
        /// <summary>
        /// The date time string used naming the terrains etc. during terrain creation. Needs to be stored in the session to allow recreation of the terrains using the same names.
        /// </summary>
        public string m_dateTimeString;
        /// <summary>
        /// The sea level in the created world. This will have no impact on the created terrain tiles initially, but will be picked up by other Gaia Tools (stamper, spawner) when they are being added to the scene.
        /// </summary>
        public int m_seaLevel = 50;
        /// <summary>
        /// Should the terrain tiles be created in their own scene each? If yes, this allows dynamic loading of terrains during editing and runtime, recommended for large worlds.
        /// </summary>
        public bool m_createInScene;
        /// <summary>
        /// Should the individual terrain scenes be unloaded immediately after creation to conserve memory? Recommended for large worlds.
        /// </summary>
        public bool m_autoUnloadScenes;
        /// <summary>
        /// Should a loading screen be added on world creation?
        /// </summary>
        public bool m_addLoadingScreen;
        /// <summary>
        /// Should a floating point imprecision fix be applied to the terrains during terrain creation? 
        /// </summary>
        public bool m_applyFloatingPointFix;
        /// <summary>
        /// Should the terrains be re-centered around the world origin if the world size setup is updated? 
        /// </summary>
        public bool m_recenterOnUpdate;
        /// <summary>
        /// Offset for the world center point. (In case the world is not supposed to be created around 0,0,0)
        /// </summary>
        /// <returns></returns>
        public Vector2 m_centerOffset = Vector2.zero;
        /// <summary>
        /// Are the terrain tiles to be part of a world map?
        /// </summary>
        public bool m_isWorldMap;

        /// <summary>
        /// if the spawners from the spawner preset list should be spawned on each tile automatically during tile creation
        /// </summary>
        public bool m_autoSpawnBiome = true;
        /// <summary>
        /// The default settings for the terrain tiles being created (terrain resolution settings, etc.). If left empty, Gaia will take the current defaults from the Gaia settings.
        /// </summary>
        public GaiaDefaults m_gaiaDefaults;
        /// <summary>
        /// The prototypes in these spawners will be added to all terrain tiles automatically. Just leave it null if you don't want to add any prototypes to the terrain.
        /// </summary>
        public List<BiomeSpawnerListEntry> m_spawnerPresetList;

        /// <summary>
        /// The world map stamp settings - if the creation was started from the world designer, this contains the information about generated stamps, etc.
        /// </summary>
        public WorldMapStampSettings m_worldMapStampSettings;

        /// <summary>
        /// A bool array that holds information about whether the terrain at that x / z tile coordinate should be created / kept when creating or updating terrain tiles.
        /// </summary>
        public bool[,] m_terrainCreationToggles;

        /// <summary>
        /// Stores the x-z offset of the old terrains within the total grid of new terrains. If e.g. the old terrain setup is a 3 by 3 grid and we are updating to 5 by 5, this offset would allow to offset the old smaller grid within the larger target grid.
        /// </summary>
        public int m_oldTerrainOffsetX = 0;
        public int m_oldTerrainOffsetZ = 0;
        

        #endregion


        #region Helper methods

        public string CheckSettings(bool isWorldMap = false)
        {
            StringBuilder defStr = new StringBuilder();

            if (!Mathf.IsPowerOfTwo(m_gaiaDefaults.m_heightmapResolution - 1))
            {
                defStr.AppendFormat("Height map size must be power of 2 + 1 number! {0} was changed to {1}.\n", m_gaiaDefaults.m_heightmapResolution, Mathf.ClosestPowerOfTwo(m_gaiaDefaults.m_heightmapResolution) + 1);
                m_gaiaDefaults.m_heightmapResolution = Mathf.ClosestPowerOfTwo(m_gaiaDefaults.m_heightmapResolution) + 1;
            }

            if (!Mathf.IsPowerOfTwo(m_gaiaDefaults.m_controlTextureResolution))
            {
                defStr.AppendFormat("Control texture resolution must be power of 2! {0} was changed to {1}.\n", m_gaiaDefaults.m_controlTextureResolution, Mathf.ClosestPowerOfTwo(m_gaiaDefaults.m_controlTextureResolution));
                m_gaiaDefaults.m_controlTextureResolution = Mathf.ClosestPowerOfTwo(m_gaiaDefaults.m_controlTextureResolution);
            }

            if (m_gaiaDefaults.m_controlTextureResolution > 4096)
            {
                defStr.AppendFormat("Control texture resolution must be <= 2048! {0} was changed to {1}.\n", m_gaiaDefaults.m_controlTextureResolution, 4096);
                m_gaiaDefaults.m_controlTextureResolution = 4096;
            }

            if (!Mathf.IsPowerOfTwo(m_gaiaDefaults.m_baseMapSize))
            {
                defStr.AppendFormat("Basemap size must be power of 2! {0} was changed to {1}.\n", m_gaiaDefaults.m_baseMapSize, Mathf.ClosestPowerOfTwo(m_gaiaDefaults.m_baseMapSize));
                m_gaiaDefaults.m_baseMapSize = Mathf.ClosestPowerOfTwo(m_gaiaDefaults.m_baseMapSize);
            }

            if (m_gaiaDefaults.m_baseMapSize > 4096)
            {
                defStr.AppendFormat("Basemap size must be <= 2048! {0} was changed to {1}.\n", m_gaiaDefaults.m_baseMapSize, 4096);
                m_gaiaDefaults.m_baseMapSize = 4096;
            }

            if (!Mathf.IsPowerOfTwo(m_gaiaDefaults.m_detailResolution))
            {
                defStr.AppendFormat("Detail map size must be power of 2! {0} was changed to {1}.\n", m_gaiaDefaults.m_detailResolution, Mathf.ClosestPowerOfTwo(m_gaiaDefaults.m_detailResolution));
                m_gaiaDefaults.m_detailResolution = Mathf.ClosestPowerOfTwo(m_gaiaDefaults.m_detailResolution);
            }

            if (m_gaiaDefaults.m_detailResolutionPerPatch < 8)
            {
                defStr.AppendFormat("Detail resolution per patch must be >= 8! {0} was changed to {1}.\n", m_gaiaDefaults.m_detailResolutionPerPatch, 8);
                m_gaiaDefaults.m_detailResolutionPerPatch = 8;
            }

            return defStr.ToString();
        }

        #endregion


    }
}