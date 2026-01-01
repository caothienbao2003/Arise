using System;
using System.Collections.Generic;
using System.Linq;
using CTB;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace SceneSetupTool
{
    [Serializable]
    public class CreateNewScriptableObjectAction : SequenceAction
    {
        [SerializeField, HideInInspector] private string selectedTypeQualifiedName;

        // --- FILE CONFIG ---
        [InfoBox("@\"Full file name: \" + FullFileName + \".asset\"")]
        public BlackboardVariable<string> FileName = new BlackboardVariable<string>();

        public string FileNamePrefix;
        public string FileNameSuffix;

        [Title("Location")] [FolderPath] [Required]
        public string FolderPath = "Assets";

        public bool CreateFolder = false;
        [ShowIf(nameof(CreateFolder))] public BlackboardVariable<string> FolderName = new BlackboardVariable<string>();

        [Title("ScriptableObject Type")]
        [ShowInInspector]
        [ValueDropdown(nameof(GetScriptableObjectTypes))]
        [OnValueChanged(nameof(OnTypeChanged))]
        [LabelText("Target Type")]
        public Type SelectedType
        {
            get => string.IsNullOrEmpty(selectedTypeQualifiedName) ? null : Type.GetType(selectedTypeQualifiedName);
            set => selectedTypeQualifiedName = value?.AssemblyQualifiedName;
        }
        
        // --- PREVIEW & MODIFICATION ---
        [Title("Data Setup")]
        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.CompletelyHidden)]
        [ShowInInspector, LabelText("Configure Properties")]
        [OnInspectorInit(nameof(EnsurePreviewInstance))]
        public ScriptableObject PreviewInstance;
        private string FullFileName =>
            FileNamePrefix + FileName.GetValue(key => Blackboard.Get<string>(key)) + FileNameSuffix;

        public override void Execute()
        {
            if (SelectedType == null || PreviewInstance == null)
            {
                Debug.LogError("[CreateNewScriptableObjectAction] No Type or Data configured!");
                return;
            }

            string fileName = FullFileName;
            string folderName = FolderName.GetValue(key => Blackboard.Get<string>(key));
            string folderPath = CreateFolder ? $"{FolderPath}/{folderName}" : FolderPath;
            string fullPath = $"{folderPath}/{fileName}.asset";

            // Use the instance we modified in the inspector
            // We use AssetDatabaseUtils.CreateAsset (assuming it handles the save/refresh)
            AssetDatabaseUtils.CreateAsset(PreviewInstance, fullPath);

            Debug.Log($"<b>[Success]</b> Created {SelectedType.Name} asset with custom data at: {fullPath}");
            EditorGUIUtility.PingObject(PreviewInstance);
            
            // Note: After saving, PreviewInstance becomes a persistent asset. 
            // We null it out so the next time the action is viewed, it's fresh or handled.
            PreviewInstance = null;
        }

        private void OnTypeChanged()
        {
            AutoFillName();
            RefreshPreviewInstance();
        }

        private void EnsurePreviewInstance()
        {
            if (PreviewInstance == null && SelectedType != null)
            {
                RefreshPreviewInstance();
            }
        }

        private void RefreshPreviewInstance()
        {
            if (SelectedType != null)
            {
                // Creates a temporary instance in memory (not saved to disk yet)
                PreviewInstance = ScriptableObject.CreateInstance(SelectedType);
            }
            else
            {
                PreviewInstance = null;
            }
        }

        private IEnumerable<ValueDropdownItem<Type>> GetScriptableObjectTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => typeof(ScriptableObject).IsAssignableFrom(t)
                            && !t.IsAbstract
                            && !t.IsInterface
                            && !t.Name.Contains("Internal")
                            && (t.Namespace == null || (!t.Namespace.StartsWith("UnityEngine") &&
                                                        !t.Namespace.StartsWith("UnityEditor"))))
                .Select(t => new ValueDropdownItem<Type>(t.Namespace != null ? $"{t.Namespace}/{t.Name}" : t.Name, t));
        }

        private void AutoFillName()
        {
            if (SelectedType != null && FileName != null && string.IsNullOrEmpty(FileName.DirectValue))
            {
                FileName.DirectValue = SelectedType.Name;
            }
        }
    }
}