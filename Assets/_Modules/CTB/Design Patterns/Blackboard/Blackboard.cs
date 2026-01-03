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
        public List<string> AvailableKeys => BlackboardVariables.Select(e => e.Key).ToList();

        [Space] [OdinSerialize] [ListDrawerSettings(DraggableItems = true, ShowFoldout = false, ShowPaging = false)]
        public List<BlackboardEntry> BlackboardVariables = new List<BlackboardEntry>();

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
                    IsPersistent = persistent,
                    TypedValue = new TypedValue()
                };
                newEntry.TypedValue.Value = value;
                if (value != null)
                {
                    newEntry.TypedValue.Type = value.GetType();
                }

                BlackboardVariables.Add(newEntry);
            }
        }

        public T Get<T>(string key)
        {
            var entry = BlackboardVariables.FirstOrDefault(e => e.Key == key);
            if (entry == null) return default;

            entry.EnsureTypedValueInitialized();
            return (entry.TypedValue.Value is T typed) ? typed : default;
        }

        public void ClearRuntimeData()
        {
            BlackboardVariables.RemoveAll(e => !e.IsPersistent);
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

        [HorizontalGroup("Entry", 20), HideLabel, ToggleLeft]
        public bool IsPersistent = true;

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

            return primitiveTypes.Concat(otherTypes).OrderBy(item => item.Text);
        }
    }
}

// using Sirenix.OdinInspector;
// using Sirenix.Serialization;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using UnityEngine.Serialization;
//
// namespace CTB
// {
//     [Serializable]
//     public class Blackboard
//     {
//         public List<string> AvailableKeys => BlackboardVariables.Select(e => e.Key).ToList();
//
//         [Space]
//         [OdinSerialize]
//         [ListDrawerSettings(DraggableItems = true, ShowFoldout = false, ShowPaging = false)]
//         public List<BlackboardEntry> BlackboardVariables = new List<BlackboardEntry>();
//
//         public void Set(string key, object value, bool persistent = false)
//         {
//             var entry = BlackboardVariables.FirstOrDefault(e => e.Key == key);
//             if (entry != null)
//             {
//                 entry.Value = value;
//             }
//             else
//             {
//                 BlackboardVariables.Add(new BlackboardEntry
//                 {
//                     Key = key,
//                     Value = value
//                 });
//             }
//         }
//
//         public T Get<T>(string key)
//         {
//             var entry = BlackboardVariables.FirstOrDefault(e => e.Key == key);
//             return (entry != null && entry.Value is T typed) ? typed : default;
//         }
//     }
//
//     [Serializable]
//     [HideReferenceObjectPicker]
//     [InlineProperty]
//     public class BlackboardEntry
//     {
//         [HorizontalGroup("Entry", 0.25f), HideLabel]
//         public string Key;
//
//         [HorizontalGroup("Entry", 0.3f), HideLabel]
//         [ValueDropdown(nameof(GetAllSerializableTypes), NumberOfItemsBeforeEnablingSearch = 5)]
//         [OnValueChanged(nameof(OnTypeChanged))]
//         [ShowInInspector]
//         [OdinSerialize]
//         // CRITICAL: This must be here!
//         public Type EntryType;
//
//         // For Unity Object types (ScriptableObject, GameObject, etc.)
//         [HorizontalGroup("Entry"), HideLabel]
//         [ShowIf(nameof(IsUnityObjectType))]
//         [AssetsOnly]
//         [ShowInInspector]
//         [ValueDropdown(nameof(GetValueDropdown), AppendNextDrawer = true, DisableListAddButtonBehaviour = true)]
//         public UnityEngine.Object ValueAsset;
//
//         // For Primitive and other serializable types
//         [ShowInInspector]
//         [HorizontalGroup("Entry"), HideLabel] [OdinSerialize] [HideIf(nameof(IsUnityObjectType))]
//         public object ValuePrimitive;
//
//         // [HideInInspector, NonSerialized] public List<string> AvailableKeys;
//
//         // Unified Value property
//         public object Value
//         {
//             get => IsUnityObjectType ? (object)ValueAsset : ValuePrimitive;
//             set
//             {
//                 if (EntryType != null && typeof(UnityEngine.Object).IsAssignableFrom(EntryType))
//                 {
//                     ValueAsset = value as UnityEngine.Object;
//                     ValuePrimitive = null;
//                 }
//                 else
//                 {
//                     ValuePrimitive = value;
//                     ValueAsset = null;
//                 }
//             }
//         }
//
//         private bool IsUnityObjectType => EntryType != null && typeof(UnityEngine.Object).IsAssignableFrom(EntryType);
//
//         private IEnumerable<ValueDropdownItem<object>> GetValueDropdown()
//         {
//             if (EntryType == null) yield break;
//
//             // Only for Unity Objects
//             if (typeof(UnityEngine.Object).IsAssignableFrom(EntryType))
//             {
// #if UNITY_EDITOR
//                 // Use AssetDatabase to find all assets of this type in the project
//                 string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:{EntryType.Name}");
//
//                 foreach (string guid in guids)
//                 {
//                     string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
//                     UnityEngine.Object asset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, EntryType);
//
//                     if (asset != null)
//                     {
//                         yield return new ValueDropdownItem<object>(asset.name, asset);
//                     }
//                 }
//
//                 // Also include objects currently in the scene/memory
//                 var loadedObjects = Resources.FindObjectsOfTypeAll(EntryType)
//                     .Where(obj =>
//                         UnityEditor.AssetDatabase.Contains(obj) &&
//                         (obj.hideFlags & HideFlags.HideInInspector) == 0);
//
//                 foreach (var obj in loadedObjects)
//                 {
//                     string assetPath = UnityEditor.AssetDatabase.GetAssetPath(obj);
//                     if (!string.IsNullOrEmpty(assetPath) &&
//                         !guids.Any(g => UnityEditor.AssetDatabase.GUIDToAssetPath(g) == assetPath))
//                     {
//                         yield return new ValueDropdownItem<object>(obj.name, obj);
//                     }
//                 }
// #else
//             // Runtime fallback
//             var objects = Resources.FindObjectsOfTypeAll(EntryType)
//                 .Where(obj => (obj.hideFlags & HideFlags.HideInInspector) == 0);
//
//             foreach (var obj in objects)
//             {
//                 yield return new ValueDropdownItem<object>(obj.name, obj);
//             }
// #endif
//             }
//         }
//
//         private void OnTypeChanged()
//         {
//             if (EntryType == null)
//             {
//                 ValueAsset = null;
//                 ValuePrimitive = null;
//                 return;
//             }
//
//             // Check if current value is compatible
//             if (Value != null && EntryType.IsAssignableFrom(Value.GetType())) return;
//
//             // Initialize based on type
//             if (typeof(UnityEngine.Object).IsAssignableFrom(EntryType))
//             {
//                 ValueAsset = null;
//                 ValuePrimitive = null;
//             }
//             else if (EntryType == typeof(string))
//             {
//                 ValuePrimitive = "";
//                 ValueAsset = null;
//             }
//             else if (EntryType == typeof(int))
//             {
//                 ValuePrimitive = 0;
//                 ValueAsset = null;
//             }
//             else if (EntryType == typeof(float))
//             {
//                 ValuePrimitive = 0f;
//                 ValueAsset = null;
//             }
//             else if (EntryType == typeof(bool))
//             {
//                 ValuePrimitive = false;
//                 ValueAsset = null;
//             }
//             else if (EntryType == typeof(double))
//             {
//                 ValuePrimitive = 0.0;
//                 ValueAsset = null;
//             }
//             else if (EntryType == typeof(long))
//             {
//                 ValuePrimitive = 0L;
//                 ValueAsset = null;
//             }
//             else if (EntryType == typeof(byte))
//             {
//                 ValuePrimitive = (byte)0;
//                 ValueAsset = null;
//             }
//             else if (EntryType == typeof(short))
//             {
//                 ValuePrimitive = (short)0;
//                 ValueAsset = null;
//             }
//             else if (EntryType.IsEnum)
//             {
//                 ValuePrimitive = Enum.GetValues(EntryType).GetValue(0);
//                 ValueAsset = null;
//             }
//             else if (EntryType.IsPrimitive)
//             {
//                 ValuePrimitive = Activator.CreateInstance(EntryType);
//                 ValueAsset = null;
//             }
//             else
//             {
//                 try
//                 {
//                     ValuePrimitive = Activator.CreateInstance(EntryType);
//                     ValueAsset = null;
//                 }
//                 catch
//                 {
//                     ValuePrimitive = null;
//                     ValueAsset = null;
//                 }
//             }
//         }
//
//         private IEnumerable<ValueDropdownItem<Type>> GetAllSerializableTypes()
//         {
//             // Manually add common primitive types first
//             var primitiveTypes = new List<ValueDropdownItem<Type>>
//             {
//                 new ValueDropdownItem<Type>("Primitives/String", typeof(string)),
//                 new ValueDropdownItem<Type>("Primitives/Int", typeof(int)),
//                 new ValueDropdownItem<Type>("Primitives/Float", typeof(float)),
//                 new ValueDropdownItem<Type>("Primitives/Bool", typeof(bool)),
//                 new ValueDropdownItem<Type>("Primitives/Double", typeof(double)),
//                 new ValueDropdownItem<Type>("Primitives/Long", typeof(long)),
//                 new ValueDropdownItem<Type>("Primitives/Byte", typeof(byte)),
//                 new ValueDropdownItem<Type>("Primitives/Short", typeof(short)),
//             };
//
//             // Get Unity types and other serializable types
//             var assemblies = AppDomain.CurrentDomain.GetAssemblies()
//                 .Where(a => a.GetName().Name.Contains("Assembly-CSharp") ||
//                             a.GetName().Name.Contains("UnityEngine"));
//
//             var otherTypes = assemblies.SelectMany(a => a.GetTypes())
//                 .Where(t => t != null && !t.IsAbstract && !t.IsInterface && !t.IsGenericType)
//                 .Where(t => typeof(UnityEngine.Object).IsAssignableFrom(t) ||
//                             (t.IsSerializable && !t.IsPrimitive &&
//                              t != typeof(string))) // Exclude primitives we already added
//                 .Select(t =>
//                 {
//                     string category;
//                     if (typeof(ScriptableObject).IsAssignableFrom(t))
//                     {
//                         category = "Unity/ScriptableObjects";
//                     }
//                     else if (typeof(MonoBehaviour).IsAssignableFrom(t))
//                     {
//                         category = "Unity/Components";
//                     }
//                     else if (typeof(UnityEngine.Object).IsAssignableFrom(t))
//                     {
//                         category = "Unity/Other";
//                     }
//                     else
//                     {
//                         category = "Classes";
//                     }
//
//                     return new ValueDropdownItem<Type>($"{category}/{t.Name}", t);
//                 });
//
//             // Combine primitives and other types
//             return primitiveTypes.Concat(otherTypes).OrderBy(item => item.Text);
//         }
//     }
// }