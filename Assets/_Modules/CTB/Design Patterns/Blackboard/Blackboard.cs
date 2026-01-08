using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CTB
{
    [Serializable]
    public class Blackboard
    {
        public List<string> AvailableKeys => BlackboardVariables.Select(e => e.Key).ToList();

        [Space] [OdinSerialize] [ListDrawerSettings(DraggableItems = true, ShowFoldout = false, ShowPaging = false, CustomAddFunction = nameof(CreateNewEntry))]
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
                entry.TypedValue.Value = value;
            }
            else
            {
                var newEntry = new BlackboardEntry
                {
                    Key = key,
                    TypedValue = new TypedValue(),
                    IsPersistent = false
                };
                newEntry.TypedValue.Value = value;
                if (value != null)
                {
                    newEntry.TypedValue.Type = value.GetType();
                }

                BlackboardVariables.Add(newEntry);
            }
        }

        public void ClearRuntimeData()
        {
            BlackboardVariables.Where(e => e.IsPersistent).ToList().ForEach(e => e = null);
        }

        public T Get<T>(string key)
        {
            var entry = BlackboardVariables.FirstOrDefault(e => e.Key == key);
            if (entry == null) return default;

            entry.EnsureTypedValueInitialized();
            return (entry.TypedValue.Value is T typed) ? typed : default;
        }
    }

    [Serializable]
    [HideReferenceObjectPicker]
    [InlineProperty]
    public class BlackboardEntry
    {
        [HorizontalGroup("Entry", 0.25f), HideLabel]
        public string Key;

        [HorizontalGroup("Entry", 0.3f), HideLabel]
        [ValueDropdown(nameof(GetAllSerializableTypes), NumberOfItemsBeforeEnablingSearch = 5)]
        [OnValueChanged(nameof(OnTypeChanged))]
        [ShowInInspector]
        [OdinSerialize]
        public Type EntryType;

        [HorizontalGroup("Entry"), HideLabel]
        [OdinSerialize]
        [InlineProperty]
        [HideReferenceObjectPicker]
        [ShowInInspector]
        public TypedValue TypedValue = new TypedValue();

        [HorizontalGroup("Entry", 0.05f), HideLabel]
        public bool IsPersistent;
        
        // Ensure TypedValue is never null
        public void EnsureTypedValueInitialized()
        {
            if (TypedValue == null)
            {
                TypedValue = new TypedValue();
                if (EntryType != null)
                {
                    TypedValue.Type = EntryType;
                }
            }
        }

        // Legacy compatibility property
        public object Value
        {
            get
            {
                EnsureTypedValueInitialized();
                return TypedValue.Value;
            }
            set
            {
                EnsureTypedValueInitialized();
                TypedValue.Value = value;
            }
        }

        private void OnTypeChanged()
        {
            EnsureTypedValueInitialized();
            TypedValue.Type = EntryType;
        }

        private IEnumerable<ValueDropdownItem<Type>> GetAllSerializableTypes()
        {
            var primitiveTypes = new List<ValueDropdownItem<Type>>
            {
                new ValueDropdownItem<Type>("Primitives/String", typeof(string)),
                new ValueDropdownItem<Type>("Primitives/Int", typeof(int)),
                new ValueDropdownItem<Type>("Primitives/Float", typeof(float)),
                new ValueDropdownItem<Type>("Primitives/Bool", typeof(bool)),
                new ValueDropdownItem<Type>("Primitives/Double", typeof(double)),
                new ValueDropdownItem<Type>("Primitives/Long", typeof(long)),
                new ValueDropdownItem<Type>("Primitives/Byte", typeof(byte)),
                new ValueDropdownItem<Type>("Primitives/Short", typeof(short)),
            };

            // ADD EDITOR-ONLY TYPES HERE
            var editorTypes = new List<ValueDropdownItem<Type>>();
#if UNITY_EDITOR
            editorTypes.Add(new ValueDropdownItem<Type>("Unity/Editor/SceneAsset", typeof(SceneAsset)));
            editorTypes.Add(new ValueDropdownItem<Type>("Unity/Editor/MonoScript", typeof(MonoScript)));
            editorTypes.Add(new ValueDropdownItem<Type>("Unity/Editor/DefaultAsset", typeof(DefaultAsset)));
#endif

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name.Contains("Assembly-CSharp") ||
                            a.GetName().Name.Contains("UnityEngine"));

            var otherTypes = assemblies.SelectMany(a => a.GetTypes())
                .Where(t => t != null && !t.IsAbstract && !t.IsInterface && !t.IsGenericType)
                .Where(t => typeof(UnityEngine.Object).IsAssignableFrom(t) ||
                            (t.IsSerializable && !t.IsPrimitive && t != typeof(string)))
                .Select(t =>
                {
                    string category;
                    if (typeof(ScriptableObject).IsAssignableFrom(t))
                    {
                        category = "Unity/ScriptableObjects";
                    }
                    else if (typeof(MonoBehaviour).IsAssignableFrom(t))
                    {
                        category = "Unity/Components";
                    }
                    else if (typeof(UnityEngine.Object).IsAssignableFrom(t))
                    {
                        category = "Unity/Other";
                    }
                    else
                    {
                        category = "Classes";
                    }

                    return new ValueDropdownItem<Type>($"{category}/{t.Name}", t);
                });

            // COMBINE ALL TYPES INCLUDING EDITOR TYPES
            return primitiveTypes
                .Concat(editorTypes)
                .Concat(otherTypes)
                .OrderBy(item => item.Text);
        }
    }
}