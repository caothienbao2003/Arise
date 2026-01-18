using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GridTool
{
    [CreateAssetMenu(fileName = "MapToolSettings", menuName = "MapTool/MapToolSettings")]
    public class GridToolSettingsSO : ScriptableObject
    {
        //private const string defaultFolder = "Assets/Modules/MapTool";
        //private const string assetPath = defaultFolder + "/MapToolSettings.asset";

#if UNITY_EDITOR

        #region Terrain types settings

        [TabGroup("Terrain types settings")]
        [FolderPath]
        public string TerrainTypePath;
        
        [TabGroup("Terrain types settings")]
        [FolderPath]
        public string DefaultTerrainTypePath;
        
        [TabGroup("Terrain types settings")]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public DefaultGridTerrainsSO TerrainTypeDefaults;
        #endregion

        #region SpawnTypes settings
        [TabGroup("Spawn types settings")]
        [FolderPath]
        public string SpawnTypePath;
        #endregion

        #region Pathfinding settings
        [TabGroup("Pathfinding settings")]
        [FolderPath]
        public string PathfindingProfilePath;
        #endregion
        
        #region Level settings

        [TabGroup("Level settings")]
        [FolderPath]
        [InfoBox("Path where new level scenes will be created.", InfoMessageType.None)]
        public string LevelScenePath;

        [TabGroup("Level settings")]
        [FolderPath]
        [InfoBox("Path where level data ScriptableObjects are stored.", InfoMessageType.None)]
        public string LevelDataPath;

        [TabGroup("Level settings")]
        [FolderPath]
        [InfoBox("Path where grid data ScriptableObjects are stored.", InfoMessageType.None)]
        public string GridDataPath;

        [TabGroup("Level settings")] [InfoBox("Template scene for each level", InfoMessageType.None)]
        public SceneAsset TemplateScene;

        [TabGroup("Level settings")]
        [InfoBox("If check then a parent folder is created with the name of the scene", InfoMessageType.None)]
        public bool IsGenericFolder = true;
        

        #endregion

        #region Actions

        [TabGroup("Actions")]
        [FolderPath]
        public string ActionAssetPath;

        #endregion

#endif
    }
}