using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;
#endif

namespace MapTool
{
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

            tree.Add("Cell Types/ Create New Type", createNewCellTypeData);
            tree.AddAllAssetsAtPath("Cell Types/ All Cell Types", settings.cellTypePath, typeof(CellTypeSO));

            tree.Add("Level editor/ Create New Level", createNewLevelData);

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

#if UNITY_EDITOR
        [HorizontalGroup("Actions")]
        [Button("Create level scene", ButtonSizes.Large), GUIColor(0.4f, 1f, 0.4f)]
        private void CreateLevel()
        {
            if (!ValidateInputs())
                return;

            string newScenePath = $"{settings.levelPath}/{levelName}.unity";

            // Step 1: Handle existing scene
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

            // Step 2: Create new scene from template
            if (!Directory.Exists(settings.levelPath))
            {
                Directory.CreateDirectory(settings.levelPath);
                AssetDatabase.Refresh();
            }

            string templateScenePath = AssetDatabase.GetAssetPath(settings.templateScene);

            //Step 3: Copy template scene to new location
            bool success = AssetDatabase.CopyAsset(templateScenePath, newScenePath);
            if (!success)
            {
                EditorUtility.DisplayDialog("Error", "Failed to create the new level scene.", "OK");
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Step 5: (Optional) Open or highlight new scene
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(newScenePath));

            if (EditorUtility.DisplayDialog(
                "Level created",
                $"Level '{levelName}' created successfully at:\n{newScenePath}",
                "Open Scene",
                "Close"))
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(newScenePath);
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
#endif
    }
}