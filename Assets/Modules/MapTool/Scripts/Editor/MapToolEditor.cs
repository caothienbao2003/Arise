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
        #region Cell types settings
        [TabGroup("Cell types settings")]
        [SerializeField]
        [FolderPath]
        private string dataPath = "Assets/Modules/MapTool/Data/CellTypes";
        #endregion

        #region Level settings
        [TabGroup("Level settings")]
        [AssetsOnly]
        [Required("Template scene is required")]
        [SerializeField]
        [InfoBox("Scene used as a template when creating new levels. It should contain essential components like Grid, Camera setup, Lighting, etc.", InfoMessageType.None)]
        private SceneAsset templateScene;

        [TabGroup("Level settings")]
        [FolderPath]
        [SerializeField]
        [InfoBox("Path where new level scenes will be created.", InfoMessageType.None)]
        private string levelPath = "Assets/Scenes/Levels";
        #endregion


        private CreateNewCellTypeData createNewCellTypeData;
        private CreateNewLevelData createNewLevelData;

        [MenuItem("Tools/Map Tool")]
        private static void OpenWindow()
        {
            var window = GetWindow<MapToolEditor>();
            window.Show();
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

            createNewCellTypeData = new CreateNewCellTypeData(dataPath);
            createNewLevelData = new CreateNewLevelData(templateScene, levelPath);

            tree.Add("Cell Types/ Create New Type", createNewCellTypeData);
            tree.AddAllAssetsAtPath("Cell Types/ All Cell Types", dataPath, typeof(CellTypeSO));

            tree.Add("Level editor/ Create New Level", createNewLevelData);

            tree.Add("Settings", this);
            return tree;
        }
    }

    public class CreateNewCellTypeData
    {
        public CreateNewCellTypeData(string dataPath)
        {
            cellTypeSO = ScriptableObject.CreateInstance<CellTypeSO>();
            this.dataPath = dataPath;
        }

        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public CellTypeSO cellTypeSO;

        private string dataPath = "";

        [BoxGroup("Actions")]
        [Button("Add New Cell Type", ButtonSizes.Medium), GUIColor(0.4f, 1f, 0.4f), HorizontalGroup("Actions/Buttons")]
        private void CreateNewData()
        {
            Debug.Log(dataPath);
            Debug.Log(cellTypeSO.DisplayName);
            AssetDatabase.CreateAsset(cellTypeSO, $"{dataPath}/{cellTypeSO.DisplayName}.asset");
            AssetDatabase.SaveAssets();

            cellTypeSO = ScriptableObject.CreateInstance<CellTypeSO>();
        }
    }

    public class CreateNewLevelData
    {
        [SerializeField]
        [ValidateInput("ValidateLevelName", "Level name is required and must contain valid characters!")]
        private string levelName;

        private SceneAsset sceneTemplate;
        private string levelPath;
        public CreateNewLevelData(SceneAsset sceneTemplate, string levelPath)
        {
            this.sceneTemplate = sceneTemplate;
            this.levelPath = levelPath;
        }

#if UNITY_EDITOR
        [HorizontalGroup("Actions")]
        [Button("Create level scene", ButtonSizes.Large), GUIColor(0.4f, 1f, 0.4f)]
        private void CreateLevel()
        {
            if (!ValidateInputs())
                return;

            string newScenePath = $"{levelPath}/{levelName}.unity";

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

            if(!Directory.Exists(levelPath))
            {
                Directory.CreateDirectory(levelPath);
                AssetDatabase.Refresh();
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
            if (sceneTemplate == null)
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

            if (string.IsNullOrWhiteSpace(levelPath) || !levelPath.StartsWith("Assets/"))
            {
                EditorUtility.DisplayDialog("Error", "Level path must be valid and start with 'Assets/'!", "OK");
                return false;
            }

            return true;
        }
#endif
    }
}