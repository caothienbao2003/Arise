using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace MapTool
{
    [Serializable]
    public class CellData
    {
        [SerializeField] private CellType CellType;
        
        public CellType cellType => CellType;
    }

    public enum CellType
    {
        King,
        Spawn,
        Movable,
        Obstacle,
    }
}