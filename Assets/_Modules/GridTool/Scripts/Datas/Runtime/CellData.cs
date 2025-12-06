using GridUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GridTool
{
    [Serializable]
    public class CellData: IPathNode
    {
        public CellData PreviousCell;
        
        public Vector2 WorldPosition;
        public TerrainTypeSO TerrainType;

        public int GCost { get; set; }
        public int HCost { get; set; }
        public IPathNode PreviousNode { get; set; }
        public Vector2Int NodePosition { get; set; }
        public bool IsWalkable { get; set; }
    }
}