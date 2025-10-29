using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MapTool
{
    [CreateAssetMenu(fileName = "NewGridData", menuName = "MapTool/Grid Data")]
    public class MapDataSO : ScriptableObject
    {
        public List<CellData> cellDatas = new();

        [Title("Grid Visualization")]
        [ShowInInspector, ReadOnly]
        [TableMatrix(DrawElementMethod = "DrawCell", SquareCells = true)]
        private Color[,] GridPreview
        {
            get
            {
                if (cellDatas == null || cellDatas.Count == 0)
                    return new Color[1, 1];

                return GenerateGridPreview();
            }
        }

        private Color[,] GenerateGridPreview()
        {
            int minX = cellDatas.Min(c => c.CellPosition.x);
            int maxX = cellDatas.Max(c => c.CellPosition.x);
            int minY = cellDatas.Min(c => c.CellPosition.y);
            int maxY = cellDatas.Max(c => c.CellPosition.y);

            int width = maxX - minX + 1;
            int height = maxY - minY + 1;

            Color[,] grid = new Color[height, width];

            // Fill with empty color
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    grid[y, x] = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                }
            }

            // Fill with terrain colors
            foreach (var cell in cellDatas)
            {
                int x = cell.CellPosition.x - minX;
                int y = maxY - cell.CellPosition.y;

                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    grid[y, x] = cell.TerrainType != null
                        ? cell.TerrainType.CellColor
                        : Color.gray;
                }
            }

            return grid;
        }


#if UNITY_EDITOR
        private static Color DrawCell(Rect rect, Color value)
        {
            UnityEditor.EditorGUI.DrawRect(rect, value);
            return value;
        }
#endif

        public CellData GetCellAt(Vector2Int position)
        {
            return cellDatas?.Find(c => c.CellPosition == position);
        }
    }
}