using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace GridTool
{
    [CreateAssetMenu(fileName = "NewSpawnData", menuName = "GridTool/Spawn Data")]
    public class SpawnDataSO : ScriptableObject
    {
        // [Title("Grid Reference")]
        // [Required]
        // [AssetsOnly]
        // [InfoBox("GridDataSO reference for this level")]
        // public GridDataSO GridDataReference;

        [Title("Fixed Spawns")]
        [InfoBox("Spawns that occur once at level start (King, Resources, etc.)")]
        [ListDrawerSettings(ShowFoldout = false, DraggableItems = true)]
        public List<FixedSpawnConfig> FixedSpawns = new List<FixedSpawnConfig>();

        [Title("Wave Spawns")]
        [InfoBox("Wave-based spawns triggered by game conditions")]
        [ListDrawerSettings(ShowFoldout = false, DraggableItems = true)]
        public List<WaveSpawnConfig> WaveSpawns = new List<WaveSpawnConfig>();

        [Title("Summon Zone")]
        [InlineProperty]
        [HideLabel]
        public SummonZoneConfig SummonZone = new SummonZoneConfig();

        // Pure data access methods only (no logic)
        // public Vector2Int? GetKingPosition()
        // {
        //     foreach (var spawn in FixedSpawns)
        //     {
        //         if (spawn.SpawnType != null && 
        //             spawn.SpawnType.CategoryName != null && 
        //             spawn.SpawnType.CategoryName.ToLower().Contains("king"))
        //         {
        //             return spawn.Position;
        //         }
        //     }
        //     return null;
        // }

        public List<WaveSpawnConfig> GetWavesForTrigger(WaveTrigger trigger, int value)
        {
            var result = new List<WaveSpawnConfig>();
            
            if (WaveSpawns == null)
                return result;

            foreach (var wave in WaveSpawns)
            {
                if (wave.ShouldTrigger(trigger, value))
                {
                    result.Add(wave);
                }
            }

            return result;
        }
    }
}