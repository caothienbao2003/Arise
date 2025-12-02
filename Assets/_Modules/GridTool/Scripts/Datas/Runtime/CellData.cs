using GridUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GridTool
{
    [Serializable]
    public class CellData
    {
        private Grid<CellData> grid;

        public int GCost;
        public int HCost;
        public int FCost => GCost + HCost;
        
        public CellData PreviousCell;
        
        public Vector2Int CellPosition;
        
        
        
        public Vector2 WorldPosition;
        public TerrainTypeSO TerrainType;
        public Sprite TileSprite;
        
        // public void CalculateFCost()
        // {
        //     FCost = GCost + HCost;
        // }

        public List<CellData> GetNeighborCells()
        {
            List<CellData> neighbors = new List<CellData>();

            List<Vector2Int> surroundingPosition = new List<Vector2Int>();
            surroundingPosition.Add(CellPosition + Vector2Int.up);
            surroundingPosition.Add(CellPosition + Vector2Int.down);
            surroundingPosition.Add(CellPosition + Vector2Int.left);
            surroundingPosition.Add(CellPosition + Vector2Int.right);
            surroundingPosition.Add(CellPosition + Vector2Int.up + Vector2Int.left);
            surroundingPosition.Add(CellPosition + Vector2Int.up + Vector2Int.right);
            surroundingPosition.Add(CellPosition + Vector2Int.down + Vector2Int.left);
            surroundingPosition.Add(CellPosition + Vector2Int.down + Vector2Int.right);

            foreach (Vector2Int pos in surroundingPosition)
            {
                CellData neighborCell = grid.GetCellGridObject(pos);
                if (neighborCell != null)
                {
                    neighbors.Add(neighborCell);
                }
            }
            
            return neighbors;
        }
    }
}