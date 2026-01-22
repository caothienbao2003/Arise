using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

namespace GridTool
{
    public class SpawnService : ISpawnService
    {
        [Inject] private readonly IGridService gridService;
        [Inject] private readonly IObjectPoolingService poolService;

        [Inject] private readonly SpawnDataSO spawnData;

        private readonly Dictionary<SpawnTypeSO, List<GameObject>> spawnedObjectsByType =
            new Dictionary<SpawnTypeSO, List<GameObject>>();

        private readonly Dictionary<GameObject, SpawnTypeSO> objectToType = new Dictionary<GameObject, SpawnTypeSO>();

        public void Initialize()
        {
            PrewarmPools();
            Debug.Log("[SpawnService] Initialized");
        }

        private void PrewarmPools()
        {
            var allTypes = new HashSet<SpawnTypeSO>();

            if (spawnData.FixedSpawns != null)
            {
                foreach (var fixedSpawn in spawnData.FixedSpawns)
                {
                    if (fixedSpawn.SpawnType != null)
                        allTypes.Add(fixedSpawn.SpawnType);
                }
            }

            if (spawnData.WaveSpawns != null)
            {
                foreach (var wave in spawnData.WaveSpawns)
                {
                    if (wave.SpawnEntries != null)
                    {
                        foreach (var entry in wave.SpawnEntries)
                        {
                            if (entry.SpawnType != null)
                                allTypes.Add(entry.SpawnType);
                        }
                    }
                }
            }

            foreach (var type in allTypes)
            {
                if (type.UsePooling && type.Prefab != null)
                {
                    poolService.PrewarmPool(type.Prefab, type.PoolPrewarmCount);
                }
            }
        }

        public void Initialize(SpawnDataSO spawnData)
        {
            throw new System.NotImplementedException();
        }

        public void SpawnAllFixed()
        {
            if (spawnData.FixedSpawns == null)
            {
                Debug.LogWarning("[SpawnService] No fixed spawns to spawn");
                return;
            }

            foreach (var fixedSpawn in spawnData.FixedSpawns)
            {
                if (fixedSpawn.SpawnOnLevelStart)
                {
                    SpawnFixed(fixedSpawn);
                }
            }

            Debug.Log($"[SpawnService] Spawned {spawnData.FixedSpawns.Count} fixed spawns");
        }

        public GameObject SpawnFixed(FixedSpawnConfig config)
        {
            if (config == null || config.SpawnType == null)
            {
                Debug.LogError("[SpawnService] Cannot spawn fixed: config is null");
                return null;
            }

            return SpawnAtPosition(config.SpawnType, config.Position, 0);
        }

        public void SpawnWave(WaveSpawnConfig waveConfig)
        {
            if (waveConfig == null || waveConfig.SpawnEntries == null)
            {
                Debug.LogError("[SpawnService] Cannot spawn wave: config is null");
                return;
            }

            Debug.Log($"[SpawnService] Spawning wave {waveConfig.WaveIndex}: {waveConfig.WaveName}");

            foreach (var entry in waveConfig.SpawnEntries)
            {
                if (entry.SpawnType == null)
                {
                    Debug.LogWarning("[SpawnService] Wave entry has null spawn type, skipping");
                    continue;
                }

                SpawnAtPosition(entry.SpawnType, entry.GridPosition, 0);
            }

            Debug.Log(
                $"[SpawnService] Wave {waveConfig.WaveIndex} spawned with {waveConfig.SpawnEntries.Count} entities");
        }

        public GameObject SpawnAtPosition(SpawnTypeSO spawnType, Vector2Int gridPosition, float rotationDegrees = 0f)
        {
            if (spawnType == null)
            {
                Debug.LogError("[SpawnService] Cannot spawn: spawnType is null");
                return null;
            }

            if (spawnType.Prefab == null)
            {
                Debug.LogError($"[SpawnService] Cannot spawn {spawnType.name}: prefab is null");
                return null;
            }

            if (!CanSpawnAt(spawnType, gridPosition))
            {
                Debug.LogWarning(
                    $"[SpawnService] Cannot spawn {spawnType.name} at {gridPosition}: validation failed");
                return null;
            }

            if (HasReachedMaxSpawns(spawnType))
            {
                Debug.LogWarning($"[SpawnService] Cannot spawn {spawnType.name}: max spawns reached");
                return null;
            }

            Vector3 worldPos = gridService.GridToWorld(gridPosition);
            Quaternion rotation = Quaternion.Euler(0, 0, rotationDegrees);

            GameObject spawnedObj;
            if (spawnType.UsePooling)
            {
                spawnedObj = poolService.GetPooledObject(spawnType.Prefab);
                spawnedObj.transform.position = worldPos;
                spawnedObj.transform.rotation = rotation;
            }
            else
            {
                spawnedObj = Object.Instantiate(spawnType.Prefab, worldPos, rotation);
            }

            if (!spawnedObjectsByType.ContainsKey(spawnType))
            {
                spawnedObjectsByType[spawnType] = new List<GameObject>();
            }

            spawnedObjectsByType[spawnType].Add(spawnedObj);
            objectToType[spawnedObj] = spawnType;

            gridService.SetCellOccupied(gridPosition, true);

            Debug.Log($"[SpawnService] Spawned {spawnType.name} at {gridPosition}");

            return spawnedObj;
        }

        public void DespawnObject(GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("[SpawnService] Cannot despawn null object");
                return;
            }

            if (!objectToType.ContainsKey(obj))
            {
                Debug.LogWarning($"[SpawnService] Object {obj.name} is not tracked by spawn service");
                return;
            }

            SpawnTypeSO type = objectToType[obj];

            if (spawnedObjectsByType.ContainsKey(type))
            {
                spawnedObjectsByType[type].Remove(obj);
            }

            objectToType.Remove(obj);

            Vector2Int gridPos = gridService.WorldToGrid(obj.transform.position);
            gridService.SetCellOccupied(gridPos, false);

            if (type.UsePooling)
            {
                var poolObj = obj.GetComponent<PoolObject>();
                if (poolObj != null)
                {
                    poolObj.Despawn();
                }
                else
                {
                    Debug.LogWarning($"[SpawnService] {obj.name} is pooled but has no PoolObject");
                    Object.Destroy(obj);
                }
            }
            else
            {
                Object.Destroy(obj);
            }

            Debug.Log($"[SpawnService] Despawned {type.name}");
        }

        public void DespawnAll()
        {
            var allObjects = objectToType.Keys.ToList();
            foreach (var obj in allObjects)
            {
                DespawnObject(obj);
            }

            Debug.Log("[SpawnService] Despawned all objects");
        }

        public List<Vector2Int> GetValidSummonPositions()
        {
            if (spawnData?.SummonZone == null)
            {
                Debug.LogWarning("[SpawnService] Summon zone not configured");
                return new List<Vector2Int>();
            }

            return spawnData.SummonZone.GetValidPositions(gridService);
        }

        public bool CanSpawnAt(SpawnTypeSO spawnType, Vector2Int position)
        {
            if (spawnType == null)
                return false;

            var cellData = gridService.GetCellAt(position);
            if (cellData == null)
                return false;

            bool isInSummonZone = IsInSummonZone(position);

            return spawnType.ValidateSpawnRules(
                position,
                cellData.IsWalkable,
                cellData.IsOccupied,
                isInSummonZone
            );
        }

        public bool IsInSummonZone(Vector2Int position)
        {
            return spawnData?.SummonZone?.IsInSummonZone(position) ?? false;
        }

        public List<GameObject> GetSpawnedObjects(SpawnTypeSO spawnType)
        {
            if (spawnType == null)
                return new List<GameObject>();

            if (!spawnedObjectsByType.ContainsKey(spawnType))
                return new List<GameObject>();

            spawnedObjectsByType[spawnType].RemoveAll(obj => obj == null);

            return new List<GameObject>(spawnedObjectsByType[spawnType]);
        }

        public int GetActiveSpawnCount(SpawnTypeSO spawnType)
        {
            return GetSpawnedObjects(spawnType).Count;
        }

        public bool HasReachedMaxSpawns(SpawnTypeSO spawnType)
        {
            if (spawnType == null)
                return true;

            if (spawnType.MaxSimultaneousSpawns < 0)
                return false;

            return GetActiveSpawnCount(spawnType) >= spawnType.MaxSimultaneousSpawns;
        }

        public List<WaveSpawnConfig> GetWavesForTrigger(WaveTrigger trigger, int value)
        {
            if (spawnData == null)
                return new List<WaveSpawnConfig>();

            return spawnData.GetWavesForTrigger(trigger, value);
        }
    }
}