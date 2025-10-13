using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;
#endif

namespace MapTool
{
#if UNITY_EDITOR
    public class MapToolEditor : OdinMenuEditorWindow
    {
        private CreateNewCellTypeWindow createNewCellTypeData;
        private CreateNewLevelWindow createNewLevelData;

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

            if (createNewCellTypeData != null && createNewCellTypeData.cellTypeSO != null)
            {
                DestroyImmediate(createNewCellTypeData.cellTypeSO);
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

            createNewCellTypeData = new CreateNewCellTypeWindow(settings);
            createNewLevelData = new CreateNewLevelWindow(settings);

            tree.Add("Cell Types", createNewCellTypeData);
            tree.AddAllAssetsAtPath("Cell Types", settings.cellTypePath, typeof(CellTypeSO));

            tree.Add("Level editor", createNewLevelData);

            var items = tree.AddAllAssetsAtPath(
                "Level editor",
                settings.levelDataPath,
                typeof(LevelDataSO),
                includeSubDirectories: true,
                flattenSubDirectories: true
            );

            foreach (var item in items)
                if (item.Value is LevelDataSO data)
                    item.Name = data.levelName;

            tree.Add("Settings", this);
            return tree;
        }
    }

    public class CreateNewCellTypeWindow
    {
        public CreateNewCellTypeWindow(MapToolSettingsSO settings)
        {
            cellTypeSO = ScriptableObject.CreateInstance<CellTypeSO>();
            this.settings = settings;
        }

        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public CellTypeSO cellTypeSO;

        private MapToolSettingsSO settings;

        [BoxGroup("Actions")]
        [Button("Add New Cell Type", ButtonSizes.Medium), GUIColor(0.4f, 1f, 0.4f), HorizontalGroup("Actions/Buttons")]
        private void CreateNewData()
        {
            Debug.Log(cellTypeSO.DisplayName);
            AssetDatabase.CreateAsset(cellTypeSO, $"{settings.cellTypePath}/{cellTypeSO.DisplayName}.asset");
            AssetDatabase.SaveAssets();

            cellTypeSO = ScriptableObject.CreateInstance<CellTypeSO>();
        }
    }

    public class CreateNewLevelWindow
    {
        [SerializeField]
        [ValidateInput("ValidateLevelName", "Level name is required and must contain valid characters!")]
        private string levelName;

        private MapToolSettingsSO settings;
        public CreateNewLevelWindow(MapToolSettingsSO settings)
        {
            this.settings = settings;
        }

        [HorizontalGroup("Actions")]
        [Button("Create level scene", ButtonSizes.Large), GUIColor(0.4f, 1f, 0.4f)]
        private void CreateLevel()
        {
            if (!ValidateInputs())
                return;


            // Step 1: Handle existing scene
            Directory.CreateDirectory($"{settings.levelPath}/{levelName}");

            string newScenePath = $"{settings.levelPath}/{levelName}/{levelName}.unity";
            
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
            if (!Directory.Exists(settings.levelPath))
            {
                Directory.CreateDirectory(settings.levelPath);
            }

            if (!Directory.Exists(settings.levelDataPath))
            {
                Directory.CreateDirectory(settings.levelDataPath);
            }

            AssetDatabase.Refresh();


            //Step 3: Copy template scene to new location
            string templateScenePath = AssetDatabase.GetAssetPath(settings.templateScene);

            bool success = AssetDatabase.CopyAsset(templateScenePath, newScenePath);
            if (!success)
            {
                EditorUtility.DisplayDialog("Error", "Failed to create the new level scene.", "OK");
                return;
            }

            //Step 4: Create LevelDataSO
            Directory.CreateDirectory($"{settings.gridDataPath}/{levelName}");
            Directory.CreateDirectory($"{settings.levelDataPath}/{levelName}");

            string newGridDataPath = $"{settings.gridDataPath}/{levelName}/{levelName}_GridData.asset";
            string newLevelDataPath = $"{settings.levelDataPath}/{levelName}/{levelName}_LevelData.asset";

            var gridData = ScriptableObject.CreateInstance<GridDataSO>();
            AssetDatabase.CreateAsset(gridData, newGridDataPath);

            var levelData = ScriptableObject.CreateInstance<LevelDataSO>();
            levelData.levelName = levelName;
            levelData.levelScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(newScenePath);
            levelData.gridData = gridData;

            AssetDatabase.CreateAsset(levelData, newLevelDataPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Step 5: Link LevelDataSO to scene
            var scene = EditorSceneManager.OpenScene(newScenePath);
            var map = GameObject.FindFirstObjectByType<GridMap>();

            if(map == null)
            {
                EditorUtility.DisplayDialog("Error", "No GridMap found in the template scene. Please ensure the template scene contains a GridMap component.", "OK");
                return;
            }
            else
            {

            }
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

            if (string.IsNullOrWhiteSpace(settings.levelPath) || !settings.levelPath.StartsWith("Assets/"))
            {
                EditorUtility.DisplayDialog("Error", "Level path must be valid and start with 'Assets/'!", "OK");
                return false;
            }

            return true;
        }
    }
#endif
}