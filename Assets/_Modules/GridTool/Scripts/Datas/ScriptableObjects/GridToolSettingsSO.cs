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
        //#region Cell types settings
        //[TabGroup("Cell types settings")]
        //[SerializeField]
        //[FolderPath]
        //public string terrainTypePath = "Assets/Modules/MapTool/Data/CellTypes";
        //#endregion

        

        //[TabGroup("Level settings")]
        //[FolderPath]
        //[SerializeField]
        //[InfoBox("Path where new level scenes will be created.", InfoMessageType.None)]
        //public string levelPath = "Assets/Scenes/Levels";

        //[TabGroup("Level settings")]
        //[FolderPath]
        //[SerializeField]
        //[InfoBox("Path where level data ScriptableObjects are stored.", InfoMessageType.None)]
        //public string levelDataPath = "Assets/Modules/MapTool/Data/LevelDatas";

        //[TabGroup("Level settings")]
        //[FolderPath]
        //[SerializeField]
        //[InfoBox("Path where grid data ScriptableObjects are stored.", InfoMessageType.None)]
        //public string gridDataPath = "Assets/Modules/MapTool/Data/GridDatas"
#endif

#if UNITY_EDITOR
        private static GridToolSettingsSO _instance;
        public static GridToolSettingsSO Instance
        {
            get
            {
                if (_instance == null)
                    _instance = LoadOrCreate();

                return _instance;
            }
        }

        private static GridToolSettingsSO LoadOrCreate()
        {
            // Try to find existing asset anywhere in the project
            string guid = AssetDatabase.FindAssets("t:MapToolSettingsSO").FirstOrDefault();
            if (!string.IsNullOrEmpty(guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var found = AssetDatabase.LoadAssetAtPath<GridToolSettingsSO>(path);
                if (found != null) return found;
            }

            // If not found, create a new one in a default location
            if (!Directory.Exists(GridToolPaths.Settings.SETTINGS_ASSET_PATH))
                Directory.CreateDirectory(GridToolPaths.Settings.SETTINGS_ASSET_PATH);

            var newSettings = CreateInstance<GridToolSettingsSO>();
            AssetDatabase.CreateAsset(newSettings, GridToolPaths.Settings.SETTINGS_ASSET_PATH);
            AssetDatabase.SaveAssets();

            Debug.Log($"[MapTool] Created new settings asset at: {GridToolPaths.Settings.SETTINGS_ASSET_PATH}");
            return newSettings;
        }
#endif
    }
}