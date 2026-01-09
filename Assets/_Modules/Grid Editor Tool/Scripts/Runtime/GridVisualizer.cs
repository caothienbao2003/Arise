#if UNITY_EDITOR
using UnityEngine;
using GridUtilities;
using Sirenix.OdinInspector;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GridTool
{
    public class GridVisualizer: MonoBehaviour
    {
        [Title("Grid Data")]
        [SerializeField, AssetsOnly, Required]
        public GridDataSO GridDataSO;
        
        [Title("Visualization Settings")]
        [SerializeField] private Color cellColor = Color.green;
        [SerializeField] private Color nonWalkableColor = Color.red;
        [SerializeField] private Color labelColor = Color.white;
        [SerializeField] private int labelFontSize = 12;
        [SerializeField] private bool showLabels = true;
        [SerializeField] private bool showNonWalkable = true;

        private void OnDrawGizmos()
        {
            if (GridDataSO == null || GridDataSO.CellDatas == null)
            {
                return;
            }
            
            foreach (var cellData in GridDataSO.CellDatas)
            {
                Gizmos.color = (showNonWalkable && !cellData.IsWalkable) 
                    ? nonWalkableColor 
                    : cellColor;
                
                Vector3 center = GetWorldPosition(cellData.GridPosition);
                Gizmos.DrawWireCube(
                    center, 
                    new Vector3(GridDataSO.CellSize, GridDataSO.CellSize, 0.01f)
                );
                
                if (showLabels)
                {
                    DrawCellLabel(cellData.GridPosition, center);
                }
            }
        }
        
        private Vector3 GetWorldPosition(Vector2Int gridPos)
        {
            return new Vector3(
                gridPos.x * GridDataSO.CellSize + GridDataSO.CellSize * 0.5f,
                gridPos.y * GridDataSO.CellSize + GridDataSO.CellSize * 0.5f,
                0f
            );
        }
        
        private void DrawCellLabel(Vector2Int pos, Vector3 center)
        {
            float halfSize = GridDataSO.CellSize * 0.5f;
            Vector3 labelPos = new Vector3(
                center.x - halfSize + 0.1f,
                center.y + halfSize - 0.1f,
                center.z
            );
            
            GUIStyle style = new GUIStyle
            {
                normal = { textColor = labelColor },
                fontSize = labelFontSize
            };
            
            Handles.Label(labelPos, $"{pos.x},{pos.y}", style);
        }
    }
}
#endif