using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SceneSetupTool
{
    [Serializable]
    [InlineProperty]
    [HideLabel]
    [GUIColor(0.8f, 1f, 0.8f)]
    [Title("@$property.Name")]
    public class BlackboardVariable<T> : IBlackboardInjectable
    {
        [LabelText(" Load?", SdfIconType.CloudDownloadFill)]
        [Tooltip("Toggle: Direct Value (Off) vs Blackboard Key (On)")]
        public bool UseBlackboard;

        [LabelText(" Value", SdfIconType.PenFill)]
        [HideIf(nameof(UseBlackboard))]
        [Required(InfoMessageType.None)] 
        public T DirectValue;

        [LabelText(" Key", SdfIconType.KeyFill)]
        [ShowIf(nameof(UseBlackboard))]
        [ValueDropdown(nameof(LocalKeys))]
        public string BlackboardKey;

        public T GetValue(Func<string, T> resolver) => UseBlackboard ? resolver(BlackboardKey) : DirectValue;
        
        [HideInInspector] 
        public IEnumerable<string> LocalKeys {get; set;}
    }
}