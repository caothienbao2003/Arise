using CTB;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CTB.DesignPatterns.Blackboard;
using UnityEngine;

namespace SceneSetupTool
{
    [Serializable]
    public abstract class SequenceAction
    {
        [BoxGroup("Description")]
        [TextArea(minLines: 1, maxLines: 10)] 
        [SerializeField]
        [HideLabel]
        [OnValueChanged(nameof(OnDescriptionChanged))]
        private string description;

        public Blackboard Blackboard { get; set; }

        [HideInInspector, NonSerialized] 
        public IEnumerable<string> AvailableKeys = new List<string>();

        // Public accessor for description
        public string GetDescription() => description;

        [Button(ButtonSizes.Medium, ButtonStyle.Box), GUIColor(0.6f, 1f, 0.4f)]
        public abstract void Execute();

        [OnInspectorGUI]
        private void InjectKeys()
        {
            if (AvailableKeys == null) return;

            InjectKeysRecursive(this);
        }

        // Callback when description changes (forces repaint of parent inspector)
        private void OnDescriptionChanged()
        {
#if UNITY_EDITOR
            // This triggers the parent ScriptableObject to refresh its inspector
            // UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void InjectKeysRecursive(object obj, HashSet<object> visited = null)
        {
            if (obj == null) return;

            if (visited == null)
                visited = new HashSet<object>();

            if (!visited.Add(obj))
                return;

            Type type = obj.GetType();

            if (type.IsPrimitive || type == typeof(string))
                return;

            if (obj is IBlackboardInjectable injectable)
            {
                injectable.LocalKeys = AvailableKeys;
            }

            if (obj is IEnumerable enumerable && !(obj is string))
            {
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        InjectKeysRecursive(item, visited);
                    }
                }

                return;
            }

            FieldInfo[] fields = type.GetFields(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance
            );

            foreach (var field in fields)
            {
                if (field.IsNotSerialized)
                    continue;

                object fieldValue = field.GetValue(obj);
                if (fieldValue != null)
                {
                    InjectKeysRecursive(fieldValue, visited);
                }
            }
        }
    }
}