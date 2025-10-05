using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace MapTool
{
    [Serializable]
    public class CellData
    {
        public CellType CellType;
        public Vector2Int Position;
        public bool walkable = true;
    }

    public enum CellType
    {
        King,
        Spawn,
        Movable,
        Obstacle,
    }
}