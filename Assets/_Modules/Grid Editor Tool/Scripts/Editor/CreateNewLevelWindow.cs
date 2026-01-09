#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Tilemaps;
using CTB;

namespace GridTool
{
    public class CreateNewLevelWindow
    {
        #region Level Info
        [TabGroup("Level Info")]
        [SerializeField]
        [ValidateInput("ValidateLevelName", "Level name is required and must contain valid characters!")]
        private string levelName;

        [TabGroup("Level Info")]
        [AssetsOnly]
        [SerializeField]
        public SceneAsset templateScene;

        [TabGroup("Level Info")]
        [SerializeField] private bool VisualizeGrid = true;

        [TabGroup("Level Info")]
        [SerializeField]
        public bool IsGenericFolder;
        #endregion

        #region Paths
        [TabGroup("Paths")]
        [SerializeField]
        [FolderPath]
        public string NewGridDataFolderPath;

        [TabGroup("Paths")]
        [SerializeField]
        [FolderPath]
        public string NewLevelDataFolderPath;

        [TabGroup("Paths")]
        [SerializeField]
        [FolderPath]
        public string NewSceneFolderPath;

        [TabGroup("Paths")]
        [FolderPath]
        public string TerrainTypesPath;
        #endregion

        private List<GridLayer> gridLayers;
        private GridDataSO gridData;
        private SceneAsset levelScene;

        public CreateNewLevelWindow() { }

        #region Editor
        [HorizontalGroup("Actions")]
        [Button("Create level scene", ButtonSizes.Large), GUIColor(0.4f, 1f, 0.4f)]
        private void CreateNewLevel()
        {
            if (!ValidateInputs())
                return;
            
            
            //
            //
            // string newScenePath;
            // string newGridDataPath;
            // string newLevelDataPath;
            //
            // if (IsGenericFolder)
            // {
            //     string genericFolderPath = $"{NewSceneFolderPath}/{levelName}";
            //     AssetDatabaseUtils.EnsureFolderExists(genericFolderPath);
            //
            //     newScenePath = $"{genericFolderPath}/{levelName}.unity";
            //     newGridDataPath = $"{genericFolderPath}/{levelName}_GridData.asset";
            //     newLevelDataPath = $"{genericFolderPath}/{levelName}_LevelEditor.asset";
            // }
            // else
            // {
            //     AssetDatabaseUtils.EnsureFolderExists(NewGridDataFolderPath);
            //     AssetDatabaseUtils.EnsureFolderExists(NewLevelDataFolderPath);
            //     AssetDatabaseUtils.EnsureFolderExists(NewSceneFolderPath);
            //
            //     newScenePath = $"{NewSceneFolderPath}/{levelName}.unity";
            //     newGridDataPath = $"{NewGridDataFolderPath}/{levelName}_GridData.asset";
            //     newLevelDataPath = $"{NewLevelDataFolderPath}/{levelName}_LevelEditor.asset";
            // }
            //
            // if (!CreateOrCopyNewSceneFromTemplate(newScenePath))
            // {
            //     return;
            // }
            //
            // if (!CreateGridDataAsset(newGridDataPath))
            // {
            //     return;
            // }
            //
            // if (!CreateLevelDataAsset(newLevelDataPath, out LevelEditorSO levelData))
            // {
            //     return;
            // }
            //
            // Scene newScene = EditorSceneManager.OpenScene(newScenePath);
            //
            // SetupScene(newScene);
            // EditorSceneManager.SaveOpenScenes();
            //
            // EditorUtility.DisplayDialog("Success", $"Level '{levelName}' created successfully!", "OK");
        }
        #endregion


        #region Set up scene data
        private bool ValidateLevelName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            char[] invalidChars = Path.GetInvalidFileNameChars();
            return name.IndexOfAny(invalidChars) == -1;
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(levelName))
            {
                EditorUtility.DisplayDialog("Error", "Level name cannot be empty!", "OK");
                return false;
            }

            if (!ValidateLevelName(levelName))
            {
                EditorUtility.DisplayDialog("Error", "Level name contains invalid characters!", "OK");
                return false;
            }

            return true;
        }

        // private bool CreateOrCopyNewSceneFromTemplate(string newScenePath)
        // {
        //     levelScene = null;
        //
        //     if (File.Exists(newScenePath))
        //     {
        //         if (!EditorUtility.DisplayDialog(
        //             "Scene already exist",
        //             $"A scene name '{levelName}' already exist at: \n{newScenePath}",
        //             "Override",
        //             "Cancel"))
        //         {
        //             return false;
        //         }
        //
        //         AssetDatabase.DeleteAsset(newScenePath);
        //     }
        //
        //     if (templateScene == null)
        //     {
        //         if (!CreateNewScene(newScenePath)) return false;
        //     }
        //     else if (!DuplicateScene(newScenePath))
        //     {
        //         return false;
        //     }
        //
        //     levelScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(newScenePath);
        //
        //     return true;
        // }
        //
        
        private bool CreateNewScene(string newScenePath)
        {
            levelScene = null;
        
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            bool success = EditorSceneManager.SaveScene(newScene, newScenePath);
        
            if (!success)
            {
                EditorUtility.DisplayDialog("Error", "Failed to create scene.", "OK");
                return false;
            }
        
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        
            return true;
        }

        private bool DuplicateScene(string newScenePath)
        {
            string templateScenePath = AssetDatabase.GetAssetPath(templateScene);
            bool success = AssetDatabaseUtils.CopyAsset(templateScenePath, newScenePath);

            if (!success)
            {
                EditorUtility.DisplayDialog("Error", "Failed to copy the new level scene from template.", "OK");
                return false;
            }

            return true;
        }

        private bool CreateGridDataAsset(string newGridDataPath)
        {
            gridData = null;

            string gridDataAssetName = $"{levelName}_GridData";
            gridData = AssetDatabaseUtils.CreateScriptableAsset<GridDataSO>(gridDataAssetName, newGridDataPath);

            if (gridData == null)
            {
                Debug.LogError($"[CreateNewLevelWindow] Failed to create GridData asset at path: {newGridDataPath}");
                return false;
            }

            return true;
        }
        private bool CreateLevelDataAsset(string newLevelDataPath, out LevelEditorSO levelEditor)
        {
            levelEditor = null;

            string levelDataAssetName = $"{levelName}_LevelData";
            levelEditor = ScriptableObject.CreateInstance<LevelEditorSO>();
            levelEditor.name = levelDataAssetName;
            levelEditor.levelScene = levelScene;
            levelEditor.levelName = levelName;
            levelEditor.GridData = gridData;
            AssetDatabaseUtils.CreateAsset(levelEditor, newLevelDataPath);

            if (levelEditor == null)
            {
                Debug.LogError($"[CreateNewLevelWindow] Failed to create LevelData asset at path: {newLevelDataPath}");
                return false;
            }

            return true;
        }
        #endregion

        #region Setup Tilemap and GridBaker
        // private void SetupScene(Scene newScene)
        // {
        //     // SetUpGridTilemap();
        //     SetUpGridBaker();
        //
        //     EditorSceneManager.MarkSceneDirty(newScene);
        //     AssetDatabase.SaveAssets();
        // }
        //
        // // private void SetUpGridTilemap()
        // // {
        // //     TerrainTypeSO[] terrainTypes = AssetDatabaseUtils.GetAllAssetsInFolder<TerrainTypeSO>(TerrainTypesPath);
        // //     SetupTilemapLayerGameObjects(terrainTypes);
        // // }
        //
        // private void SetUpGridBaker()
        // {
        //     GridBaker gridBaker = GameObjectUtils.FindOrCreateComponent<GridBaker>("MapBaker");
        //
        //     gridBaker.SetUpLayers();
        //     // gridBaker.SetOutputMapData(gridData);
        //
        //     EditorUtility.SetDirty(gridBaker);
        //     EditorUtility.SetDirty(gridBaker.gameObject);
        // }
        #endregion

        #region Setup Visualization
        // private void SetupGridInitializer()
        // {
        //     if (gridData == null) return;
        //     GridInitializer gridInitializer = GameObjectUtils.FindOrCreateComponent<GridInitializer>("Grid Controller");
        //
        //     if (VisualizeGrid) SetupGridVisualization(gridInitializer);
        // }
        // private void SetupGridVisualization(GridInitializer gridInitializer)
        // {
        //     gridInitializer.GridDataSO = gridData;
        //     gridInitializer.AddComponent<GridVisualizer>();
        // }
        #endregion
    }

    public class CreateNewLevelWindowBuilder
    {
        private CreateNewLevelWindow window = new CreateNewLevelWindow();

        public CreateNewLevelWindowBuilder WithTemplateScene(SceneAsset templateScene)
        {
            window.templateScene = templateScene;
            return this;
        }

        public CreateNewLevelWindowBuilder WithNewGridDataPath(string newGridDataPath)
        {
            window.NewGridDataFolderPath = newGridDataPath;
            return this;
        }

        public CreateNewLevelWindowBuilder WithNewLevelDataPath(string newLevelDataPath)
        {
            window.NewLevelDataFolderPath = newLevelDataPath;
            return this;
        }

        public CreateNewLevelWindowBuilder WithNewScenePath(string newScenePath)
        {
            window.NewSceneFolderPath = newScenePath;
            return this;
        }

        public CreateNewLevelWindowBuilder WithTerrainTypesPath(string terrainTypesPath)
        {
            window.TerrainTypesPath = terrainTypesPath;
            return this;
        }

        public CreateNewLevelWindowBuilder WithIsGenericFolder(bool isGenericFolder)
        {
            window.IsGenericFolder = isGenericFolder;
            return this;
        }

        public CreateNewLevelWindow Build() => window;
    }
}
#endif