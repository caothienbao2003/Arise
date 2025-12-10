using GridUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GridTool
{
    [Serializable]
    public class CellData: IPathNode
    {
        [SerializeField] private Vector2Int position; 
        
        public CellData PreviousCell;
        
        public Vector2 WorldPosition;
        public TerrainTypeSO TerrainType;

        public int GCost { get; set; }
        public int HCost { get; set; }
        public IPathNode PreviousNode { get; set; }
        public Vector2Int NodePosition
        {
            get => position;
            set => position = value;
        }
        public bool IsWalkable { get; set; }
    }
}