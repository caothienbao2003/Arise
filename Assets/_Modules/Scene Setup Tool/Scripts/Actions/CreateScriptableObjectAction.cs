using System;
using CTB;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace SceneSetupTool
{
    [Serializable]
    public class CreateNewScriptableObjectAction : SequenceAction
    {
        [InfoBox("@\"Full file name: \" + FullFileName + \".asset\"")]
        [BoxGroup("File Configuration")]
        [InlineProperty, HideLabel]
        public BlackboardVariable<string> FileName = new BlackboardVariable<string>();

        private string fileName => (FileName != null && Blackboard != null) 
            ? FileName.GetValue(key => Blackboard.Get<string>(key)) 
            : "";

        [BoxGroup("File Configuration")]
        public string FileNamePrefix;

        [BoxGroup("File Configuration")]
        public string FileNameSuffix;

        [BoxGroup("Folder")] 
        [FolderPath] 
        [Required]
        public string FolderPath = "Assets";

        [BoxGroup("Folder")] 
        public bool CreateFolder = false;

        [BoxGroup("Folder")]
        [ShowIf(nameof(CreateFolder))] 
        [InlineProperty]
        public BlackboardVariable<string> FolderName = new BlackboardVariable<string>();

        [BoxGroup("Type Selection")]
        [InlineProperty]
        [HideLabel]
        public TypeSelector TypeSelection = new TypeSelector(
            TypeFilter.ScriptableObjectOnly | 
            TypeFilter.ProjectScriptsOnly | 
            TypeFilter.NonAbstract,
            withValueField: false
        );

        [BoxGroup("Blackboard Output")]
        public BlackboardOutput OutputAsset = new();

        private string FullFileName => FileNamePrefix + fileName + FileNameSuffix;
        
        public override void Execute()
        {
            if (TypeSelection.SelectedType == null)
            {
                Debug.LogError("[CreateNewScriptableObjectAction] No Type selected!");
                return;
            }

            if (!typeof(ScriptableObject).IsAssignableFrom(TypeSelection.SelectedType))
            {
                Debug.LogError($"[CreateNewScriptableObjectAction] {TypeSelection.SelectedType.Name} is not a ScriptableObject!");
                return;
            }

            string folderName = FolderName?.GetValue(key => Blackboard.Get<string>(key)) ?? "";
            
            string folderPath = CreateFolder 
                ? $"{FolderPath}/{folderName}" 
                : FolderPath;
            
            string assetPath = $"{folderPath}/{FullFileName}.asset";

            if (!AssetDatabaseUtils.CheckFileExisted(assetPath))
            {
                return;
            }
            
            ScriptableObject instance = ScriptableObject.CreateInstance(TypeSelection.SelectedType);
            AssetDatabaseUtils.CreateAsset(instance, assetPath);

            OutputAsset.TrySave(Blackboard, instance);

            Debug.Log($"<b>[Success]</b> Created {instance.name} of type {TypeSelection.SelectedType.Name} at: {assetPath}");
            EditorGUIUtility.PingObject(instance);
        }
    }
}