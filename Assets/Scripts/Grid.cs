using TMPro;
using UnityEngine;

namespace FN
{
    public class Grid
    {
        private int width;
        private int height;
        private float cellSize;
        private Vector3 originPosition;

        private Transform gridParent;
        private Canvas canvas;

        private int[,] gridArray;
        private TextMeshProUGUI[,] debugTextArray;

        public Grid(int width, int height, float cellSizes, Vector3 originPosition, Canvas canvas, Transform gridParent = null)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSizes;
            this.originPosition = originPosition;

            this.canvas = canvas;
            this.gridParent = gridParent;

            gridArray = new int[width, height];
            debugTextArray = new TextMeshProUGUI[width, height];

            SetUpGrid();
        }

        private void SetUpGrid()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    string displayText = gridArray[x, y].ToString();
                    debugTextArray[x,y] = UIUtils.CreateWorldText(
                        text: displayText,
                        worldPosition: ConvertXYToWorldPos(x, y) + new Vector3(cellSize, cellSize) * .5f,
                        size: new Vector2(cellSize, cellSize),
                        fontSize: 1,
                        color: Color.white,
                        parent: gridParent,
                        canvas: canvas,
                        sortingOrder: 0
                        );

                    Debug.DrawLine(ConvertXYToWorldPos(x, y), ConvertXYToWorldPos(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(ConvertXYToWorldPos(x, y), ConvertXYToWorldPos(x + 1, y), Color.white, 100f);
                }
            }
        }

        private void ConvertWorldPosToXY(Vector3 worldPosition, out int x, out int y)
        {
            x = Mathf.FloorToInt((worldPosition.x - originPosition.x) / cellSize);
            y = Mathf.FloorToInt((worldPosition.y - originPosition.y) / cellSize);
        }

        private Vector3 ConvertXYToWorldPos(int x, int y)
        {
            return new Vector3(x, y) * cellSize + originPosition;
        }

        public void SetCellValue(int x, int y, int value)
        {
            if (x >= width || y >= height || x < 0 || y < 0) return;

            gridArray[x,y] = value;
            debugTextArray[x,y].text = gridArray[x,y].ToString();
        }

        public void SetCellValue(Vector3 worldPosition, int value)
        {
            int x, y = 0;

            ConvertWorldPosToXY(worldPosition, out x, out y);
            SetCellValue(x, y, value);
        }

        public int GetCellValue(int x, int y)
        {
            if(x >= width || y >= height || x < 0 || y < 0) return -1;
            return gridArray[x,y];
        }

        public int GetCellValue(Vector3 worldPosition)
        {
            int x, y = 0;

            ConvertWorldPosToXY(worldPosition, out x, out y);
            return GetCellValue(x, y);
        }
    }
}