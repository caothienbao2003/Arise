using Sirenix.OdinInspector;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MapTool
{
    [CreateAssetMenu(fileName = "MapToolSettings", menuName = "MapTool/MapToolSettings")]
    public class MapToolSettingsSO : ScriptableObject
    {
        private const string defaultFolder = "Assets/Modules/MapTool";
        private const string assetPath = defaultFolder + "/MapToolSettings.asset";

#if UNITY_EDITOR
        #region Cell types settings
        [TabGroup("Cell types settings")]
        [SerializeField]
        [FolderPath]
        public string terrainTypePath = "Assets/Modules/MapTool/Data/CellTypes";
        #endregion

        #region Level settings
        [TabGroup("Level settings")]
        [AssetsOnly]
        [Required("Template scene is required")]
        [SerializeField]
        [InfoBox("Scene used as a template when creating new levels. It should contain essential components like Grid, Camera setup, Lighting, etc.", InfoMessageType.None)]
        public SceneAsset templateScene;

        [TabGroup("Level settings")]
        [FolderPath]
        [SerializeField]
        [InfoBox("Path where new level scenes will be created.", InfoMessageType.None)]
        public string levelPath = "Assets/Scenes/Levels";

        [TabGroup("Level settings")]
        [FolderPath]
        [SerializeField]
        [InfoBox("Path where level data ScriptableObjects are stored.", InfoMessageType.None)]
        public string levelDataPath = "Assets/Modules/MapTool/Data/LevelDatas";

        [TabGroup("Level settings")]
        [FolderPath]
        [SerializeField]
        [InfoBox("Path where grid data ScriptableObjects are stored.", InfoMessageType.None)]
        public string gridDataPath = "Assets/Modules/MapTool/Data/GridDatas";
        #endregion
#endif

#if UNITY_EDITOR
        private static MapToolSettingsSO _instance;
        public static MapToolSettingsSO Instance
        {
            get
            {
                if (_instance == null)
                    _instance = LoadOrCreate();

                return _instance;
            }
        }

        private static MapToolSettingsSO LoadOrCreate()
        {
            // Try to find existing asset anywhere in the project
            string guid = AssetDatabase.FindAssets("t:MapToolSettingsSO").FirstOrDefault();
            if (!string.IsNullOrEmpty(guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var found = AssetDatabase.LoadAssetAtPath<MapToolSettingsSO>(path);
                if (found != null) return found;
            }

            // If not found, create a new one in a default location
            if (!Directory.Exists(defaultFolder))
                Directory.CreateDirectory(defaultFolder);

            var newSettings = CreateInstance<MapToolSettingsSO>();
            AssetDatabase.CreateAsset(newSettings, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[MapTool] Created new settings asset at: {assetPath}");
            return newSettings;
        }
#endif
    }
}