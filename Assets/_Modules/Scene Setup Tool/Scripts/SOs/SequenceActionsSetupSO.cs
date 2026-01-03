using System;
using CTB;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

namespace SceneSetupTool
{
    [CreateAssetMenu(fileName = "SequenceActionsSetupSO", menuName = "Scriptable Objects/Sequence Actions Setup")]
    public class SequenceActionsSetupSO : SerializedScriptableObject
    {
        [HideLabel]
        [NonSerialized]
        [OdinSerialize]
        public Blackboard Blackboard = new Blackboard();
        
        [Space]
        
        [Searchable]
        [NonSerialized]
        [OdinSerialize]
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
        }
        
        [OnInspectorGUI]
        private void SyncActions()  
        {
            if (Actions == null) return;
            
            // Sync keys to variables
            // Blackboard.SyncKeys();

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