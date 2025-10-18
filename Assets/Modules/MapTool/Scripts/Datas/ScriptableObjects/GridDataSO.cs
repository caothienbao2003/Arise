using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MapTool
{
    [CreateAssetMenu(fileName = "NewGridData", menuName = "MapTool/Grid Data")]
    public class GridDataSO : ScriptableObject
    {
        [HorizontalGroup("Grid Settings")]
        [VerticalGroup("Grid Settings/Width")]
        public int width = 10;

        [HorizontalGroup("Grid Settings")]
        [VerticalGroup("Grid Settings/Height")]
        public int height = 10;

        [SerializeField, HideInInspector]
        public List<CellData> cellDatas = new();
    }
}