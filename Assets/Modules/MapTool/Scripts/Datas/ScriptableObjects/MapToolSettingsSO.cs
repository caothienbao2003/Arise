using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace MapTool
{
    [CreateAssetMenu(fileName = "MapToolSettings", menuName = "MapTool/MapToolSettings")]
    public class MapToolSettingsSO : ScriptableObject
    {
        #region Cell types settings
        [TabGroup("Cell types settings")]
        [SerializeField]
        [FolderPath]
        public string cellTypePath = "Assets/Modules/MapTool/Data/CellTypes";
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
        #endregion
    }
}