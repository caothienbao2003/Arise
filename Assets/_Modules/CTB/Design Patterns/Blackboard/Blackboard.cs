using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace CTB
{
    [Serializable]
    public class Blackboard
    {
        [SerializeField]
        [OnValueChanged(nameof(SyncKeys))]
        public List<string> AvailableKeys = new List<string>();
        
        [Space]
        
        [SerializeField]
        [OnCollectionChanged(nameof(SyncKeys))]
        [ListDrawerSettings(DraggableItems = true, ShowFoldout = true, ShowPaging = false, HideAddButton = false)]
        public List<BlackboardEntry> BlackboardVariables = new List<BlackboardEntry>();

        public void SyncKeys()
        {
            foreach (var entry in BlackboardVariables)
            {
                entry.AvailableKeys = AvailableKeys;
            }
        }

        public void Set(string key, object value, bool persistent = false)
        {
            var entry = BlackboardVariables.FirstOrDefault(e => e.Key == key);
            if (entry != null)
            {
                entry.Value = value;
            }
            else
            {
                BlackboardVariables.Add(new BlackboardEntry
                {
                    Key = key,
                    Value = value,
                    IsPersistent = persistent
                });
            }
        }

        public T Get<T>(string key)
        {
            var entry = BlackboardVariables.FirstOrDefault(e => e.Key == key);
            return (entry != null && entry.Value is T typed) ? typed : default;
        }

        public void ClearRuntimeData()
        {
            BlackboardVariables.RemoveAll(e => !e.IsPersistent);
        }
    }

    [Serializable]
    public class BlackboardEntry
    {
        [HorizontalGroup("Entry", Width = 0.3f), HideLabel, ValueDropdown(nameof(AvailableKeys))]
        public string Key;

        // We use TWO fields. One for Assets, one for Strings/Data.
        [HorizontalGroup("Entry"), HideLabel, ShowIf(nameof(IsUnityAsset))]
        public UnityEngine.Object UnityValue;

        [HorizontalGroup("Entry"), HideLabel, HideIf(nameof(IsUnityAsset))]
        public string StringValue;

        [HorizontalGroup("Entry", Width = 40)]
        [ToggleLeft, HideLabel, Tooltip("Is this a Unity Asset?")]
        public bool IsUnityAsset;

        [HorizontalGroup("Entry", Width = 20), HideLabel, ToggleLeft]
        public bool IsPersistent = true;

        [HideInInspector, NonSerialized] public List<string> AvailableKeys;

        // Your code still uses .Value, so nothing else breaks!
        public object Value 
        {
            get => IsUnityAsset ? UnityValue : StringValue;
            set {
                if (value is UnityEngine.Object uo) { UnityValue = uo; IsUnityAsset = true; }
                else { StringValue = value?.ToString(); IsUnityAsset = false; }
            }
        }
    }
}