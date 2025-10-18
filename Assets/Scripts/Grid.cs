using System;
using TMPro;
using UnityEngine;
using FreelancerNecromancer;

namespace FreelancerNecromancer
{
    public class Grid<TGridObject>
    {
        private int width;
        private int height;
        private float cellSize;
        private Vector3 originPosition;

        private TGridObject[,] gridArray;
        private TextMeshProUGUI[,] debugTextArray;

        public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;

        public delegate TGridObject CreateGridObjectDelegate(Grid<TGridObject> grid, int x, int y);

        public class OnGridValueChangedEventArgs : EventArgs
        {
            public int x;
            public int y;
        }

        public Grid(int width, int height, float cellSize, Vector3 originPosition, CreateGridObjectDelegate createGridObject)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;

            gridArray = new TGridObject[width, height];

            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    gridArray[x, y] = createGridObject(this, x, y);
                }
            }

            debugTextArray = new TextMeshProUGUI[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    string displayText = gridArray[x, y].ToString();
                    debugTextArray[x, y] = UIUtils.CreateWorldText(
                        text: displayText,
                        worldPosition: GetWorldPos(x, y) + new Vector3(cellSize, cellSize) * .5f,
                        size: new Vector2(cellSize, cellSize),
                        fontSize: 0.5f,
                        color: Color.white,
                        parent: null,
                        canvas: null,
                        sortingOrder: 0
                        );

                    Debug.DrawLine(GetWorldPos(x, y), GetWorldPos(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPos(x, y), GetWorldPos(x + 1, y), Color.white, 100f);
                }
            }

            Debug.DrawLine(GetWorldPos(0, height), GetWorldPos(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPos(width, 0), GetWorldPos(width, height), Color.white, 100f);

            OnGridValueChanged += (object sender, OnGridValueChangedEventArgs eventArgs) =>
            {
                debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y].ToString();
            };
        }

        public void TriggerGridObjectChanged(Vector3 worldPosition)
        {
            int x, y = 0;
            GetXYPos(worldPosition, out x, out y);
            OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { x = x, y = y });
        }

        public void TriggerGridObjectChanged(int x, int y)
        {
            OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { x = x, y = y });
        }

        public void GetXYPos(Vector3 worldPosition, out int x, out int y)
        {
            x = Mathf.FloorToInt((worldPosition.x - originPosition.x) / cellSize);
            y = Mathf.FloorToInt((worldPosition.y - originPosition.y) / cellSize);
        }

        public Vector3 GetWorldPos(int x, int y)
        {
            return new Vector3(x, y) * cellSize + originPosition;
        }

        public void SetCellGridObject(int x, int y, TGridObject value)
        {
            if (x >= width || y >= height || x < 0 || y < 0) return;

            gridArray[x, y] = value;
            debugTextArray[x, y].text = gridArray[x, y].ToString();

            OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { x = x, y = y });
        }

        public void SetCellGridObject(Vector3 worldPosition, TGridObject value)
        {
            int x, y = 0;

            GetXYPos(worldPosition, out x, out y);
            SetCellGridObject(x, y, value);

            OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { x = x, y = y });
        }

        public TGridObject GetCellGridObject(int x, int y)
        {
            if (x >= width || y >= height || x < 0 || y < 0) return default(TGridObject);
            return gridArray[x, y];
        }

        public TGridObject GetCellGridObject(Vector3 worldPosition)
        {
            int x, y = 0;

            GetXYPos(worldPosition, out x, out y);
            return GetCellGridObject(x, y);
        }
    }


}