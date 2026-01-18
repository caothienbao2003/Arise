#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using _Modules.Grid_Editor_Tool.Scripts.Utils;
using CTB;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GridTool
{
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "MapTool/Level Data")]
    public class LevelEditorSO : ScriptableObjectWithActions
    {
        #region Data
        
        [VerticalGroup("Tabs/LevelSetup/Details")]
        public SceneAsset levelScene;

        [TabGroup("Tabs", "LevelSetup")] [TitleGroup("Tabs/LevelSetup/Terrains")]
        public DefaultGridTerrainsSO DefaultGridTerrainsSO;

        [TitleGroup("Tabs/LevelSetup/Terrains")]
        public List<TerrainTypeSO> TerrainTypes = new List<TerrainTypeSO>();

        [TabGroup("Tabs", "Grid Setup")] [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public GridDataSO GridData;

        #endregion

        #region Terrain Actions

        public void ResetTerrainTypeToDefault()
        {
            if (DefaultGridTerrainsSO == null)
            {
                ShowError("Default terrains not assigned!");
                return;
            }

            TerrainTypes = new List<TerrainTypeSO>(DefaultGridTerrainsSO.DefaultTerrainTypes);
            EditorUtility.SetDirty(this);
        }

        #endregion

        #region Level Deletion

        [HorizontalGroup("Actions/Buttons")]
        [Button("Delete", ButtonSizes.Large)]
        [GUIColor(1f, 0.4f, 0.4f)]
        [ShowIf(nameof(IsSavedAsset))]
        [PropertyOrder(999)]
        protected override void DeleteAsset()
        {
            if (!EditorUtility.DisplayDialog(
                    "Delete Level Data?",
                    $"Are you sure you want to delete '{name}'?\n\nThis will delete the entire folder.",
                    "Yes", "No"))
            {
                return;
            }

            string levelPath = AssetDatabaseUtils.GetAssetPath(this);
            string folderPath = AssetDatabaseUtils.GetParentFolderPath(levelPath);

            if (AssetDatabaseUtils.DeleteAsset(folderPath))
            {
                ShowSuccess("Level deleted successfully");
            }
            else
            {
                ShowError("Failed to delete level folder");
            }
        }

        #endregion

        #region Setup Grid Baker

        [HorizontalGroup("Tabs/Grid Setup/Action")]
        [Button(ButtonSizes.Large, ButtonStyle.CompactBox, Icon = SdfIconType.Eye)]
        public void SetUpGridBaker()
        {
            ExecuteInScene(scene =>
            {
                var baker = GameObjectUtils.FindOrCreateComponent<GridBaker>("Grid Baker");
                baker.OutputGridDataSO = GridData;
                baker.TerrainTypeSos = TerrainTypes;
                baker.SetUpLayers();
            }, "Grid Baker Setup");
        }

        [HorizontalGroup("Tabs/Grid Setup/Action")]
        [Button(ButtonSizes.Large, ButtonStyle.CompactBox, Icon = SdfIconType.Trash2Fill)]
        public void DisableGridBaker()
        {
            ExecuteInScene(scene =>
            {
                var baker = UnityEngine.Object.FindAnyObjectByType<GridBaker>();
                if (baker != null)
                {
                    UnityEngine.Object.DestroyImmediate(baker.gameObject);
                }
            }, "Grid Baker Removal");
        }

        #endregion

        #region Grid Visualizer

        [HorizontalGroup("Tabs/Grid Setup/Action")]
        [Button(ButtonSizes.Large, ButtonStyle.CompactBox, Icon = SdfIconType.Eye)]
        public void SetupGridVisualize()
        {
            ExecuteInScene(scene =>
            {
                var visualizer = GameObjectUtils.FindOrCreateComponent<GridVisualizer>("Grid Visualizer");
                visualizer.GridDataSO = GridData;
            }, "Grid Visualizer Setup");
        }

        [HorizontalGroup("Tabs/Grid Setup/Action")]
        [Button(ButtonSizes.Large, ButtonStyle.CompactBox, Icon = SdfIconType.EyeSlash)]
        public void DisableGridVisualize()
        {
            ExecuteInScene(scene =>
            {
                var visualizer = FindAnyObjectByType<GridVisualizer>();
                if (visualizer != null)
                {
                    UnityEngine.Object.DestroyImmediate(visualizer.gameObject);
                }
            }, "Grid Visualizer Removal");
        }

        #endregion

        #region BakeGrid

        [HorizontalGroup("Tabs/Grid Setup/Action")]
        [Button(ButtonSizes.Large, ButtonStyle.CompactBox, Icon = SdfIconType.Grid3x3GapFill)]
        public void BakeGrid()
        {
            ExecuteInScene(scene =>
            {
                var baker = GameObjectUtils.FindOrCreateComponent<GridBaker>("Grid Baker");
                baker.BakeGrid();
            }, "Bake Grid");
        }

        #endregion


        #region Helper Methods

        private void ExecuteInScene(Action<Scene> action, string operationName)
        {
            if (!ValidateForSceneOperation())
            {
                return;
            }

            bool success = SceneUtils.ExecuteInScene(
                levelScene,
                action,
                SceneUtils.SceneExecutionMode.CloseOriginalAfter,
                saveAfterAction: true,
                saveOnClose: true
            );

            if (success)
            {
                ShowSuccess($"{operationName} completed for '{name}'");
            }
            else
            {
                ShowError($"{operationName} failed");
            }
        }

        private bool ValidateForSceneOperation()
        {
            if (string.IsNullOrEmpty(name))
            {
                ShowError("Level name cannot be empty!");
                return false;
            }

            if (levelScene == null)
            {
                ShowError("Level scene cannot be null!");
                return false;
            }

            if (GridData == null)
            {
                ShowError("Grid data cannot be null!");
                return false;
            }

            return true;
        }

        private void ShowSuccess(string message)
        {
            EditorUtility.DisplayDialog("Success", message, "OK");
        }

        private void ShowError(string message)
        {
            EditorUtility.DisplayDialog("Error", message, "OK");
        }

        #endregion

        #region Spawn Setup

        [TabGroup("Tabs", "Spawn Setup")]
        [Title("Spawn Data")]
        [Required]
        [AssetsOnly]
        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public SpawnDataSO SpawnData;

        [TabGroup("Tabs", "Spawn Setup")]
        [HorizontalGroup("Tabs/Spawn Setup/Actions")]
        [Button("Setup Spawn Visualizer", ButtonSizes.Large, Icon = SdfIconType.Eye)]
        [GUIColor(0.4f, 0.8f, 1f)]
        public void SetupSpawnVisualizer()
        {
            ExecuteInScene(scene =>
            {
                var visualizer = GameObjectUtils.FindOrCreateComponent<SpawnVisualizer>("Spawn Visualizer");
                visualizer.SpawnDataSO = SpawnData;
                visualizer.GridDataSO = GridData;
            }, "Spawn Visualizer Setup");
        }

        [HorizontalGroup("Tabs/Spawn Setup/Actions")]
        [Button("Disable Spawn Visualizer", ButtonSizes.Large, Icon = SdfIconType.EyeSlash)]
        [GUIColor(1f, 0.6f, 0.4f)]
        public void DisableSpawnVisualizer()
        {
            ExecuteInScene(scene =>
            {
                var visualizer = UnityEngine.Object.FindAnyObjectByType<SpawnVisualizer>();
                if (visualizer != null)
                {
                    UnityEngine.Object.DestroyImmediate(visualizer.gameObject);
                }
            }, "Spawn Visualizer Removal");
        }

        // [TabGroup("Tabs", "Spawn Setup")]
        // [HorizontalGroup("Tabs/Spawn Setup/Utility")]
        // [Button("Auto-Calculate Summon Zone", ButtonSizes.Large, Icon = SdfIconType.Calculator)]
        // [GUIColor(0.3f, 0.9f, 0.3f)]
        // private void AutoCalculateSummonZone()
        // {
        //     if (SpawnData == null)
        //     {
        //         ShowError("Spawn data is not assigned!");
        //         return;
        //     }
        //
        //     var kingPos = SpawnData.GetKingPosition();
        //     if (kingPos.HasValue)
        //     {
        //         SpawnData.SummonZone.UpdateKingPosition(kingPos.Value);
        //         EditorUtility.SetDirty(SpawnData);
        //         ShowSuccess($"Summon zone calculated around king at {kingPos.Value}");
        //     }
        //     else
        //     {
        //         ShowError("King position not found! Place a king first in SpawnData.");
        //     }
        // }

        [HorizontalGroup("Tabs/Spawn Setup/Utility")]
        [Button("Validate Spawns", ButtonSizes.Large, Icon = SdfIconType.CheckCircleFill)]
        [GUIColor(0.4f, 0.8f, 1f)]
        private void ValidateSpawns()
        {
            if (SpawnData == null)
            {
                ShowError("Spawn data is not assigned!");
                return;
            }

            if (GridData == null)
            {
                ShowError("Grid data reference is not assigned in SpawnData!");
                return;
            }

            Debug.Log("=== Validating Spawn Data ===");

            bool hasErrors = false;

            // Validate fixed spawns
            foreach (var spawn in SpawnData.FixedSpawns)
            {
                if (spawn.SpawnType == null)
                {
                    Debug.LogError("Fixed spawn has null SpawnType");
                    hasErrors = true;
                }
                else
                {
                    var cell = GridData.GetCellAt(spawn.Position);
                    if (cell == null)
                    {
                        Debug.LogError(
                            $"Fixed spawn '{spawn.SpawnType.name}' at {spawn.Position} is outside grid bounds");
                        hasErrors = true;
                    }
                    else if (spawn.SpawnType.MustSpawnOnWalkable && !cell.IsWalkable)
                    {
                        Debug.LogError(
                            $"Fixed spawn '{spawn.SpawnType.name}' at {spawn.Position} is on non-walkable cell");
                        hasErrors = true;
                    }
                }
            }

            // Validate wave spawns
            foreach (var wave in SpawnData.WaveSpawns)
            {
                foreach (var entry in wave.SpawnEntries)
                {
                    if (entry.SpawnType == null)
                    {
                        Debug.LogError($"Wave {wave.WaveIndex} has entry with null SpawnType");
                        hasErrors = true;
                    }
                    else
                    {
                        var cell = GridData.GetCellAt(entry.GridPosition);
                        if (cell == null)
                        {
                            Debug.LogError(
                                $"Wave {wave.WaveIndex} spawn '{entry.SpawnType.name}' at {entry.GridPosition} is outside grid bounds");
                            hasErrors = true;
                        }

                        bool inSummonZone = SpawnData.SummonZone.IsInSummonZone(entry.GridPosition);
                        if (!entry.SpawnType.CanSpawnInSummonZone && inSummonZone)
                        {
                            Debug.LogError(
                                $"Wave {wave.WaveIndex} spawn '{entry.SpawnType.name}' at {entry.GridPosition} is in summon zone but not allowed");
                            hasErrors = true;
                        }
                    }
                }
            }

            if (!hasErrors)
            {
                ShowSuccess("Validation passed! All spawns are valid.");
            }
            else
            {
                ShowError("Validation failed! Check console for details.");
            }
        }

        #endregion
    }
}
#endif