using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace MapTool
{
    [CreateAssetMenu(fileName = "NewGridData", menuName = "MapTool/Grid Data")]
    public class GridDataSO : MonoBehaviour
    {
        public int width = 10;
        public int height = 10;

        [TabGroup("Map", "Map settings")]
        [TableMatrix(HorizontalTitle = "TableMatrixTest", DrawElementMethod = "DrawCell", ResizableColumns = false, RowHeight = 16, SquareCells = true)]
        public List<CellData> cellDatas = new();


        public CellData GetCell(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                Debug.LogError("GetCellData: Index out of range");
                return null;
            }
            int index = y * width + x;
            if (index < 0 || index >= cellDatas.Count)
            {
                Debug.LogError("GetCellData: Index out of range");
                return null;
            }
            return cellDatas[index];
        }
    }
}