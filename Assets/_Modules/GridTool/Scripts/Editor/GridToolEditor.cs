#if UNITY_EDITOR
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;
using CTB_Utils;

namespace GridTool
{
    public class GridToolEditor : OdinMenuEditorWindow
    {
        private CreateNewTerrainTypeWindow createNewTerrainTypeWindow;
        private CreateNewLevelWindow createNewLevelWindow;

        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        [SerializeField, Required]
        [ShowIf("@settings != null")]
        private GridToolSettingsSO settings;

        [ShowIf("@settings == null")]
        [Button(ButtonSizes.Large)]
        private void CreateSettingsAsset()
        {
            string folderPath = GridToolPaths.Settings.SETTINGS_ASSET_PATH;
            // settings = ScriptableObject.CreateInstance<GridToolSettingsSO>();

            GridToolSettingsSO settingsSO = AssetDatabaseUtils.CreateScriptableAsset<GridToolSettingsSO>("Settings", folderPath);

            Debug.Log($"Created new GridToolSettingsSO at {folderPath}");
        }

        [MenuItem("Tools/Grid Tool")]
        private static void OpenWindow()
        {
            var window = GetWindow<GridToolEditor>();
            window.Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            string folderPath = GridToolPaths.Settings.SETTINGS_ASSET_PATH;

            Debug.Log(folderPath);
            // Try to find the asset first
            settings = AssetDatabase.LoadAssetAtPath<GridToolSettingsSO>(folderPath);
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
            OdinMenuTree tree = new OdinMenuTree();

            if (settings == null)
            {
                tree.Add("Settings", this);
                return tree;
            }

            HandleCreateNewTerrainTypeWindow(tree);
            HandleCreateNewLevelWindow(tree);
            HandleCreateSettingsWindow(tree);
            
            
            return tree;
        }
        
        private void HandleCreateNewLevelWindow(OdinMenuTree tree)
        {
            createNewLevelWindow = new CreateNewLevelWindow();
            
            tree.Add("Level editor", createNewLevelWindow);
            
            IEnumerable<OdinMenuItem> levels = tree.AddAllAssetsAtPath(
                "Level editor",
                GridToolPaths.Levels.LEVELS_DATA_FOLDER,
                typeof(LevelDataSO),
                includeSubDirectories: true,
                flattenSubDirectories: true
            );
            
            foreach (var item in levels)
                if (item.Value is LevelDataSO data)
                    item.Name = data.levelName;

        }

        private void HandleCreateNewTerrainTypeWindow(OdinMenuTree tree)
        {
            createNewTerrainTypeWindow = new CreateNewTerrainTypeWindow(settings);

            tree.Add("Terrain Types", createNewTerrainTypeWindow);
            IEnumerable<OdinMenuItem> terrainTypes = tree.AddAllAssetsAtPath(
                "Terrain Types",
                GridToolPaths.TerrainTypes.TERRAIN_TYPE_FOLDER,
                typeof(TerrainTypeSO),
                includeSubDirectories: true,
                flattenSubDirectories: true
            );
            
            foreach(var item in terrainTypes)
                if (item.Value is TerrainTypeSO data)
                    item.Name = data.DisplayName;
        }
        
        private void HandleCreateSettingsWindow(OdinMenuTree tree)
        {
            tree.Add("Settings", this);
        }
    }
}
#endif