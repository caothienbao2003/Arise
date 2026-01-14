#if UNITY_EDITOR
using CTB;
using Sirenix.OdinInspector;
using UnityEditor;
#endif

using GridTool;
using UnityEngine;

namespace GridUtilities
{
    [CreateAssetMenu(
        fileName = "PathfindingProfile",
        menuName = "Pathfinding/Pathfinding Profile"
    )]
    public class PathfindingProfileSO : ScriptableObject, IDisplayNameable
    {
        [SerializeField] private string displayName;
        public string DisplayName => displayName;
        [Header("Movement")] public bool AllowDiagonal = true;

        [Tooltip("Prevent cutting corners when moving diagonally")]
        public bool BlockDiagonalCorner = true;

        public int MoveStraightCost = 10;
        public int MoveDiagonalCost = 14;

#if UNITY_EDITOR
        [Button(ButtonSizes.Medium)]
        [GUIColor(0.4f, 0.8f, 1f)]
        public void OpenAsset()
        {
            Selection.activeObject = this;
            EditorGUIUtility.PingObject(this);
        }
#endif
    }
}