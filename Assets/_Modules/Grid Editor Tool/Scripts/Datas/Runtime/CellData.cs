using GridUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GridTool
{
    [Serializable]
    public class CellData : IPathNode, IGridObject
    {
        // [SerializeField] private Vector2Int gridPosition; 
        [field: SerializeField]
        public CellData PreviousCell;

        [field: SerializeField]
        public TerrainTypeSO TerrainType;

        [field: SerializeField]
        public int GCost { get; set; }

        [field: SerializeField]
        public int HCost { get; set; }

        [field: SerializeField]
        public IPathNode PreviousNode { get; set; }
        // public Vector2Int GridPosition
        // {
        //     get => gridPosition;
        //     set => gridPosition = value;
        // }

        [field: SerializeField]
        public Vector2Int GridPosition { get; set; }

        [field: SerializeField]
        public bool IsWalkable { get; set; }
    }
}