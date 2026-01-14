using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SceneSetupTool;

namespace CTB.DesignPatterns.Blackboard
{
    [Serializable]
    public class Blackboard
    {
        public List<string> AvailableKeys => BlackboardVariables.Select(e => e.Key).ToList();

        [Space] 
        [OdinSerialize] 
        [ListDrawerSettings(
            DraggableItems = true, 
            ShowFoldout = false, 
            ShowPaging = false, 
            CustomAddFunction = nameof(CreateNewEntry))]
        public List<BlackboardEntry> BlackboardVariables = new List<BlackboardEntry>();

        private BlackboardEntry CreateNewEntry()
        {
            return new BlackboardEntry
            {
                IsPersistent = true
            };
        }
        
        public void Set(string key, object value, bool persistent = false)
        {
            var entry = BlackboardVariables.FirstOrDefault(e => e.Key == key);
            if (entry != null)
            {
                entry.EnsureTypedValueInitialized();
                entry.TypeSelector.TypedValue.Value = value;
            }
            else
            {
                var newEntry = new BlackboardEntry
                {
                    Key = key,
                    IsPersistent = false
                };
                newEntry.TypeSelector = new TypeSelector();
                newEntry.TypeSelector.TypedValue = new TypedValue();
                newEntry.TypeSelector.TypedValue.Value = value;
                if (value != null)
                {
                    newEntry.TypeSelector.SelectedType = value.GetType();
                    newEntry.TypeSelector.TypedValue.Type = value.GetType();
                }

                BlackboardVariables.Add(newEntry);
            }
        }

        public void ClearRuntimeData()
        {
            BlackboardVariables.Where(e => !e.IsPersistent).ToList().ForEach(e => 
            {
                e.EnsureTypedValueInitialized();
                e.TypeSelector.TypedValue.Value = null;
            });
        }

        public T Get<T>(string key)
        {
            var entry = BlackboardVariables.FirstOrDefault(e => e.Key == key);
            if (entry == null) return default;

            entry.EnsureTypedValueInitialized();
            return (entry.TypeSelector.TypedValue.Value is T typed) ? typed : default;
        }
    }

    [Serializable]
    [HideReferenceObjectPicker]
    [InlineProperty]
    public class BlackboardEntry
    {
        [HorizontalGroup("Entry", 0.2f)]
        [HideLabel]
        public string Key;

        [HorizontalGroup("Entry", 0.75f)]
        [HideLabel]
        [InlineProperty]
        [OdinSerialize]
        [ShowInInspector]
        public TypeSelector TypeSelector = new TypeSelector(TypeFilter.None, withValueField: true, horizontal: true);

        [HorizontalGroup("Entry", 0.05f)]
        [HideLabel]
        public bool IsPersistent;

        // Legacy compatibility properties
        public Type EntryType
        {
            get => TypeSelector?.SelectedType;
            set
            {
                if (TypeSelector != null)
                    TypeSelector.SelectedType = value;
            }
        }

        public TypedValue TypedValue
        {
            get
            {
                EnsureTypedValueInitialized();
                return TypeSelector.TypedValue;
            }
            set
            {
                if (TypeSelector != null)
                    TypeSelector.TypedValue = value;
            }
        }

        public object Value
        {
            get
            {
                EnsureTypedValueInitialized();
                return TypeSelector.TypedValue.Value;
            }
            set
            {
                EnsureTypedValueInitialized();
                TypeSelector.TypedValue.Value = value;
            }
        }

        public void EnsureTypedValueInitialized()
        {
            if (TypeSelector == null)
            {
                TypeSelector = new TypeSelector(TypeFilter.None, withValueField: true, horizontal: true);
            }
            
            TypeSelector.EnsureTypedValueInitialized();
        }
    }
}