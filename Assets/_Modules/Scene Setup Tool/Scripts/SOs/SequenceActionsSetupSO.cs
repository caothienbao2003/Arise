using CTB;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace SceneSetupTool
{
    [CreateAssetMenu(fileName = "SequenceActionsSetupSO", menuName = "Scriptable Objects/Sequence Actions Setup")]
    public class SequenceActionsSetupSO : ScriptableObject
    {
        [HideLabel]
        [SerializeField]
        public Blackboard Blackboard = new Blackboard();
        
        [Space]
        
        [Searchable]
        [ListDrawerSettings(DraggableItems = true, ShowFoldout = true, ShowPaging = false, HideAddButton = false)]
        [SerializeReference]
        public List<SequenceAction> Actions = new List<SequenceAction>();
        
        [Button(ButtonSizes.Large, ButtonStyle.Box), GUIColor(0.4f, 1f, 0.4f)]
        public void ExecuteActions()
        {
            // Ensure data is synced before run
            SyncActions();
            
            // Clean up non-persistent data
            
            foreach (var action in Actions)
            {
                if (action != null)
                {
                    action.Blackboard = this.Blackboard; // Pass the actual instance
                    action.Execute();
                }
            }
            
            Blackboard.ClearRuntimeData();
        }
        
        [OnInspectorGUI]
        private void SyncActions()  
        {
            if (Actions == null) return;
            
            // Sync keys to variables
            Blackboard.SyncKeys();

            foreach (var action in Actions)
            {
                if (action != null)
                {
                    // Update the injection list
                    action.AvailableKeys = Blackboard.AvailableKeys;
                    action.Blackboard = Blackboard;
                }
            }
        }
    }
}