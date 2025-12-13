using GridUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GridTool
{
    [Serializable]
    public class CellData: IPathNode, IGridObject
    {
        [SerializeField] private Vector2Int gridPosition; 
        
        public CellData PreviousCell;
        
        public TerrainTypeSO TerrainType;

        public int GCost { get; set; }
        public int HCost { get; set; }
        public IPathNode PreviousNode { get; set; }
        public Vector2Int GridPosition
        {
            get => gridPosition;
            set => gridPosition = value;
        }
        public bool IsWalkable { get; set; }
    }
}