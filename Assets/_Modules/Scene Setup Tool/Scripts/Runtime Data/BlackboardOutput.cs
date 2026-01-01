using System;
using System.Collections.Generic;
using CTB;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SceneSetupTool
{
    [Serializable]
    [InlineProperty]
    [HideLabel]
    [Title("Blackboard output")]
    public class BlackboardOutput : IBlackboardInjectable
    {
        [LabelText(" Save?", SdfIconType.Save2Fill)]
        [GUIColor(0.8f, 0.8f, 1f)]
        public bool SaveToBlackboard;
        
        [LabelText(" Key", SdfIconType.KeyFill)]
        [GUIColor(0.8f, 0.8f, 1f)]
        [ShowIf(nameof(SaveToBlackboard))]
        [ValueDropdown(nameof(LocalKeys))]
        [Required(InfoMessageType.Error)]
        [HideLabel]
         // Subtle orange for the output key
        public string Key;

        public void TrySave(Blackboard bb, object value)
        {
            if (SaveToBlackboard && !string.IsNullOrEmpty(Key))
            {
                bb.Set(Key, value, true);
            }
        }

        [HideInInspector]
        public IEnumerable<string> LocalKeys { get; set; }
    }
}