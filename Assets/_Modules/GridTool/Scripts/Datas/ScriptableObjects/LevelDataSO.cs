#if UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GridTool
{
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "MapTool/Level Data")]
    public class LevelDataSO : ScriptableObject
    {
        [BoxGroup("Level")]
        [VerticalGroup("Level/Left"), LabelWidth(100)]
        public string levelName = "";

        [BoxGroup("Level")]
        [VerticalGroup("Level/Right"), LabelWidth(100)]
        public SceneAsset levelScene;

        [BoxGroup("Grid Data"), LabelWidth(100)]
        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        [SerializeField]
        public GridDataSO gridData;

        [HorizontalGroup("Controls", LabelWidth = 70)]
        [Button(ButtonSizes.Large, ButtonStyle.CompactBox, Icon = SdfIconType.TrashFill)]
        public void DeleteLevel()
        {
            bool confirmDelete = EditorUtility.DisplayDialog("Delete Level Data?", $"Are you sure you want to delete level data for {levelName}?", "Yes", "No");

            if (!confirmDelete) return;

            string assetPath = GridToolPaths.Levels.LEVELS_DATA_FOLDER + "/" + levelName;

            bool success = AssetDatabase.DeleteAsset(assetPath);

            if (success)
            {
                EditorUtility.DisplayDialog("Success", $"Deleted level data at {assetPath}", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Failed", "Failed to delete asset.", "OK");
            }
        }

        [HorizontalGroup("Controls")]
        [Button(ButtonSizes.Large, ButtonStyle.CompactBox, Icon = SdfIconType.Controller)]
        public void SetupGridController()
        {
            if (!ValidateInputs()) return;

            bool success = SceneUtils.ExecuteInScene(
                levelScene,
                SetupGridControllerInScene,
                SceneUtils.SceneExecutionMode.CloseOriginalAfter,
                saveAfterAction: true,
                saveOnClose: true);

            if (success)
            {
                EditorUtility.DisplayDialog("Success", $"Grid controller setup completed for {levelName}", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Failed", "Failed to setup grid controller.", "OK");
            }

        }

        [HorizontalGroup("Controls")]
        [Button(ButtonSizes.Large, ButtonStyle.CompactBox, Icon = SdfIconType.Eye)]
        public void SetupGridVisualize()
        {
            if (!ValidateInputs()) return;

            bool success = SceneUtils.ExecuteInScene(
                levelScene,
                SetupGridVisualizationInScene,
                SceneUtils.SceneExecutionMode.CloseOriginalAfter,
                saveAfterAction: true,
                saveOnClose: true);

            if (success)
            {
                EditorUtility.DisplayDialog("Success", $"Grid visualizer setup completed for {levelName}", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Failed", "Failed to setup grid visualizer.", "OK");
            }
        }

        [HorizontalGroup("Controls")]
        [Button(ButtonSizes.Large, ButtonStyle.CompactBox, Icon = SdfIconType.EyeSlash)]
        public void DisableGridVisualize()
        {
            if (!ValidateInputs()) return;

            bool success = SceneUtils.ExecuteInScene(
                levelScene,
                DisableGridVisualizationInScene,
                SceneUtils.SceneExecutionMode.CloseOriginalAfter,
                saveAfterAction: true,
                saveOnClose: true);

            if (success)
            {
                EditorUtility.DisplayDialog("Success", $"Grid visualizer setup completed for {levelName}", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Failed", "Failed to setup grid visualizer.", "OK");
            }
        }


        private bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(levelName))
            {
                EditorUtility.DisplayDialog("Error", "Level name cannot be empty!", "OK");
                return false;
            }

            if (levelScene == null)
            {
                EditorUtility.DisplayDialog("Error", "Level scene asset cannot be null!", "OK");
                return false;
            }

            if (gridData == null)
            {
                EditorUtility.DisplayDialog("Error", "Grid data cannot be null!", "OK");
                return false;
            }

            return true;
        }

        private void SetupGridControllerInScene(Scene scene)
        {

            Debug.Log($"Setting up grid controller in scene '{scene.name}'...");

            GridInitializer gridInitializer = GameObjectUtils.FindOrCreateComponent<GridInitializer>("Grid Controller");
            gridInitializer.GridDataSO = gridData;

            if (gridInitializer.TryGetComponent(out GridController gridController))
            {
                return;
            }

            gridInitializer.AddComponent<GridController>();
        }

        private void SetupGridVisualizationInScene(Scene scene)
        {
            Debug.Log($"Setting up grid visualization in scene '{scene.name}'...");

            GridInitializer gridInitializer = GameObjectUtils.FindOrCreateComponent<GridInitializer>("Grid Controller");
            gridInitializer.GridDataSO = gridData;

            if (gridInitializer.TryGetComponent(out GridVisualizer gridVisualizer))
            {
                return;
            }

            gridInitializer.AddComponent<GridVisualizer>();
        }

        private void DisableGridVisualizationInScene(Scene scene)
        {
            Debug.Log($"Disabling grid visualization in scene '{scene.name}'...");

            GridInitializer gridInitializer = GameObjectUtils.FindOrCreateComponent<GridInitializer>("Grid Controller");

            if (gridInitializer.TryGetComponent(out GridVisualizer gridVisualizer))
            {
                Undo.DestroyObjectImmediate(gridInitializer.GetComponent<GridVisualizer>());
            }
        }

    }
}
#endif