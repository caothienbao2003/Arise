#if UNITY_EDITOR
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;
using CTB;
using SceneSetupTool;

namespace GridTool
{
    public class GridToolEditor : OdinMenuEditorWindow
    {
        [SerializeField]
        [FolderPath]
        private string settingsAssetPath;

        private string SettingAssetFolderPath 
        {
            get => EditorPrefs.GetString("GridTool_SettingsPath", "Assets/");
            set => EditorPrefs.SetString("GridTool_SettingsPath", value);
        }
        
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
            string folderPath = SettingAssetFolderPath;
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

            // Try to find the asset first
            settingsAssetPath = SettingAssetFolderPath;
            settings = AssetDatabaseUtils.GetAssetAtFolder<GridToolSettingsSO>(SettingAssetFolderPath);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SettingAssetFolderPath = settingsAssetPath;
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree();
            
            HandleCreateNewTerrainTypeWindow(tree);
            HandleCreateNewLevelWindow(tree);
            HandleCreateSettingsWindow(tree);

            tree.AddAllAssetsAtPath(
                "Actions",
                settings.ActionAssetPath,
                typeof(SequenceActionsSetupSO),
                includeSubDirectories: true,
                flattenSubDirectories: true
            );
            
            if (settings == null)
            {
                tree.Add("Settings", this);
                return tree;
            }
            
            return tree;
        }

        private void HandleCreateNewLevelWindow(OdinMenuTree tree)
        {
            createNewLevelWindow = new CreateNewLevelWindowBuilder()
                .WithNewGridDataPath(settings.GridDataPath)
                .WithNewLevelDataPath(settings.LevelDataPath)
                .WithNewScenePath(settings.LevelScenePath)
                .WithTemplateScene(settings.TemplateScene)
                .WithTerrainTypesPath(settings.TerrainTypePath)
                .WithIsGenericFolder(settings.IsGenericFolder)
                .Build();

            tree.Add("Level editor", createNewLevelWindow);

            IEnumerable<OdinMenuItem> levels = tree.AddAllAssetsAtPath(
                "Level editor",
                settings.LevelDataPath,
                typeof(LevelEditorSO),
                includeSubDirectories: true,
                flattenSubDirectories: true
            );

            foreach (var item in levels)
                if (item.Value is LevelEditorSO data)
                    item.Name = data.levelName;

        }

        private void HandleCreateNewTerrainTypeWindow(OdinMenuTree tree)
        {
            createNewTerrainTypeWindow = new CreateNewTerrainTypeWindowBuilder()
                .WithNewTerrainTypePath(settings.TerrainTypePath)
                .Build();

            tree.Add("Terrain Types", createNewTerrainTypeWindow);
            IEnumerable<OdinMenuItem> terrainTypes = tree.AddAllAssetsAtPath(
                "Terrain Types",
                settings.TerrainTypePath,
                typeof(TerrainTypeSO),
                includeSubDirectories: true,
                flattenSubDirectories: true
            );

            foreach (var item in terrainTypes)
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