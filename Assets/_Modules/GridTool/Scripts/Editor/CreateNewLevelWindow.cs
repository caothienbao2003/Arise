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
using CTB_Utils;

namespace GridTool
{
    public class CreateNewLevelWindow
    {
        [SerializeField]
        [ValidateInput("ValidateLevelName", "Level name is required and must contain valid characters!")]
        private string levelName;

        [AssetsOnly]
        [SerializeField]
        public SceneAsset templateScene;

        [SerializeField] private bool VisualizeGrid = true;

        private List<GridLayer> gridLayers;
        private GridDataSO gridData;
        private SceneAsset levelScene;

        public CreateNewLevelWindow() { }

        #region Editor
        [HorizontalGroup("Actions")]
        [Button("Create level scene", ButtonSizes.Large), GUIColor(0.4f, 1f, 0.4f)]
        private void CreateLevel()
        {
            if (!ValidateInputs())
                return;

            string dataParentFolder = $"{GridToolPaths.Levels.LEVELS_DATA_FOLDER}/{levelName}";
            string newGridDataPath = $"{dataParentFolder}/{levelName}_GridData.asset";
            string newDataPath = $"{dataParentFolder}/{levelName}_LevelData.asset";
            string newScenePath = $"{dataParentFolder}/{levelName}.unity";
            
            Debug.Log($"Creating new level scene at: {newScenePath}");
            Debug.Log($"Grid data asset will be saved at: {newGridDataPath}");
            Debug.Log($"Level data asset will be saved at: {newDataPath}");

            AssetDatabaseUtils.EnsureFolderExists(dataParentFolder);
            
            if (!CreateOrCopyNewSceneFromTemplate(newScenePath))
            {
                return;
            }

            if (!CreateGridDataAsset(newGridDataPath))
            {
                return;
            }

            if (!CreateLevelDataAsset(newDataPath, out LevelDataSO levelData))
            {
                return;
            }

            Scene newScene = EditorSceneManager.OpenScene(newScenePath);

            SetupScene(newScene);
            EditorSceneManager.SaveOpenScenes();
            
            EditorUtility.DisplayDialog("Success", $"Level '{levelName}' created successfully!", "OK");
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

            if (string.IsNullOrWhiteSpace(GridToolPaths.Levels.LEVELS_DATA_FOLDER) || !GridToolPaths.Levels.LEVELS_DATA_FOLDER.StartsWith("Assets/"))
            {
                EditorUtility.DisplayDialog("Error", "Level path must be valid and start with 'Assets/'!", "OK");
                return false;
            }
            return true;
        }
        
        private bool CreateOrCopyNewSceneFromTemplate(string newScenePath)
        {
            levelScene = null;

            if (File.Exists(newScenePath))
            {
                if (!EditorUtility.DisplayDialog(
                    "Scene already exist",
                    $"A scene name '{levelName}' already exist at: \n{newScenePath}",
                    "Override",
                    "Cancel"))
                {
                    return false;
                }

                AssetDatabase.DeleteAsset(newScenePath);
            }

            if (templateScene == null)
            {
                if(!CreateNewScene(newScenePath)) return false;
            }
            else if (!DuplicateScene(newScenePath))
            {
                return false;
            }

            levelScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(newScenePath);

            return true;
        }
        
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
        private bool CreateLevelDataAsset(string newLevelDataPath, out LevelDataSO levelData)
        {
            levelData = null;

            string levelDataAssetName = $"{levelName}_LevelData";
            levelData = ScriptableObject.CreateInstance<LevelDataSO>();
            levelData.name = levelDataAssetName;
            levelData.levelScene = levelScene;
            levelData.levelName = levelName;
            levelData.gridData = gridData;
            AssetDatabaseUtils.CreateAsset(levelData, newLevelDataPath);

            if (levelData == null)
            {
                Debug.LogError($"[CreateNewLevelWindow] Failed to create LevelData asset at path: {newLevelDataPath}");
                return false;
            }

            return true;
        }
        #endregion

        #region Setup Tilemap and GridBaker
        private void SetupScene(Scene newScene)
        {
            SetUpGridTilemap();
            SetUpGridBaker();

            EditorSceneManager.MarkSceneDirty(newScene);
            AssetDatabase.SaveAssets();
        }

        private void SetUpGridTilemap()
        {
            TerrainTypeSO[] terrainTypes = AssetDatabaseUtils.GetAllAssetsInFolder<TerrainTypeSO>(GridToolPaths.TerrainTypes.TERRAIN_TYPE_FOLDER);
            SetupTilemapLayerGameObjects(terrainTypes);
        }

        private void SetUpGridBaker()
        {
            GridBaker gridBaker = GameObjectUtils.FindOrCreateComponent<GridBaker>("MapBaker");

            gridBaker.SetGridLayers(gridLayers);
            gridBaker.SetOutputMapData(gridData);

            EditorUtility.SetDirty(gridBaker);
            EditorUtility.SetDirty(gridBaker.gameObject);
        }

        private void SetupTilemapLayerGameObjects(TerrainTypeSO[] terrainTypes)
        {
            gridLayers = new List<GridLayer>();
            GameObject gridParent = GameObjectUtils.FindOrCreateComponent<Grid>("Grid").gameObject;

            foreach (TerrainTypeSO terrain in terrainTypes)
            {
                Tilemap layerTileMap = ProcessTilemapLayer(terrain, gridParent);

                // Determine name based on rendering status
                // string layerGOName = terrain.IsRender ? $"Core_{terrain.DisplayName}" : $"NoRender_{terrain.DisplayName}";

                GridLayer newLayer = new GridLayer
                {
                    LayerName = terrain.DisplayName,
                    TerrainType = terrain,
                    TileMap = layerTileMap,
                    Priority = terrain.Priority
                };
                gridLayers.Add(newLayer);
            }
        }
        private Tilemap ProcessTilemapLayer(TerrainTypeSO terrain, GameObject gridParent)
        {
            string layerGOName = $"Grid_{terrain.DisplayName}";

            GameObject tileMapLayerGO = GameObjectUtils.FindOrCreateGameObject(layerGOName);
            tileMapLayerGO.transform.SetParent(gridParent.transform);

            Tilemap layerTileMap = tileMapLayerGO.AddComponent<Tilemap>();
            TilemapRenderer tilemapRenderer = tileMapLayerGO.AddComponent<TilemapRenderer>();

            // Set sorting order based on index to ensure consistent rendering
            tilemapRenderer.sortingOrder = terrain.Priority;

            return layerTileMap;
        }
        #endregion

        #region Setup Visualization
        private void SetupGridInitializer()
        {
            if (gridData == null) return;
            GridInitializer gridInitializer = GameObjectUtils.FindOrCreateComponent<GridInitializer>("Grid Controller");

            gridInitializer.AddComponent<GridController>();
            
            if(VisualizeGrid) SetupGridVisualization(gridInitializer);
        }
        private void SetupGridVisualization(GridInitializer gridInitializer)
        {
            gridInitializer.GridDataSO = gridData;
            gridInitializer.AddComponent<GridVisualizer>();
        }
        
        #endregion

        private List<TerrainTypeSO> LoadAllTerrainTypes()
        {
            List<TerrainTypeSO> terrainTypes = new List<TerrainTypeSO>();
            GUID[] terrainGUIDs = AssetDatabase.FindAssetGUIDs(
                $"t:{nameof(TerrainTypeSO)}",
                new[] { GridToolPaths.TerrainTypes.TERRAIN_TYPE_FOLDER }
            );

            Debug.Log($"Found {terrainGUIDs.Length} TerrainType assets in {GridToolPaths.TerrainTypes.TERRAIN_TYPE_FOLDER}");

            foreach (GUID terrain in terrainGUIDs)
            {
                TerrainTypeSO terrainType = AssetDatabase.LoadAssetByGUID<TerrainTypeSO>(terrain);
                if (terrainType != null)
                {
                    Debug.Log("Loaded terrain type: " + terrainType.DisplayName);
                    terrainTypes.Add(terrainType);
                }
            }
            return terrainTypes;
        }
    }
}
#endif