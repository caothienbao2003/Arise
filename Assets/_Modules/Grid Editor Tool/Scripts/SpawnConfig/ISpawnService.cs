using System.Collections.Generic;
using UnityEngine;

namespace GridTool
{
    public interface ISpawnService
    {
        void Initialize();
        
        void SpawnAllFixed();
        
        GameObject SpawnFixed(FixedSpawnConfig config);
        
        void SpawnWave(WaveSpawnConfig waveConfig);
        
        GameObject SpawnAtPosition(SpawnTypeSO spawnType, Vector2Int gridPosition, float rotationDegrees = 0f);
        
        void DespawnObject(GameObject obj);
        
        void DespawnAll();
        
        List<Vector2Int> GetValidSummonPositions();
        
        bool CanSpawnAt(SpawnTypeSO spawnType, Vector2Int position);
        
        bool IsInSummonZone(Vector2Int position);
        
        List<GameObject> GetSpawnedObjects(SpawnTypeSO spawnType);
        
        int GetActiveSpawnCount(SpawnTypeSO spawnType);
        
        bool HasReachedMaxSpawns(SpawnTypeSO spawnType);
        
        List<WaveSpawnConfig> GetWavesForTrigger(WaveTrigger trigger, int value);
    }
}