using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace MapTool
{
    [Serializable]
    public class CellData
    {
        public CellTypeSO CellType;
        public Vector2Int Position;
    }
}