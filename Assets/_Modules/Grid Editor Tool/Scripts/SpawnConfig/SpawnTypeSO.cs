using _Modules.Grid_Editor_Tool.Scripts.Utils;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GridTool
{
    [CreateAssetMenu(fileName = "SpawnType_", menuName = "GridTool/Spawn Type")]
    public class SpawnTypeSO : ScriptableObjectWithActions
    {
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
                Debug.LogWarning($"[{name}] Cannot spawn on non-walkable cell at {position}");
                return false;
            }

            if (CheckOccupancy && isOccupied)
            {
                Debug.LogWarning($"[{name}] Cell at {position} is already occupied");
                return false;
            }

            if (!CanSpawnInSummonZone && isInSummonZone)
            {
                Debug.LogWarning($"[{name}] Cannot spawn in summon zone at {position}");
                return false;
            }

            return true;
        }
    }
}