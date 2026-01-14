using System;
using CTB;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;
using System.Linq;
using System.Text;
using CTB.DesignPatterns.Blackboard;
using GridTool;

namespace SceneSetupTool
{
    [CreateAssetMenu(fileName = "SequenceActionsSetupSO", menuName = "Scriptable Objects/Sequence Actions Setup")]
    public class SequenceActionsSetupSO : SerializedScriptableObject, IDisplayNameable
    {
        public string DisplayName => displayName;
        [SerializeField] 
        private string displayName;
        
        [FoldoutGroup("Summary", Expanded = true)]
        [HideLabel]
        [ReadOnly]
        [TextArea(minLines: 3, maxLines: 20)]
        [ShowInInspector]
        [PropertyOrder(-1)]
        [InfoBox("@GetSummaryStats()", InfoMessageType.Info)]
        private string DescriptionSummary => GenerateDescriptionSummary();

        [FoldoutGroup("Blackboard")]
        [HideLabel]
        [NonSerialized]
        [OdinSerialize]
        [InlineProperty]
        public Blackboard Blackboard = new Blackboard();
        
        [Space]
        
        [FoldoutGroup("Actions")]
        [Searchable]
        [NonSerialized]
        [OdinSerialize]
        [ListDrawerSettings(DraggableItems = true, ShowFoldout = true, ShowPaging = false, HideAddButton = false)]
        [SerializeReference]
        public List<SequenceAction> Actions = new List<SequenceAction>();
        
        [Button(ButtonSizes.Large, ButtonStyle.Box), GUIColor(0.4f, 1f, 0.4f)]
        public void ExecuteActions()
        {
            SyncActions();
            
            foreach (var action in Actions)
            {
                if (action != null)
                {
                    action.Blackboard = this.Blackboard;
                    action.Execute();
                }
            }
            
            Blackboard.ClearRuntimeData();
        }
        
        [OnInspectorGUI]
        private void SyncActions()  
        {
            if (Actions == null) return;

            foreach (var action in Actions)
            {
                if (action != null)
                {
                    action.AvailableKeys = Blackboard.AvailableKeys;
                    action.Blackboard = Blackboard;
                }
            }
        }

        private string GetSummaryStats()
        {
            if (Actions == null || Actions.Count == 0)
                return "No actions";
            
            int totalActions = Actions.Count;
            int describedActions = Actions.Count(a => a != null && !string.IsNullOrWhiteSpace(a.GetDescription()));
            int undescribedActions = totalActions - describedActions;
            
            return $"Total: {totalActions} actions | Described: {describedActions} | Missing descriptions: {undescribedActions}";
        }

        private string GenerateDescriptionSummary()
        {
            if (Actions == null || Actions.Count == 0)
            {
                return "No actions defined.";
            }

            var summary = new StringBuilder();

            for (int i = 0; i < Actions.Count; i++)
            {
                var action = Actions[i];
                if (action == null)
                {
                    summary.AppendLine($"{i + 1}. [NULL ACTION]");
                    continue;
                }

                string description = action.GetDescription();
                
                string typeName = action.GetType().Name;
                
                if (typeName.EndsWith("Action"))
                {
                    typeName = typeName.Substring(0, typeName.Length - 6);
                }
                
                if (string.IsNullOrWhiteSpace(description))
                {
                    // Show type name with better formatting
                    
                    summary.AppendLine($"{i + 1}. [{typeName}] - (No description)");
                }
                else
                {
                    // Clean up the description (remove extra whitespace, newlines)
                    string cleanDescription = description.Trim().Replace("\n", " ").Replace("\r", "");
                    summary.AppendLine($"{i + 1}. [{typeName}] - {cleanDescription}");
                }
            }

            return summary.ToString().TrimEnd();
        }
    }
}