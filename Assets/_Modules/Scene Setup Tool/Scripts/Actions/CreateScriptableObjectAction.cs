using System;
using System.Collections.Generic;
using System.Linq;
using CTB;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;

namespace SceneSetupTool
{
    [Serializable]
    public class CreateNewScriptableObjectAction : SequenceAction
    {
        [OdinSerialize]
        [BoxGroup("Select Type")]
        [InfoBox("@\"Selected: \" + (SelectedType != null ? SelectedType.Name : \"None\")", InfoMessageType.Info)]
        [Title("ScriptableObject Selection")]
        [LabelText("Target Type")]
        [Required(InfoMessageType.Error)]
// This ensures Odin's drawer is used instead of Unity's default text field
        [TypeDrawerSettings(BaseType = typeof(ScriptableObject))]
        [TypeSelectorSettings(
            FilterTypesFunction = nameof(IsProjectSO),
            ShowCategories = true,
            PreferNamespaces = true)]
        [ShowInInspector]
        public Type SelectedType; // No longer needs a hidden string backing!
        
        [InfoBox("@\"Full file name: \" + FullFileName + \".asset\"")]
        [BoxGroup("File Configuration")]
        [InlineProperty, HideLabel]
        public BlackboardVariable<string> FileName = new BlackboardVariable<string>();

        [BoxGroup("File Configuration")]
        public string FileNamePrefix;

        [BoxGroup("File Configuration")]
        public string FileNameSuffix;

        [BoxGroup("Folder")] [FolderPath] [Required]
        public string FolderPath = "Assets";

        [BoxGroup("Folder")] public bool CreateFolder = false;

        [BoxGroup("Folder")]
        [ShowIf(nameof(CreateFolder))] [InlineProperty]
        public BlackboardVariable<string> FolderName = new BlackboardVariable<string>();

        [BoxGroup("Blackboard Output")]
        public BlackboardOutput OutputAsset;

        private string FullFileName =>
            FileNamePrefix + FileName.GetValue(key => Blackboard.Get<string>(key)) + FileNameSuffix;

        public override void Execute()
        {
            if (SelectedType == null)
            {
                Debug.LogError("[CreateNewScriptableObjectAction] No Type selected!");
                return;
            }

            string folderName = FolderName.GetValue(key => Blackboard.Get<string>(key));
            
            string folderPath;

            if (CreateFolder)
            {
                folderPath = $"{FolderPath}/{folderName}";
            }
            else
            {
                folderPath = $"{FolderPath}";
            }
            
            string assetPath = $"{folderPath}/{FullFileName}.asset";

            if (!AssetDatabaseUtils.CheckFileExisted(assetPath))
            {
                return;
            }
            
            ScriptableObject instance = ScriptableObject.CreateInstance(SelectedType);
            AssetDatabaseUtils.CreateAsset(instance, assetPath);

            OutputAsset.TrySave(Blackboard, instance);

            Debug.Log($"<b>[Success]</b> Created {instance.name} of type {SelectedType.Name} at: {assetPath}");
            EditorGUIUtility.PingObject(instance);
        }
        
        private bool IsProjectSO(Type t) => !t.IsAbstract && t.Assembly.GetName().Name == "Assembly-CSharp";
    }
}