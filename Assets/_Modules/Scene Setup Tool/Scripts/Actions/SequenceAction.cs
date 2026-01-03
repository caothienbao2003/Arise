using CTB;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SceneSetupTool
{
    [Serializable]
    public abstract class SequenceAction
    {
        public Blackboard Blackboard { get; set;}
        
        [HideInInspector, NonSerialized]
        public IEnumerable<string> AvailableKeys = new List<string>();
        
        [Button(ButtonSizes.Medium, ButtonStyle.Box), GUIColor(0.6f, 1f, 0.4f)]
        public abstract void Execute();
        
        [OnInspectorGUI]
        private void InjectKeys()
        {
            if (AvailableKeys == null) return;

            InjectKeysRecursive(this);
        }

        private void InjectKeysRecursive(object obj, HashSet<object> visited = null)
        {
            if (obj == null) return;

            // Prevent infinite loops from circular references
            if (visited == null)
                visited = new HashSet<object>();

            // Skip if already visited (prevents infinite recursion)
            if (!visited.Add(obj))
                return;

            Type type = obj.GetType();

            // Skip primitive types and strings
            if (type.IsPrimitive || type == typeof(string))
                return;

            // If this object implements IBlackboardInjectable, inject keys
            if (obj is IBlackboardInjectable injectable)
            {
                injectable.LocalKeys = AvailableKeys;
            }

            // Handle collections (List, Array, etc.)
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

            // Get all fields (public, private, instance)
            FieldInfo[] fields = type.GetFields(
                BindingFlags.Public | 
                BindingFlags.NonPublic | 
                BindingFlags.Instance
            );

            foreach (var field in fields)
            {
                // Skip fields marked with [NonSerialized]
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