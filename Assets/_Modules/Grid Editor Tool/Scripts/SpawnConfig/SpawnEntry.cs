using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace GridTool
{
    [Serializable]
    [InlineProperty]
    [HideReferenceObjectPicker]
    public class SpawnEntry
    {
        [HorizontalGroup("Entry", 0.4f)]
        [HideLabel]
        [Required]
        [AssetsOnly]
        public SpawnTypeSO SpawnType;

        [HorizontalGroup("Entry", 0.3f)]
        [HideLabel]
        [LabelText("Position")]
        public Vector2Int GridPosition;

        // [HorizontalGroup("Entry", 0.3f)]
        // [HideLabel]
        // [LabelText("Rotation")]
        // [Range(0, 360)]
        // public float RotationDegrees = 0f;
        //
        // [FoldoutGroup("Advanced")]
        // [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.Foldout)]
        // public Dictionary<string, string> CustomData = new Dictionary<string, string>();
        //
        // public Quaternion GetRotation()
        // {
        //     return Quaternion.Euler(0, 0, RotationDegrees);
        // }
        //
        // public void SetCustomData(string key, string value)
        // {
        //     if (CustomData == null)
        //         CustomData = new Dictionary<string, string>();
        //     CustomData[key] = value;
        // }
        //
        // public string GetCustomData(string key)
        // {
        //     if (CustomData == null || !CustomData.ContainsKey(key))
        //         return null;
        //     return CustomData[key];
        // }

        public bool Validate(IGridService gridService, bool isInSummonZone)
        {
            if (SpawnType == null)
            {
                Debug.LogError("SpawnEntry validation failed: SpawnType is null");
                return false;
            }

            if (SpawnType.Prefab == null)
            {
                Debug.LogError($"SpawnEntry validation failed: Prefab is null for {SpawnType.DisplayName}");
                return false;
            }

            var cellData = gridService.GetCellAt(GridPosition);
            if (cellData == null)
            {
                Debug.LogError($"SpawnEntry validation failed: Position {GridPosition} does not exist in grid");
                return false;
            }

            bool isWalkable = cellData.IsWalkable;
            bool isOccupied = cellData.IsOccupied;

            return SpawnType.ValidateSpawnRules(GridPosition, isWalkable, isOccupied, isInSummonZone);
        }
    }
}