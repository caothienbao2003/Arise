using UnityEngine;

namespace GridTool
{
    public class SpawnPoint
    {
        public Vector2Int Position { get; set; }
        public SpawnConfigSO SpawnConfig { get; set; }
        public SpawnPoint(Vector2Int position, SpawnConfigSO spawnConfig)
        {
            Position = position;
            SpawnConfig = spawnConfig;
        }
    }
}