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

        public bool Validate(IGridService gridService, bool isInSummonZone)
        {
            if (SpawnType == null)
            {
                Debug.LogError("SpawnEntry validation failed: SpawnType is null");
                return false;
            }

            if (SpawnType.Prefab == null)
            {
                Debug.LogError($"SpawnEntry validation failed: Prefab is null for {SpawnType.name}");
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