using CTB;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace SceneSetupTool
{
    [CreateAssetMenu(fileName = "SetupSceneBehaviourSO", menuName = "Scriptable Objects/SetupSceneBehaviourSO")]
    public class SequenceActionsSetupSO : SerializedScriptableObject
    {
        [HideLabel]
        public Blackboard Blackboard = new Blackboard();
        
        [SerializeReference]
        [OnCollectionChanged(nameof(SyncActions))]
        public List<SequenceAction> Actions = new List<SequenceAction>();
        
        [Button(ButtonSizes.Large, ButtonStyle.Box), GUIColor(0.4f, 1f, 0.4f)]
        public void ExecuteActions()
        {
            SyncActions();
            
            // 1. Clean up from last run
            Blackboard.ClearRuntimeData();
            
            // 2. Execute
            foreach (var action in Actions)
            {
                action?.Execute();
            }
        }

        [OnInspectorGUI]
        private void SyncActions()
        {
            if (Actions == null) return;

            foreach (var action in Actions)
            {
                if (action != null)
                {
                    // Now we pass the logic/keys from the unified Entries list
                    action.AvailableKeys = Blackboard.GetAvailableKeys;
                    action.Blackboard = Blackboard;
                }
            }
        }
        
        protected override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            SyncActions();
        }
    }
}