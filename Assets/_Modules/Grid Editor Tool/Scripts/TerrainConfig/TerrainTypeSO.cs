using _Modules.Grid_Editor_Tool.Scripts.Utils;
using UnityEngine;
using Sirenix.OdinInspector;

namespace GridTool
{
    [CreateAssetMenu(fileName = "NewCellType", menuName = "Map Tool/Cell Type")]
    public class TerrainTypeSO : ScriptableObjectWithActions
    {
        [TabGroup("Basic info")] [SerializeField, TextArea(1, 5)]
        public string Description = "";

        [TabGroup("Properties")] [SerializeField]
        public bool IsWalkable = true;

        [TabGroup("Properties")] [SerializeField]
        public int Priority = 0;

        [TabGroup("Visualization")] [SerializeField]
        public Color CellColor = Color.white;

        [TabGroup("Visualization")] [SerializeField]
        public bool IsRender = true;
    }
}