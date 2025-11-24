#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

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

        private GridToolSettingsSO settings;
        public CreateNewLevelWindow(GridToolSettingsSO settings)
        {
            this.settings = settings;
        }

        [HorizontalGroup("Actions")]
        [Button("Create level scene", ButtonSizes.Large), GUIColor(0.4f, 1f, 0.4f)]
        private void CreateLevel()
        {
            if (!ValidateInputs())
                return;


            Directory.CreateDirectory($"{GridToolPaths.Levels.LEVELS_SCENES_FOLDER}");

            string newScenePath = $"{GridToolPaths.Levels.LEVELS_SCENES_FOLDER}/{levelName}.unity";

            // Step 1: Check if file existed, if yes, override or return
            if (File.Exists(newScenePath))
            {
                if (!EditorUtility.DisplayDialog(
                    "Scene already exist",
                    $"A scene name '{levelName} already exist at: \n{newScenePath}'",
                    "Override",
                    "Cancel"))
                {
                    return;
                }

                AssetDatabase.DeleteAsset(newScenePath);
            }

            // Step 2: Create directories if missing
            //if (!Directory.Exists(GridToolPaths.Levels.LEVELS_DATA_FOLDER))
            //{
            //    Directory.CreateDirectory(GridToolPaths.Levels.LEVELS_DATA_FOLDER);
            //}

            //AssetDatabase.Refresh();


            //Step 3: Copy template scene to new location
            string templateScenePath = AssetDatabase.GetAssetPath(settings.templateScene);

            bool success = AssetDatabase.CopyAsset(templateScenePath, newScenePath);
            if (!success)
            {
                EditorUtility.DisplayDialog("Error", "Failed to create the new level scene.", "OK");
                return;
            }

            //Step 4: Create LevelDataSO
            Directory.CreateDirectory($"{GridToolPaths.GridData.GRID_DATA_FOLDER}");
            Directory.CreateDirectory($"{GridToolPaths.Levels.LEVELS_DATA_FOLDER}");

            string newGridDataPath = $"{GridToolPaths.GridData.GRID_DATA_FOLDER}/{levelName}_GridData.asset";
            string newLevelDataPath = $"{GridToolPaths.Levels.LEVELS_DATA_FOLDER}/{levelName}_LevelData.asset";

            var gridData = ScriptableObject.CreateInstance<GridDataSO>();
            AssetDatabase.CreateAsset(gridData, newGridDataPath);

            var levelData = ScriptableObject.CreateInstance<LevelDataSO>();
            levelData.levelName = levelName;
            levelData.levelScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(newScenePath);
            levelData.gridData = gridData;

            AssetDatabase.CreateAsset(levelData, newLevelDataPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Step 5: Open scene, set up MapBaker
            var scene = EditorSceneManager.OpenScene(newScenePath);
            SetUpMapBaker(scene, gridData);

            EditorSceneManager.SaveScene(scene);
            EditorUtility.DisplayDialog("Success", $"Level '{levelName}' created successfully!", "OK");
        }

        private bool ValidateLevelName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            char[] invalidChars = Path.GetInvalidFileNameChars();
            return name.IndexOfAny(invalidChars) == -1;
        }

        private bool ValidateInputs()
        {
            if (settings.templateScene == null)
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

        private void SetUpMapBaker(Scene scene, GridDataSO mapData)
        {
            //Find or create MapEditor game object
            GridBaker mapBaker = GameObject.FindAnyObjectByType<GridBaker>();
            GameObject mapBakerGO = null;

            if (mapBaker == null)
            {
                mapBakerGO = new GameObject("MapBaker");
                mapBaker = mapBakerGO.AddComponent<GridBaker>();
            }
            else
            {
                mapBakerGO = mapBaker.gameObject;
            }

            GUID[] terrainGUIDs = AssetDatabase.FindAssetGUIDs($"t:{nameof(TerrainTypeSO).ToString()}", new[] { GridToolPaths.TerrainTypes.TERRAIN_TYPE_FOLDER });
            List<TerrainTypeSO> terrainTypes = new List<TerrainTypeSO>();

            Debug.Log("Terrain GUIDS: " + terrainGUIDs.Length + " " + GridToolPaths.TerrainTypes.TERRAIN_TYPE_FOLDER);

            foreach (GUID terrain in terrainGUIDs)
            {
                TerrainTypeSO terrainType = AssetDatabase.LoadAssetByGUID<TerrainTypeSO>(terrain);
                if (terrainType != null)
                {
                    Debug.Log("Found terrain type: " + terrainType.DisplayName);
                    terrainTypes.Add(terrainType);
                }
            }

            GameObject tileMapParentGO = GameObject.FindAnyObjectByType<Grid>()?.gameObject;
            if (tileMapParentGO == null)
            {
                tileMapParentGO = new GameObject("Grid");
                tileMapParentGO.AddComponent<Grid>();
            }

            List<GridLayer> layers = new List<GridLayer>();

            foreach (TerrainTypeSO terrain in terrainTypes)
            {
                GameObject tileMapLayerGO = null;

                if (terrain.IsRender)
                {
                    tileMapLayerGO = new GameObject("Core_" + terrain.DisplayName);
                }
                else
                {
                    tileMapLayerGO = new GameObject("NoRender_" + terrain.DisplayName);
                }

                tileMapLayerGO.transform.SetParent(tileMapParentGO.transform);
                Tilemap layerTileMap = tileMapLayerGO.AddComponent<Tilemap>();

                TilemapRenderer tilemapRenderer = tileMapLayerGO.AddComponent<TilemapRenderer>();
                tilemapRenderer.sortingOrder = layers.Count;

                GridLayer newLayer = new GridLayer
                {
                    layerName = terrain.DisplayName,
                    terrainType = terrain,
                    tileMap = layerTileMap
                };
                layers.Add(newLayer);
            }

            mapBaker.SetUp(layers, mapData);

            //Set dirty to save changes
            EditorUtility.SetDirty(mapBaker);
            EditorUtility.SetDirty(mapBaker.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);

            AssetDatabase.SaveAssets();

            Debug.Log("MapBaker setup complete.");
        }
    }
}
#endif