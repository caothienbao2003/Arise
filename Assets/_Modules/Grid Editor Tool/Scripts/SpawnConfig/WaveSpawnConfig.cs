using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GridTool
{
    public enum WaveTrigger
    {
        TurnCount,
        EnemiesKilled,
        BossDefeated,
        Manual
    }

    [Serializable]
    [HideReferenceObjectPicker]
    public class WaveSpawnConfig
    {
        [TitleGroup("$GetWaveTitle")]
        [HorizontalGroup("$GetWaveTitle/Header", Width = 0.3f)]
        [HideLabel]
        [LabelText("Wave")]
        public int WaveIndex = 1;

        [HorizontalGroup("$GetWaveTitle/Header", Width = 0.7f)]
        [HideLabel]
        [LabelText("Name")]
        public string WaveName = "New Wave";

        [TitleGroup("$GetWaveTitle")]
        [BoxGroup("$GetWaveTitle/Trigger")]
        [HorizontalGroup("$GetWaveTitle/Trigger/Row")]
        [HideLabel]
        [LabelText("Trigger Type")]
        public WaveTrigger TriggerType = WaveTrigger.TurnCount;

        [HorizontalGroup("$GetWaveTitle/Trigger/Row")]
        [HideLabel]
        [ShowIf(nameof(ShowTriggerValue))]
        [LabelText("Value")]
        public int TriggerValue = 1;

        [BoxGroup("$GetWaveTitle/Trigger")]
        [HorizontalGroup("$GetWaveTitle/Trigger/Delay")]
        [LabelText("Spawn Delay (seconds)")]
        [Range(0f, 10f)]
        public float SpawnDelay = 0f;

        [TitleGroup("$GetWaveTitle")]
        [BoxGroup("$GetWaveTitle/Spawns")]
        [ListDrawerSettings(
            ShowFoldout = false,
            DraggableItems = true,
            ShowPaging = false,
            CustomAddFunction = nameof(CreateNewSpawnEntry))]
        public List<SpawnEntry> SpawnEntries = new List<SpawnEntry>();

        private bool ShowTriggerValue => TriggerType != WaveTrigger.Manual;

        private string GetWaveTitle()
        {
            return $"Wave {WaveIndex}: {WaveName}";
        }

        private SpawnEntry CreateNewSpawnEntry()
        {
            return new SpawnEntry
            {
                GridPosition = Vector2Int.zero
            };
        }

        public bool ShouldTrigger(WaveTrigger triggerType, int currentValue)
        {
            if (TriggerType != triggerType)
                return false;

            if (TriggerType == WaveTrigger.Manual)
                return false;

            return currentValue >= TriggerValue;
        }

        public bool Validate(IGridService gridService, Func<Vector2Int, bool> isInSummonZone)
        {
            if (SpawnEntries == null || SpawnEntries.Count == 0)
            {
                Debug.LogWarning($"Wave {WaveIndex} ({WaveName}) has no spawn entries");
                return false;
            }

            bool allValid = true;
            for (int i = 0; i < SpawnEntries.Count; i++)
            {
                var entry = SpawnEntries[i];
                bool inSummonZone = isInSummonZone(entry.GridPosition);
                
                if (!entry.Validate(gridService, inSummonZone))
                {
                    Debug.LogError($"Wave {WaveIndex} ({WaveName}), Entry {i} validation failed");
                    allValid = false;
                }
            }

            return allValid;
        }

        public int GetTotalSpawnCount()
        {
            return SpawnEntries?.Count ?? 0;
        }

        public Dictionary<SpawnTypeSO, int> GetSpawnCountByCategory()
        {
            var counts = new Dictionary<SpawnTypeSO, int>();

            if (SpawnEntries == null)
                return counts;

            foreach (var entry in SpawnEntries)
            {
                if (entry.SpawnType == null)
                    continue;

                var category = entry.SpawnType;
                if (!counts.ContainsKey(category))
                    counts[category] = 0;
                counts[category]++;
            }

            return counts;
        }
    }
}