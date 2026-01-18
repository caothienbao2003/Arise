#if UNITY_EDITOR
using CTB;
using Sirenix.OdinInspector;
using UnityEditor;
#endif

using _Modules.Grid_Editor_Tool.Scripts.Utils;
using GridTool;
using UnityEngine;

namespace GridUtilities
{
    [CreateAssetMenu(
        fileName = "PathfindingProfile",
        menuName = "Pathfinding/Pathfinding Profile"
    )]
    public class PathfindingProfileSO : ScriptableObjectWithActions
    {
        [Header("Movement")] public bool AllowDiagonal = true;

        [Tooltip("Prevent cutting corners when moving diagonally")]
        public bool BlockDiagonalCorner = true;

        public int MoveStraightCost = 10;
        public int MoveDiagonalCost = 14;
    }
}