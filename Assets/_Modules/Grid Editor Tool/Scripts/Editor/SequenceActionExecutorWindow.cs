using SceneSetupTool;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace GridTool
{
    public class SequenceActionExecutorWindow
    {
        [SerializeField] 
        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Foldout)]
        [ListDrawerSettings(ShowFoldout = true)]
        private List<SequenceActionsSetupSO> actions;
        
        [Button(ButtonSizes.Large, ButtonStyle.Box), GUIColor(0.6f, 1f, 0.4f)]
        public void ExecuteAllActions()
        {
            foreach (SequenceActionsSetupSO action in actions)
            {
                action.ExecuteActions();
            }
        }
    }
}