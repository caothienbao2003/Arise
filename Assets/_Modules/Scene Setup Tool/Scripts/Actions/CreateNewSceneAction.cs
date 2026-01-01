using CTB;
using Sirenix.OdinInspector;
using System;
using Sirenix.Serialization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Serialization;

namespace SceneSetupTool
{
    [Serializable]
    public class CreateNewSceneAction : SequenceAction
    {
        public bool CreateFromTemplate;

        [ShowIf(nameof(CreateFromTemplate))] [AssetsOnly]
        public SceneAsset TemplateScene;
        
        [InfoBox("@\"Full Name: \" + SceneFullName")]
        public BlackboardVariable<string> SceneName;

        [Space]
        
        public string SceneNamePrefix;
        public string SceneNameSuffix;
        
        private string SceneFullName =>
            SceneNamePrefix + SceneName.GetValue(key => Blackboard.Get<string>(key)) + SceneNameSuffix;

        [Title("Folder")] 
        [FolderPath] 
        [Required]
        public string FolderPath;

        public bool CreateNewFolder;
        
        [ShowIf(nameof(CreateNewFolder))]
        public BlackboardVariable<string> FolderName;

        [Title("Scene Setup")] public NewSceneMode NewSceneMode = NewSceneMode.Single;

        public NewSceneSetup NewSceneSetup = NewSceneSetup.DefaultGameObjects;

        [Title("Post created")] 
        public bool OpenSceneAfterCreated;
        
        public override void Execute()
        {
            if (!ValidateInputs()) return;

            string sceneName = SceneNamePrefix + SceneName.GetValue(key => Blackboard.Get<string>(key)) + SceneNameSuffix;
            
            string folderName = FolderName.GetValue(key => Blackboard.Get<string>(key));
            
            string folderPath = GetFolderPath(folderName);

            if (!SceneUtils.CheckSceneExisted(sceneName, folderPath)) return;

            if (CreateFromTemplate)
            {
                CreateSceneFromTemplate(folderPath);
            }
            else
            {
                CreateNewScene(sceneName, folderPath);
            }

            Debug.Log($"Scene '{sceneName}' created successfully at '{folderPath}'.");

            if (OpenSceneAfterCreated)
            {
                SceneUtils.OpenScene(sceneName, folderPath, OpenSceneMode.Additive);
            }
        }

        private string GetFolderPath(string folderName)
        {
            string folderPath;

            if (CreateNewFolder)
            {
                folderPath = $"{FolderPath}/{folderName}";
            }
            else
            {
                folderPath = FolderPath;
            }

            return folderPath;
        }

        // private string GetSceneName()
        // {
        //     string sceneName;
        //     if (GetNameFromBlackboard)
        //     {
        //         sceneName = Blackboard.Get<string>(SceneNameKey);
        //     }
        //     else
        //     {
        //         sceneName = SceneName;
        //     }
        //
        //     sceneName = SceneNamePrefix + sceneName + SceneNameSuffix;
        //     return sceneName;
        // }

        private void CreateSceneFromTemplate(string folderPath)
        {
            if (TemplateScene == null)
            {
                Debug.LogWarning("[CreateNewSceneAction] Template scene is null.");
                return;
            }

            AssetDatabaseUtils.CopyAsset(TemplateScene, folderPath);
        }

        private void CreateNewScene(string sceneName, string folderPath)
        {
            SceneUtils.CreateScene(sceneName, folderPath, NewSceneSetup, NewSceneMode);
            SceneUtils.PingScene(sceneName, folderPath);
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(FolderPath))
            {
                Debug.LogWarning("[CreateNewSceneAction] Folder path is empty.");
                return false;
            }

            return true;
        }
    }
}