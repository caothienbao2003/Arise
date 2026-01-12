using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace GridTool
{
    [Serializable]
    [InlineProperty]
    [HideReferenceObjectPicker]
    public class FixedSpawnConfig
    {
        [TitleGroup("$GetTitle")]
        [HorizontalGroup("$GetTitle/Main")]
        [HideLabel]
        [Required]
        [AssetsOnly]
        [OnValueChanged(nameof(OnSpawnTypeChanged))]
        public SpawnTypeSO SpawnType;

        [HorizontalGroup("$GetTitle/Main")]
        [HideLabel]
        [LabelText("Position")]
        public Vector2Int Position;

        [TitleGroup("$GetTitle")]
        [HorizontalGroup("$GetTitle/Settings")]
        [LabelText("Spawn on Start")]
        [ToggleLeft]
        public bool SpawnOnLevelStart = true;
        //
        // [HorizontalGroup("$GetTitle/Settings")]
        // [LabelText("Rotation")]
        // [Range(0, 360)]
        // public float RotationDegrees = 0f;

        private string GetTitle()
        {
            if (SpawnType != null)
            {
                string category = !string.IsNullOrEmpty(SpawnType.DisplayName) 
                    ? SpawnType.DisplayName 
                    : "Spawn";
                return $"{category}: {SpawnType.DisplayName}";
            }
            return "Unassigned Fixed Spawn";
        }

        private void OnSpawnTypeChanged()
        {
            // Can add logic here if needed when spawn type changes
        }

        public SpawnEntry ToSpawnEntry()
        {
            return new SpawnEntry
            {
                SpawnType = SpawnType,
                GridPosition = Position
                // RotationDegrees = RotationDegrees
            };
        }

        public bool Validate(IGridService gridService, bool isInSummonZone)
        {
            if (SpawnType == null)
            {
                Debug.LogError("FixedSpawnConfig validation failed: SpawnType is null");
                return false;
            }

            return ToSpawnEntry().Validate(gridService, isInSummonZone);
        }
    }
}