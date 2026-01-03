using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace SceneSetupTool
{
    [Serializable]
    public class SetScriptableObjectPropertyAction : SequenceAction
    {
        [Title("Target Object")]
        [ValueDropdown(nameof(GetBlackboardKeys))]
        [Required]
        public string BlackboardKey;

        [Title("Field Settings")]
        [ValueDropdown(nameof(GetFieldNames))]
        [OnValueChanged(nameof(OnFieldSelected))]
        [Required]
        public string FieldName;

        [HideInInspector]
        [OdinSerialize]
        private Type cachedFieldType;

        [ShowIf(nameof(IsFieldSelected))]
        [BoxGroup("Value Injection")]
        [LabelText("New Value")]
        [OdinSerialize]
        [InlineProperty]
        [ShowIf(nameof(IsUnityObjectType))]
        [AssetsOnly]
        public UnityEngine.Object NewValueAsset;

        [ShowIf(nameof(IsFieldSelected))]
        [BoxGroup("Value Injection")]
        [LabelText("New Value")]
        [OdinSerialize]
        [HideIf(nameof(IsUnityObjectType))]
        public object NewValuePrimitive;

        public object NewValue
        {
            get => IsUnityObjectType ? NewValueAsset : NewValuePrimitive;
            set
            {
                if (cachedFieldType != null && typeof(UnityEngine.Object).IsAssignableFrom(cachedFieldType))
                {
                    NewValueAsset = value as UnityEngine.Object;
                }
                else
                {
                    NewValuePrimitive = value;
                }
            }
        }

        private bool IsUnityObjectType => cachedFieldType != null && typeof(UnityEngine.Object).IsAssignableFrom(cachedFieldType);

        public override void Execute()
        {
            var host = Blackboard.Get<ScriptableObject>(BlackboardKey);
            if (host == null)
            {
                Debug.LogError($"[SetProperty] Host not found at key: {BlackboardKey}");
                return;
            }

            FieldInfo field = host.GetType().GetField(FieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                Debug.LogError($"[SetProperty] Field '{FieldName}' not found on {host.name}");
                return;
            }

            object valueToSet = NewValue;

            // Type validation
            if (valueToSet != null && !field.FieldType.IsAssignableFrom(valueToSet.GetType()))
            {
                Debug.LogError($"[SetProperty] Type mismatch: Cannot assign {valueToSet.GetType()} to {field.FieldType}");
                return;
            }

            // Apply the value
            field.SetValue(host, valueToSet);
            
            EditorUtility.SetDirty(host);
            Debug.Log($"<b>[SetProperty]</b> Set {host.name}.{FieldName} to {valueToSet}");
        }

        private void OnFieldSelected()
        {
            Type hostType = GetBlackboardEntryType();
            if (hostType == null)
            {
                cachedFieldType = null;
                return;
            }

            FieldInfo field = hostType.GetField(FieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                cachedFieldType = null;
                return;
            }

            cachedFieldType = field.FieldType;

            // Initialize with appropriate default value
            if (typeof(UnityEngine.Object).IsAssignableFrom(cachedFieldType))
            {
                NewValueAsset = null;
                NewValuePrimitive = null;
            }
            else if (cachedFieldType == typeof(string))
            {
                NewValuePrimitive = "";
                NewValueAsset = null;
            }
            else if (cachedFieldType == typeof(int))
            {
                NewValuePrimitive = 0;
                NewValueAsset = null;
            }
            else if (cachedFieldType == typeof(float))
            {
                NewValuePrimitive = 0f;
                NewValueAsset = null;
            }
            else if (cachedFieldType == typeof(bool))
            {
                NewValuePrimitive = false;
                NewValueAsset = null;
            }
            else if (cachedFieldType == typeof(double))
            {
                NewValuePrimitive = 0.0;
                NewValueAsset = null;
            }
            else if (cachedFieldType.IsValueType)
            {
                try
                {
                    NewValuePrimitive = Activator.CreateInstance(cachedFieldType);
                    NewValueAsset = null;
                }
                catch
                {
                    NewValuePrimitive = null;
                    NewValueAsset = null;
                }
            }
            else
            {
                NewValuePrimitive = null;
                NewValueAsset = null;
            }
        }

        private bool IsFieldSelected => !string.IsNullOrEmpty(FieldName);
        
        private IEnumerable<string> GetBlackboardKeys() => Blackboard?.AvailableKeys ?? new List<string>();

        private IEnumerable<string> GetFieldNames()
        {
            Type hostType = GetBlackboardEntryType();
            if (hostType == null)
            {
                return new List<string> { "Select a valid Blackboard key first" };
            }

            var fields = hostType
                .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(f => !f.IsLiteral && !f.IsInitOnly) // Exclude constants and readonly
                .Select(f => f.Name)
                .ToList();

            if (fields.Count == 0)
            {
                return new List<string> { "No fields found on this type" };
            }

            return fields;
        }

        // Helper method to get the Type from the Blackboard entry, not the value
        private Type GetBlackboardEntryType()
        {
            if (Blackboard == null || string.IsNullOrEmpty(BlackboardKey))
            {
                return null;
            }

            // Find the blackboard entry with this key
            var entry = Blackboard.BlackboardVariables.FirstOrDefault(e => e.Key == BlackboardKey);
            if (entry == null)
            {
                return null;
            }

            // Get the type from the entry's EntryType field
            Type entryType = entry.EntryType;
            
            // Make sure it's a ScriptableObject type
            if (entryType != null && typeof(ScriptableObject).IsAssignableFrom(entryType))
            {
                return entryType;
            }

            return null;
        }
    }
}