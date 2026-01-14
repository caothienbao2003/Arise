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
    public class SetPropertiesAction : SequenceAction
    {
        [InlineProperty]
        [HideLabel]
        public BlackboardVariable<object> TargetObject = new BlackboardVariable<object>();

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

        private bool HasTarget => TargetObject != null && (!TargetObject.UseBlackboard || !string.IsNullOrEmpty(TargetObject.BlackboardKey));

        public override void Execute()
        {
            var host = TargetObject?.GetValue(key => Blackboard.Get<object>(key));
            if (host == null)
            {
                Debug.LogError($"[SetProperties] Target object not found");
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

            // Get the value from BlackboardVariable
            object valueToSet = assignment.Value?.GetValue(key => Blackboard.Get<object>(key));

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

        private void UpdateTargetTypeDisplay()
        {
            Type hostType = GetTargetType();
            targetTypeDisplay = hostType != null 
                ? $"Editing: {hostType.Name}" 
                : "No target selected";
        }

        private FieldAssignment AddFieldAssignment()
        {
            Type hostType = GetTargetType();
            var assignment = new FieldAssignment
            {
                Value = new BlackboardVariable<object>()
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

        private IEnumerable<string> GetEditableFieldNames(Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(f => !f.IsLiteral && !f.IsInitOnly)
                .Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null)
                .Select(f => f.Name);
        }

        private Type GetTargetType()
        {
            if (Blackboard == null || TargetObject == null)
                return null;

            if (TargetObject.UseBlackboard && !string.IsNullOrEmpty(TargetObject.BlackboardKey))
            {
                var entry = Blackboard.BlackboardVariables.FirstOrDefault(e => e.Key == TargetObject.BlackboardKey);
                return entry?.EntryType;
            }
            else if (!TargetObject.UseBlackboard && TargetObject.DirectValue != null)
            {
                return TargetObject.DirectValue.GetType();
            }

            return null;
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
        public BlackboardVariable<object> Value = new BlackboardVariable<object>();

        [HideInInspector, NonSerialized]
        public List<string> AvailableFields = new List<string>();

        [HideInInspector, NonSerialized]
        public Type HostType;

        private void OnFieldSelected()
        {
            if (HostType == null || string.IsNullOrEmpty(FieldName))
            {
                return;
            }

            FieldInfo field = HostType.GetField(
                FieldName, 
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic
            );

            if (field != null)
            {
                // We can log the field type for debugging if needed
                Debug.Log($"Selected field '{FieldName}' of type {field.FieldType.Name}");
            }
        }
    }
}