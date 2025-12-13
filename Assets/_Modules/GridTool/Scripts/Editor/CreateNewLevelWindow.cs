#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace GridTool
{
    public class CreateNewLevelWindow
    {
        [SerializeField]
        [ValidateInput("ValidateLevelName", "Level name is required and must contain valid characters!")]
        private string levelName;
        
        [AssetsOnly]
        [Required("Template scene is required")]
        [SerializeField]
        public SceneAsset templateScene;
        
        [SerializeField] private bool VisualizeGrid = true;

        // private GridToolSettingsSO settings;
        public CreateNewLevelWindow()
        {
        }

        [HorizontalGroup("Actions")]
        [Button("Create level scene", ButtonSizes.Large), GUIColor(0.4f, 1f, 0.4f)]
        private void CreateLevel()
        {
            if (!ValidateInputs())
                return;

            string newScenePath = $"{GridToolPaths.Levels.LEVELS_SCENES_FOLDER}/{levelName}.unity";

            // 1. Handle scene file existence and copy template
            if (!HandleSceneFileCreation(newScenePath))
                return;

            // 2. Create and link ScriptableObject assets
            if (!CreateAssetFiles(newScenePath, out GridDataSO gridData, out LevelDataSO levelData))
                return;

            // 3. Open scene and set up runtime components
            OpenSceneAndSetupBaker(newScenePath, gridData);

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Success", $"Level '{levelName}' created successfully!", "OK");
        }
        
        // --- STEP 1: SCENE HANDLING ---

        private bool HandleSceneFileCreation(string newScenePath)
        {
            Directory.CreateDirectory($"{GridToolPaths.Levels.LEVELS_SCENES_FOLDER}");

            // Check if file exists and handle overwrite
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

            // Copy template scene
            string templateScenePath = AssetDatabase.GetAssetPath(templateScene);

            bool success = AssetDatabase.CopyAsset(templateScenePath, newScenePath);
            if (!success)
            {
                EditorUtility.DisplayDialog("Error", "Failed to copy the new level scene from template.", "OK");
                return false;
            }

            AssetDatabase.Refresh();
            return true;
        }

        // --- STEP 2: ASSET CREATION ---

        private bool CreateAssetFiles(string newScenePath, out GridDataSO gridData, out LevelDataSO levelData)
        {
            gridData = null;
            levelData = null;

            Directory.CreateDirectory($"{GridToolPaths.GridData.GRID_DATA_FOLDER}");
            Directory.CreateDirectory($"{GridToolPaths.Levels.LEVELS_DATA_FOLDER}");

            string newGridDataPath = $"{GridToolPaths.GridData.GRID_DATA_FOLDER}/{levelName}_GridData.asset";
            string newLevelDataPath = $"{GridToolPaths.Levels.LEVELS_DATA_FOLDER}/{levelName}_LevelData.asset";

            try
            {
                // Create GridDataSO
                gridData = ScriptableObject.CreateInstance<GridDataSO>();
                AssetDatabase.CreateAsset(gridData, newGridDataPath);

                // Create LevelDataSO
                levelData = ScriptableObject.CreateInstance<LevelDataSO>();
                levelData.levelName = levelName;
                levelData.levelScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(newScenePath);
                levelData.gridData = gridData;
                AssetDatabase.CreateAsset(levelData, newLevelDataPath);
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Asset Error", $"Failed to create GridData or LevelData assets: {e.Message}", "OK");
                return false;
            }

            AssetDatabase.SaveAssets();
            return true;
        }

        // --- STEP 3: SCENE SETUP ---

        private void OpenSceneAndSetupBaker(string newScenePath, GridDataSO gridData)
        {
            var scene = EditorSceneManager.OpenScene(newScenePath);
            
            SetUpMapBaker(scene, gridData);

            EditorSceneManager.SaveScene(scene);
        }

        // --- VALIDATION HELPERS (Kept as is for brevity, but could be split further) ---

        private bool ValidateLevelName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            char[] invalidChars = Path.GetInvalidFileNameChars();
            return name.IndexOfAny(invalidChars) == -1;
        }

        private bool ValidateInputs()
        {
            // ... (Your existing validation logic remains here) ...
            if (templateScene == null)
            {
                EditorUtility.DisplayDialog("Error", "Template scene is required!", "OK");
                return false;
            }

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


        // --- REFACTORED SetUpMapBaker ---

        private void SetUpMapBaker(Scene scene, GridDataSO mapData)
        {
            GridBaker mapBaker = FindOrCreateGridBaker();
            
            List<TerrainTypeSO> terrainTypes = LoadAllTerrainTypes();

            GameObject tileMapParentGO = FindOrCreateTilemapParent();

            List<GridLayer> layers = CreateTilemapLayers(terrainTypes, tileMapParentGO);

            mapBaker.SetUp(layers, mapData);

            // Finalizing scene changes
            EditorUtility.SetDirty(mapBaker);
            EditorUtility.SetDirty(mapBaker.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("MapBaker setup complete.");
        }

        private GridBaker FindOrCreateGridBaker()
        {
            GridBaker mapBaker = GameObject.FindAnyObjectByType<GridBaker>();

            if (mapBaker == null)
            {
                GameObject mapBakerGO = new GameObject("MapBaker");
                mapBaker = mapBakerGO.AddComponent<GridBaker>();
            }

            return mapBaker;
        }

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

        private GameObject FindOrCreateTilemapParent()
        {
            // Grid is the default parent object for all Tilemaps in Unity's 2D setup
            GameObject tileMapParentGO = GameObject.FindAnyObjectByType<Grid>()?.gameObject;
            if (tileMapParentGO == null)
            {
                tileMapParentGO = new GameObject("Grid");
                tileMapParentGO.AddComponent<Grid>();
            }
            return tileMapParentGO;
        }

        private List<GridLayer> CreateTilemapLayers(List<TerrainTypeSO> terrainTypes, GameObject tileMapParent)
        {
            List<GridLayer> layers = new List<GridLayer>();
            int layerIndex = 0;

            foreach (TerrainTypeSO terrain in terrainTypes)
            {
                // Determine name based on rendering status
                string layerGOName = terrain.IsRender ? 
                    $"Core_{terrain.DisplayName}" : 
                    $"NoRender_{terrain.DisplayName}";

                GameObject tileMapLayerGO = new GameObject(layerGOName);
                tileMapLayerGO.transform.SetParent(tileMapParent.transform);
                
                Tilemap layerTileMap = tileMapLayerGO.AddComponent<Tilemap>();
                TilemapRenderer tilemapRenderer = tileMapLayerGO.AddComponent<TilemapRenderer>();
                
                // Set sorting order based on index to ensure consistent rendering
                tilemapRenderer.sortingOrder = layerIndex;

                GridLayer newLayer = new GridLayer
                {
                    LayerName = terrain.DisplayName,
                    TerrainType = terrain,
                    TileMap = layerTileMap,
                    Priority = terrain.Priority
                };
                layers.Add(newLayer);
                layerIndex++;
            }
            return layers;
        }
    }
}
#endif