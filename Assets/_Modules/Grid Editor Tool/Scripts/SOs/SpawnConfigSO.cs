using UnityEngine;

namespace GridTool
{
    [CreateAssetMenu(fileName = "New Spawn Type", menuName = "GridTool/Spawn Type")]
    public class SpawnConfigSO: ScriptableObject
    {
        [SerializeField] public string SpawnName;
        [SerializeField] public int MaxSpawn;
    }
}