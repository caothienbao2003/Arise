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
        [SerializeField] private Color cellColor = Color.green;
        [SerializeField] private Color labelColor = Color.white;
        [SerializeField] private int labelFontSize = 12;

        private GridDataSO gridData => GridManager.Instance.GridData;
        private Grid<CellData> runtimeGrid => GridManager.Instance.RuntimeGrid;
        private Pathfinding<CellData> pathfinding => GridManager.Instance.Pathfinding;

        void OnDrawGizmos()
        {
            if (runtimeGrid == null || gridData == null)
            {
                Debug.LogWarning("GridVisualizer is missing GridData or RuntimeGrid.");
                return;
            }
            
            Gizmos.color = cellColor;
            
            foreach (Vector2Int pos in runtimeGrid.GetAllCellXYPositions())
            {
                Vector3 center = runtimeGrid.GetCellCenterWorldPos(pos);
                Gizmos.DrawWireCube(center, new Vector3(gridData.CellSize, gridData.CellSize, 0.01f));
                
#if UNITY_EDITOR
                DrawCellLabel(pos, center);
#endif
            }
        }
        
#if UNITY_EDITOR
        private void DrawCellLabel(Vector2Int pos, Vector3 center)
        {
            float halfSize = gridData.CellSize * 0.5f;
            Vector3 labelPos = new Vector3(
                center.x - halfSize + 0.1f,
                center.y + halfSize - 0.1f,
                center.z
            );
            
            GUIStyle style = new GUIStyle();
            style.normal.textColor = labelColor;
            style.fontSize = labelFontSize;
            
            Handles.Label(labelPos, $"{pos.x},{pos.y}", style);
        }
#endif
    }
}