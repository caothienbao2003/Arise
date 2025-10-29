#if UNITY_EDITOR
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Linq;

using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;

namespace MapTool
{
    public class MapToolEditor : OdinMenuEditorWindow
    {
        private CreateNewTerrainTypeWindow createNewTerrainTypeWindow;
        private CreateNewLevelWindow createNewLevelWindow;

        [SerializeField]
        [ShowIf("@settings == null")]
        [FolderPath]
        private string SettingsAssetPath = "Assets/Modules/MapTool";

        [SerializeField]
        [ShowIf("@settings == null")]
        [FolderPath]
        private string SettingsName = "MapToolSettings";

        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        [SerializeField, Required]
        [ShowIf("@settings != null")]
        private MapToolSettingsSO settings;


        [ShowIf("@settings == null")]
        [Button(ButtonSizes.Large)]
        private void CreateSettingsAsset()
        {
            string folderPath = $"{SettingsAssetPath}/{SettingsName}.asset";

            // Create folder path if it doesn’t exist
            string folder = Path.GetDirectoryName(folderPath);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // Create new ScriptableObject instance
            settings = ScriptableObject.CreateInstance<MapToolSettingsSO>();

            // Save as an asset in the project
            AssetDatabase.CreateAsset(settings, folderPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Ping it for visual feedback
            EditorGUIUtility.PingObject(settings);

            Debug.Log($"Created new MapToolSettingsSO at {folderPath}");
        }

        [MenuItem("Tools/Map Tool")]
        private static void OpenWindow()
        {
            var window = GetWindow<MapToolEditor>();
            window.Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            string folderPath = $"{SettingsAssetPath}/{SettingsName}.asset";

            Debug.Log(folderPath);
            // Try to find the asset first
            settings = AssetDatabase.LoadAssetAtPath<MapToolSettingsSO>(folderPath);

            // If not found, automatically create it
            //if (settings == null)
            //{
            //    Debug.Log("MapToolSettingsSO not found, creating new one...");
            //    CreateSettingsAsset();
            //}
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (createNewTerrainTypeWindow != null && createNewTerrainTypeWindow.cellTypeSO != null)
            {
                DestroyImmediate(createNewTerrainTypeWindow.cellTypeSO);
            }
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();

            if (settings == null)
            {
                tree.Add("Settings", this);
                return tree;
            }

            createNewTerrainTypeWindow = new CreateNewTerrainTypeWindow(settings);
            createNewLevelWindow = new CreateNewLevelWindow(settings);

            tree.Add("Terrain Types", createNewTerrainTypeWindow);
            var terrainTypes = tree.AddAllAssetsAtPath(
                "Terrain Types",
                settings.terrainTypePath,
                typeof(TerrainTypeSO),
                includeSubDirectories: true,
                flattenSubDirectories: true
                );

            foreach(var item in terrainTypes)
                if (item.Value is TerrainTypeSO data)
                    item.Name = data.DisplayName;

            tree.Add("Level editor", createNewLevelWindow);

            var levels = tree.AddAllAssetsAtPath(
                "Level editor",
                settings.levelDataPath,
                typeof(LevelDataSO),
                includeSubDirectories: true,
                flattenSubDirectories: true
            );

            foreach (var item in levels)
                if (item.Value is LevelDataSO data)
                    item.Name = data.levelName;

            tree.Add("Settings", this);
            return tree;
        }
    }
}
#endif