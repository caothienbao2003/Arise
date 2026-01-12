using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

namespace GridTool
{
    [Serializable]
    [InlineProperty]
    [HideReferenceObjectPicker]
    public class  SummonZoneConfig
    {
        [TitleGroup("Summon Zone Configuration")]
        [BoxGroup("TitleGroup/Auto-Calculate")]
        [HorizontalGroup("TitleGroup/Auto-Calculate/Row")]
        [LabelText("Auto-Calculate Around King")]
        [ToggleLeft]
        public bool AutoCalculateAroundKing = true;

        [HorizontalGroup("TitleGroup/Auto-Calculate/Row")]
        [ShowIf(nameof(AutoCalculateAroundKing))]
        [HideLabel]
        [LabelText("King Position")]
        [ReadOnly]
        public Vector2Int KingPositionReference;

        [BoxGroup("TitleGroup/Auto-Calculate")]
        [ShowIf(nameof(AutoCalculateAroundKing))]
        [Button(ButtonSizes.Medium, Icon = SdfIconType.Calculator)]
        [GUIColor(0.4f, 0.8f, 1f)]
        private void RecalculateZone()
        {
            Positions = CalculatePositionsAroundKing(KingPositionReference);
            Debug.Log($"Summon zone recalculated: {Positions.Count} positions around king at {KingPositionReference}");
        }

        [TitleGroup("Summon Zone Configuration")]
        [BoxGroup("TitleGroup/Positions")]
        [ListDrawerSettings(
            ShowFoldout = false,
            DraggableItems = true,
            ShowPaging = false,
            NumberOfItemsPerPage = 10)]
        [HideIf(nameof(AutoCalculateAroundKing))]
        public List<Vector2Int> Positions = new List<Vector2Int>();

        [BoxGroup("TitleGroup/Positions")]
        [ShowInInspector]
        [ReadOnly]
        [LabelText("Total Summon Positions")]
        public int TotalPositions => Positions?.Count ?? 0;

        public void UpdateKingPosition(Vector2Int kingPosition)
        {
            KingPositionReference = kingPosition;
            
            if (AutoCalculateAroundKing)
            {
                Positions = CalculatePositionsAroundKing(kingPosition);
            }
        }

        public List<Vector2Int> GetValidPositions(IGridService gridService)
        {
            if (Positions == null || Positions.Count == 0)
                return new List<Vector2Int>();

            return Positions
                .Where(pos => 
                {
                    var cell = gridService.GetCellAt(pos);
                    return cell != null && cell.IsWalkable && !cell.IsOccupied;
                })
                .ToList();
        }

        public bool IsInSummonZone(Vector2Int position)
        {
            if (Positions == null)
                return false;

            return Positions.Contains(position);
        }

        public bool Validate(IGridService gridService)
        {
            if (Positions == null || Positions.Count == 0)
            {
                Debug.LogWarning("Summon zone has no positions defined");
                return false;
            }

            bool allValid = true;
            foreach (var pos in Positions)
            {
                var cell = gridService.GetCellAt(pos);
                if (cell == null)
                {
                    Debug.LogError($"Summon zone position {pos} does not exist in grid");
                    allValid = false;
                    continue;
                }

                if (!cell.IsWalkable)
                {
                    Debug.LogWarning($"Summon zone position {pos} is not walkable");
                    allValid = false;
                }
            }

            return allValid;
        }

        private List<Vector2Int> CalculatePositionsAroundKing(Vector2Int kingPos)
        {
            var positions = new List<Vector2Int>
            {
                kingPos + new Vector2Int(-1, 1),  // Top-left
                kingPos + new Vector2Int(0, 1),   // Top
                kingPos + new Vector2Int(1, 1),   // Top-right
                kingPos + new Vector2Int(-1, 0),  // Left
                kingPos + new Vector2Int(1, 0),   // Right
                kingPos + new Vector2Int(-1, -1), // Bottom-left
                kingPos + new Vector2Int(0, -1),  // Bottom
                kingPos + new Vector2Int(1, -1)   // Bottom-right
            };

            return positions;
        }
    }
}