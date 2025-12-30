using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CTB
{
    [Serializable]
    public class Blackboard
    {
        [TableList(AlwaysExpanded = true)]
        public List<BlackboardEntry> Entries = new List<BlackboardEntry>();

        // This replaces the old AvailableKeys list
        public IEnumerable<string> GetAvailableKeys => 
            Entries.Select(e => e.Key).Where(k => !string.IsNullOrEmpty(k));

        public void Set(string key, object value, bool persistent = false)
        {
            var entry = Entries.FirstOrDefault(e => e.Key == key);
            if (entry != null)
            {
                entry.Value = value;
            }
            else
            {
                Entries.Add(new BlackboardEntry { 
                    Key = key, 
                    Value = value, 
                    IsPersistent = persistent 
                });
            }
        }

        public T Get<T>(string key)
        {
            var entry = Entries.FirstOrDefault(e => e.Key == key);
            return (entry != null && entry.Value is T typed) ? typed : default;
        }

        public void ClearRuntimeData()
        {
            // // Remove entries created during execution (IsPersistent == false)
            // Entries.RemoveAll(e => !e.IsPersistent);
            //
            // // Optional: Reset values of persistent entries to null so you start fresh
            foreach (var entry in Entries)
            {
                if (entry.IsPersistent)
                {
                    entry.Value = null;
                }
            }
        }
    }

    [Serializable]
    public class BlackboardEntry
    {
        public string Key;

        [ShowInInspector]
        [OdinSerialize]
        public object Value;

        [Tooltip("Is Persistent? (Checked = Design-time config, Unchecked = Runtime data)")]
        public bool IsPersistent = true;
    }
}