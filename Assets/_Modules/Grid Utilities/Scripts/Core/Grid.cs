using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GridUtilities
{
    public class Grid<TGridObject>
    {
        private Dictionary<Vector2Int, TGridObject> gridObjectDictionary;

        private Vector3 originPosition;
        private float cellSize;

        public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;

        // public delegate TGridObject CreateGridObjectDelegate(Grid<TGridObject> grid, int x, int y);

        public class OnGridValueChangedEventArgs : EventArgs
        {
            public Vector2Int CellXYPosition;
        }

        public Grid(Vector3 originPosition = default, float cellSize = 1f)
        {
            gridObjectDictionary = new Dictionary<Vector2Int, TGridObject>();

            Debug.Log($"Created grid with cell size {cellSize}");
            
            OnGridValueChanged += HandleGridValueChanged;
            
            this.cellSize = cellSize;
            this.originPosition = originPosition;
        }
        
        private void HandleGridValueChanged(object sender, OnGridValueChangedEventArgs eventArgs)
        {
            Debug.Log($"{sender.GetType().Name} changed at {eventArgs.CellXYPosition}");
        }

        private bool HasCell(Vector2Int cellXYPosition)
        {
            return gridObjectDictionary.ContainsKey(cellXYPosition);
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
            return gridObjectDictionary.Keys;
        }

        public IEnumerable<TGridObject> GetAllGridObjects()
        {
            return gridObjectDictionary.Values;
        }

        public int GetCellCount()
        {
            return gridObjectDictionary.Count;
        }
        
        public void SetCellGridObject(int x, int y, TGridObject value)
        {
            SetCellGridObject(new Vector2Int(x, y), value);
        }
        
        public void SetCellGridObject(Vector2Int cellXYPosition, TGridObject value)
        {
            gridObjectDictionary[cellXYPosition] = value;
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
            return gridObjectDictionary[cellXYPosition];
        }

        public TGridObject GetCellGridObject(Vector3 worldPosition)
        {
            Vector2Int cellXYPosition = GetCellXYPos(worldPosition);
            return GetCellGridObject(cellXYPosition);
        }

        public List<TGridObject> GetNeighbors(Vector2Int cellXYPosition)
        {
            List<TGridObject> neighbors = new List<TGridObject>();
            foreach (Vector2Int neighborPosition in GetNeighborPositions(cellXYPosition))
            {
                if (!HasCell(neighborPosition)) continue;
                neighbors.Add(GetCellGridObject(neighborPosition));
            }
            return neighbors;
        }
        
        public List<Vector2Int> GetNeighborPositions(Vector2Int cellXYPosition)
        {
            List<Vector2Int> neighborPosition = new List<Vector2Int>();
            neighborPosition.Add(cellXYPosition + Vector2Int.up);
            neighborPosition.Add(cellXYPosition + Vector2Int.down);
            neighborPosition.Add(cellXYPosition + Vector2Int.left);
            neighborPosition.Add(cellXYPosition + Vector2Int.right);
            neighborPosition.Add(cellXYPosition + Vector2Int.up + Vector2Int.left);
            neighborPosition.Add(cellXYPosition + Vector2Int.up + Vector2Int.right);
            neighborPosition.Add(cellXYPosition + Vector2Int.down + Vector2Int.left);
            neighborPosition.Add(cellXYPosition + Vector2Int.down + Vector2Int.right);
            return neighborPosition;
        }
    }
}