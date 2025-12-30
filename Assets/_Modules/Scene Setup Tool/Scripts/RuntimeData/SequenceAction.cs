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
    }
}