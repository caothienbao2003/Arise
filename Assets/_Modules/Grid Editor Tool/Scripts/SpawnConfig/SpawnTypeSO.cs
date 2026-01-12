using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GridTool
{
    [CreateAssetMenu(fileName = "SpawnType_", menuName = "GridTool/Spawn Type")]
    public class SpawnTypeSO : ScriptableObject, IDisplayNameable
    {
        [TabGroup("Basic Info")]
        [SerializeField] private string displayName;

        public string DisplayName => displayName;

        // [TabGroup("Basic Info")]
        // [SerializeField] public string CategoryName;

        [TabGroup("Basic Info")]
        [SerializeField, TextArea(1, 5)] public string Description;

        [TabGroup("Basic Info")]
        [Required]
        [AssetsOnly]
        [PreviewField(75, ObjectFieldAlignment.Left)]
        [SerializeField] public GameObject Prefab;

        [TabGroup("Spawn Rules")]
        [InfoBox("Maximum number of this spawn type that can exist simultaneously. -1 = unlimited")]
        [SerializeField] public int MaxSimultaneousSpawns = -1;

        [TabGroup("Spawn Rules")]
        [InfoBox("If true, this spawn must be placed on walkable cells")]
        [SerializeField] public bool MustSpawnOnWalkable = true;

        [TabGroup("Spawn Rules")]
        [InfoBox("If true, this spawn can be placed in summon zone. If false, it cannot.")]
        [SerializeField] public bool CanSpawnInSummonZone = false;

        [TabGroup("Spawn Rules")]
        [InfoBox("If true, validate that spawn position is not already occupied")]
        [SerializeField] public bool CheckOccupancy = true;

        [TabGroup("Pool Settings")]
        [InfoBox("If true, use object pooling for this spawn type")]
        [SerializeField] public bool UsePooling = true;

        [TabGroup("Pool Settings")]
        [ShowIf(nameof(UsePooling))]
        [InfoBox("Number of objects to prewarm in the pool")]
        [SerializeField] public int PoolPrewarmCount = 10;

        [TabGroup("Pool Settings")]
        [ShowIf(nameof(UsePooling))]
        [InfoBox("Maximum pool size. -1 = unlimited")]
        [SerializeField] public int MaxPoolSize = 50;

        [TabGroup("Editor Visualization")]
        [SerializeField] public Color GizmoColor = Color.white;

        [TabGroup("Editor Visualization")]
        [SerializeField] public string IconText = "?";

        // [TabGroup("Advanced")]
        // [InfoBox("Additional configuration specific to this spawn type")]
        // [SerializeField, TextArea(2, 5)] public string CustomData;

        public bool ValidateSpawnRules(Vector2Int position, bool isWalkable, bool isOccupied, bool isInSummonZone)
        {
            if (MustSpawnOnWalkable && !isWalkable)
            {
                Debug.LogWarning($"[{DisplayName}] Cannot spawn on non-walkable cell at {position}");
                return false;
            }

            if (CheckOccupancy && isOccupied)
            {
                Debug.LogWarning($"[{DisplayName}] Cell at {position} is already occupied");
                return false;
            }

            if (!CanSpawnInSummonZone && isInSummonZone)
            {
                Debug.LogWarning($"[{DisplayName}] Cannot spawn in summon zone at {position}");
                return false;
            }

            return true;
        }

// #if UNITY_EDITOR
//         private bool IsSavedAsset => !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(this));
//
//         [BoxGroup("Actions"), PropertyOrder(100)]
//         [Button("Ping Prefab", ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1f)]
//         [HorizontalGroup("Actions/Buttons")]
//         [ShowIf(nameof(Prefab))]
//         private void PingPrefab()
//         {
//             if (Prefab != null)
//             {
//                 EditorGUIUtility.PingObject(Prefab);
//                 Selection.activeObject = Prefab;
//             }
//         }
//
//         [BoxGroup("Actions"), PropertyOrder(101)]
//         [Button("Open Asset", ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1f)]
//         [HorizontalGroup("Actions/Buttons")]
//         [ShowIf(nameof(IsSavedAsset))]
//         private void OpenAsset()
//         {
//             Selection.activeObject = this;
//             EditorGUIUtility.PingObject(this);
//         }
//
//         [BoxGroup("Actions"), PropertyOrder(102)]
//         [Button("Delete Asset", ButtonSizes.Medium), GUIColor(1f, 0.4f, 0.4f)]
//         [HorizontalGroup("Actions/Buttons")]
//         [ShowIf(nameof(IsSavedAsset))]
//         private void DeleteAsset()
//         {
//             string assetPath = AssetDatabase.GetAssetPath(this);
//
//             if (string.IsNullOrEmpty(assetPath))
//             {
//                 EditorUtility.DisplayDialog("Error", "Cannot delete this asset. Asset path not found.", "OK");
//                 return;
//             }
//
//             bool confirmed = EditorUtility.DisplayDialog(
//                 "Delete Spawn Type",
//                 $"Are you sure you want to delete '{SpawnName}'?\n\nPath: {assetPath}\n\nThis action cannot be undone!",
//                 "Delete",
//                 "Cancel"
//             );
//
//             if (confirmed)
//             {
//                 bool success = AssetDatabase.DeleteAsset(assetPath);
//
//                 if (success)
//                 {
//                     AssetDatabase.SaveAssets();
//                     Debug.Log($"Deleted Spawn Type: {SpawnName} at {assetPath}");
//                 }
//                 else
//                 {
//                     EditorUtility.DisplayDialog("Error", $"Failed to delete asset at:\n{assetPath}", "OK");
//                 }
//             }
//         }
// #endif
    }
}