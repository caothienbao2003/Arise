using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GridTool
{
    public class Grid<TGridObject>
    {
        private float cellSize;
        private Vector3 originPosition;

        private Dictionary<Vector2Int, TGridObject> gridDictionary;

        public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;

        // public delegate TGridObject CreateGridObjectDelegate(Grid<TGridObject> grid, int x, int y);

        public class OnGridValueChangedEventArgs : EventArgs
        {
            public Vector2Int CellXYPosition;
        }

        public Grid(float cellSize, Vector3 originPosition)
        {
            this.cellSize = cellSize;
            this.originPosition = originPosition;
            gridDictionary = new Dictionary<Vector2Int, TGridObject>();

            Debug.Log($"Created grid with cell size {cellSize}");
            
            OnGridValueChanged += HandleGridValueChanged;
        }
        private void HandleGridValueChanged(object sender, OnGridValueChangedEventArgs eventArgs)
        {
            Debug.Log($"{sender.GetType().Name} changed at {eventArgs.CellXYPosition}");
        }

        private bool HasCell(Vector2Int cellXYPosition)
        {
            return gridDictionary.ContainsKey(cellXYPosition);
        }

        public void TriggerGridObjectChanged(Vector3 worldPosition)
        {
            Vector2Int cellXYPos = GetCellXYPos(worldPosition);
            OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { CellXYPosition = cellXYPos});
        }

        public void TriggerGridObjectChanged(Vector2Int cellXYPosition)
        {
            if (!HasCell(cellXYPosition)) return;
            OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { CellXYPosition = cellXYPosition});
        }

        public void TriggerGridObjectChanged(int x, int y)
        {
            TriggerGridObjectChanged(new Vector2Int(x, y));
        }


        public Vector2Int GetCellXYPos(Vector3 worldPosition)
        {
            int x = Mathf.FloorToInt((worldPosition.x - originPosition.x) / cellSize);
            int y = Mathf.FloorToInt((worldPosition.y - originPosition.y) / cellSize);
            return new Vector2Int(x, y);
        }

        public Vector3 GetCellWorldPos(int x, int y)
        {
            return GetCellWorldPos(new Vector2Int(x, y));
        }

        public Vector3 GetCellWorldPos(Vector2Int cellXYPosition)
        {
            return new Vector3(cellXYPosition.x, cellXYPosition.y) * cellSize + originPosition;
        }

        public Vector3 GetCellCenterWorldPos(Vector2Int cellXYPosition)
        {
            return GetCellWorldPos(cellXYPosition) + new Vector3(cellSize, cellSize) * .5f;
        }

        public Vector3 GetCellCenterWorldPos(int x, int y)
        {
            return GetCellCenterWorldPos(new Vector2Int(x, y));
        }

        public IEnumerable<Vector2Int> GetAllCellXYPositions()
        {
            return gridDictionary.Keys;
        }

        public IEnumerable<TGridObject> GetAllGridObjects()
        {
            return gridDictionary.Values;
        }

        public int GetCellCount()
        {
            return gridDictionary.Count;
        }
        
        public void SetCellGridObject(int x, int y, TGridObject value)
        {
            SetCellGridObject(new Vector2Int(x, y), value);
        }
        
        public void SetCellGridObject(Vector2Int cellXYPosition, TGridObject value)
        {
            gridDictionary[cellXYPosition] = value;
            OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { CellXYPosition = cellXYPosition});
        }

        public void SetCellGridObject(Vector3 worldPosition, TGridObject value)
        {
            Vector2Int cellXYPos = GetCellXYPos(worldPosition);
            SetCellGridObject(cellXYPos, value);
        }

        public TGridObject GetCellGridObject(int x, int y)
        {
            return GetCellGridObject(new Vector2Int(x, y));
        }

        public TGridObject GetCellGridObject(Vector2Int cellXYPosition)
        {
            if (!HasCell(cellXYPosition)) return default;
            return gridDictionary[cellXYPosition];
        }

        public TGridObject GetCellGridObject(Vector3 worldPosition)
        {
            Vector2Int cellXYPosition = GetCellXYPos(worldPosition);
            return GetCellGridObject(cellXYPosition);
        }
    }
}