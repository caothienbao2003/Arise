using CTB;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
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

            // Get all public and private fields in this action
            var fields = this.GetType().GetFields(System.Reflection.BindingFlags.Public | 
                                                  System.Reflection.BindingFlags.NonPublic | 
                                                  System.Reflection.BindingFlags.Instance);
            
            foreach (var f in fields)
            {
                // Check if the field implements our interface
                if (typeof(IBlackboardInjectable).IsAssignableFrom(f.FieldType))
                {
                    var injected = f.GetValue(this) as IBlackboardInjectable;
                    if (injected != null)
                    {
                        injected.LocalKeys = AvailableKeys;
                    }
                }
            }
        }
    }
}