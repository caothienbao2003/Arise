using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CTB;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace SceneSetupTool
{
    [Serializable]
    public class SetPropertiesAction : SequenceAction
    {
        [Title("Target Object")]
        [ValueDropdown(nameof(GetBlackboardKeys))]
        [Required]
        [OnValueChanged(nameof(OnTargetChanged))]
        public string BlackboardKey;

        [Title("Field Assignments")]
        [OdinSerialize]
        [ListDrawerSettings(
            DraggableItems = false, 
            ShowFoldout = false, 
            ShowPaging = false,
            CustomAddFunction = nameof(AddFieldAssignment)
        )]
        public List<FieldAssignment> FieldAssignments = new List<FieldAssignment>();

        [ShowIf(nameof(HasTarget))]
        [BoxGroup("Info")]
        [ReadOnly]
        [LabelText("Target Type")]
        [DisplayAsString]
        private string targetTypeDisplay;

        private bool HasTarget => !string.IsNullOrEmpty(BlackboardKey);

        public override void Execute()
        {
            var host = Blackboard.Get<object>(BlackboardKey);
            if (host == null)
            {
                Debug.LogError($"[SetProperties] Target object not found at key: {BlackboardKey}");
                return;
            }

            int successCount = 0;
            int failCount = 0;

            foreach (var assignment in FieldAssignments)
            {
                if (string.IsNullOrEmpty(assignment.FieldName))
                {
                    Debug.LogWarning($"[SetProperties] Skipping empty field assignment");
                    continue;
                }

                if (ExecuteFieldAssignment(host, assignment))
                {
                    successCount++;
                }
                else
                {
                    failCount++;
                }
            }

            // Mark dirty if it's a Unity Object
            if (host is UnityEngine.Object unityObj)
            {
                EditorUtility.SetDirty(unityObj);
            }

            Debug.Log($"<b>[SetProperties]</b> {host.GetType().Name}: {successCount} succeeded, {failCount} failed");
        }

        private bool ExecuteFieldAssignment(object host, FieldAssignment assignment)
        {
            FieldInfo field = host.GetType().GetField(
                assignment.FieldName, 
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic
            );

            if (field == null)
            {
                Debug.LogError($"[SetProperties] Field '{assignment.FieldName}' not found on {host.GetType().Name}");
                return false;
            }

            // Get TypedValue (either direct or from Blackboard), then extract the actual value
            TypedValue typedValue = assignment.Value.GetValue(key => 
            {
                // When loading from Blackboard, we get the raw object, so wrap it in a TypedValue
                object blackboardValue = Blackboard.Get<object>(key);
                if (blackboardValue is TypedValue tv)
                    return tv;
                
                // If it's not a TypedValue, create one and set its value
                var newTypedValue = new TypedValue();
                if (blackboardValue != null)
                    newTypedValue.Type = blackboardValue.GetType();
                newTypedValue.Value = blackboardValue;
                return newTypedValue;
            });

            object valueToSet = typedValue?.Value;

            if (!ValidateType(field.FieldType, valueToSet))
            {
                Debug.LogError($"[SetProperties] Type mismatch for {assignment.FieldName}: Cannot assign {valueToSet?.GetType()?.Name ?? "null"} to {field.FieldType.Name}");
                return false;
            }

            field.SetValue(host, valueToSet);
            
            string valueDisplay = assignment.Value.UseBlackboard 
                ? $"{valueToSet?.ToString() ?? "null"} (from {assignment.Value.BlackboardKey})"
                : valueToSet?.ToString() ?? "null";
            
            Debug.Log($"  - Set {assignment.FieldName} = {valueDisplay}");
            return true;
        }

        private void OnTargetChanged()
        {
            FieldAssignments.Clear();
            UpdateTargetTypeDisplay();
        }

        private void UpdateTargetTypeDisplay()
        {
            Type hostType = GetBlackboardEntryType();
            targetTypeDisplay = hostType != null 
                ? $"Editing: {hostType.Name}" 
                : "No target selected";
        }

        private FieldAssignment AddFieldAssignment()
        {
            Type hostType = GetBlackboardEntryType();
            var assignment = new FieldAssignment
            {
                Value = new BlackboardVariable<TypedValue>()
            };
            
            if (hostType != null)
            {
                assignment.AvailableFields = GetEditableFieldNames(hostType).ToList();
                assignment.HostType = hostType;
            }

            // Pass blackboard keys to the variable
            assignment.Value.LocalKeys = Blackboard?.AvailableKeys ?? new List<string>();

            return assignment;
        }

        private IEnumerable<string> GetBlackboardKeys() => 
            Blackboard?.AvailableKeys ?? new List<string>();

        private IEnumerable<string> GetEditableFieldNames(Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(f => !f.IsLiteral && !f.IsInitOnly)
                .Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null)
                .Select(f => f.Name);
        }

        private Type GetBlackboardEntryType()
        {
            if (Blackboard == null || string.IsNullOrEmpty(BlackboardKey))
                return null;

            var entry = Blackboard.BlackboardVariables.FirstOrDefault(e => e.Key == BlackboardKey);
            return entry?.EntryType;
        }

        private bool ValidateType(Type targetType, object value)
        {
            if (value == null) 
                return !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null;
            
            return targetType.IsAssignableFrom(value.GetType());
        }
    }

    [Serializable, HideReferenceObjectPicker]
    public class FieldAssignment
    {
        [HorizontalGroup("Assignment", 0.3f)]
        [ValueDropdown(nameof(AvailableFields))]
        [HideLabel]
        [OnValueChanged(nameof(OnFieldSelected))]
        public string FieldName;

        [HorizontalGroup("Assignment")]
        [HideLabel]
        [OdinSerialize]
        [InlineProperty]
        public BlackboardVariable<TypedValue> Value = new BlackboardVariable<TypedValue>();

        [HideInInspector, NonSerialized]
        public List<string> AvailableFields = new List<string>();

        [HideInInspector, NonSerialized]
        public Type HostType;

        private void OnFieldSelected()
        {
            if (HostType == null || string.IsNullOrEmpty(FieldName))
            {
                if (Value?.DirectValue != null)
                {
                    Value.DirectValue.Type = null;
                }
                return;
            }

            FieldInfo field = HostType.GetField(
                FieldName, 
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic
            );

            if (field != null && Value?.DirectValue != null)
            {
                Value.DirectValue.Type = field.FieldType;
            }
        }
    }
}