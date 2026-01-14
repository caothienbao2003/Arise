using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CTB;
using CTB.DesignPatterns.Blackboard;
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

        [ShowIf(nameof(IsFieldSelected))]
        [BoxGroup("Value Injection")]
        [LabelText("New Value")]
        [OdinSerialize]
        [InlineProperty]
        [HideReferenceObjectPicker]
        public TypedValue NewValue = new TypedValue();

        private bool IsFieldSelected => !string.IsNullOrEmpty(FieldName);

        public override void Execute()
        {
            var host = Blackboard.Get<ScriptableObject>(BlackboardKey);
            if (host == null)
            {
                Debug.LogError($"[SetProperty] Host not found at key: {BlackboardKey}");
                return;
            }

            FieldInfo field = GetFieldInfo(host.GetType());
            if (field == null)
            {
                Debug.LogError($"[SetProperty] Field '{FieldName}' not found on {host.name}");
                return;
            }

            object valueToSet = NewValue.Value;

            if (!ValidateType(field.FieldType, valueToSet))
            {
                Debug.LogError($"[SetProperty] Type mismatch: Cannot assign {valueToSet?.GetType()} to {field.FieldType}");
                return;
            }

            field.SetValue(host, valueToSet);
            EditorUtility.SetDirty(host);
            Debug.Log($"<b>[SetProperty]</b> Set {host.name}.{FieldName} to {valueToSet}");
        }

        private void OnFieldSelected()
        {
            Type hostType = GetBlackboardEntryType();
            if (hostType == null)
            {
                NewValue.Type = null;
                return;
            }

            FieldInfo field = GetFieldInfo(hostType);
            if (field == null)
            {
                NewValue.Type = null;
                return;
            }

            NewValue.Type = field.FieldType;
        }

        private IEnumerable<string> GetBlackboardKeys() => 
            Blackboard?.AvailableKeys ?? new List<string>();

        private IEnumerable<string> GetFieldNames()
        {
            Type hostType = GetBlackboardEntryType();
            if (hostType == null)
            {
                return new List<string> { "Select a valid Blackboard key first" };
            }

            var fields = hostType
                .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(f => !f.IsLiteral && !f.IsInitOnly)
                .Select(f => f.Name)
                .ToList();

            return fields.Count == 0 
                ? new List<string> { "No fields found on this type" } 
                : fields;
        }

        private Type GetBlackboardEntryType()
        {
            if (Blackboard == null || string.IsNullOrEmpty(BlackboardKey))
                return null;

            var entry = Blackboard.BlackboardVariables.FirstOrDefault(e => e.Key == BlackboardKey);
            if (entry?.EntryType == null)
                return null;

            return typeof(ScriptableObject).IsAssignableFrom(entry.EntryType) 
                ? entry.EntryType 
                : null;
        }

        private FieldInfo GetFieldInfo(Type hostType)
        {
            return hostType?.GetField(FieldName, 
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private bool ValidateType(Type targetType, object value)
        {
            return value == null || targetType.IsAssignableFrom(value.GetType());
        }
    }
}