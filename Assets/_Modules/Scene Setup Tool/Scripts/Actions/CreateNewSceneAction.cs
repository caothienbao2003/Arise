using CTB;
using Sirenix.OdinInspector;
using System;
using CTB.DesignPatterns.Blackboard;
using Sirenix.Serialization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace SceneSetupTool
{
    [Serializable]
    public class CreateNewSceneAction : SequenceAction
    {
        public bool CreateFromTemplate;

        [ShowIf(nameof(CreateFromTemplate))] [AssetsOnly]
        public SceneAsset TemplateScene;
        
        [InfoBox("@\"Full file name: \" + SceneFullName + \".asset\"")]
        public BlackboardVariable<string> SceneName = new();

        [Space]
        
        public string SceneNamePrefix;
        public string SceneNameSuffix;
        
        private string SceneFullName =>
            SceneNamePrefix + sceneName + SceneNameSuffix;

        private string sceneName => (SceneName != null && Blackboard!=null) ? SceneName.GetValue(key => Blackboard.Get<string>(key)): "";
        
        [Title("Folder")] 
        [FolderPath] 
        [Required]
        public string FolderPath;

        public bool CreateNewFolder;
        
        [ShowIf(nameof(CreateNewFolder))]
        public BlackboardVariable<string> FolderName = new();

        [Title("Scene Setup")] public NewSceneMode NewSceneMode = NewSceneMode.Single;

        public NewSceneSetup NewSceneSetup = NewSceneSetup.DefaultGameObjects;

        [Title("Post created")] 
        public bool OpenSceneAfterCreated;

        [Title("Output")]
        public BlackboardOutput SceneOutput = new();
        public override void Execute()
        {
            if (!ValidateInputs()) return;

            string sceneName = SceneNamePrefix + SceneName.GetValue(key => Blackboard.Get<string>(key)) + SceneNameSuffix;
            
            string folderName = FolderName.GetValue(key => Blackboard.Get<string>(key));
            
            string folderPath = GetFolderPath(folderName);
            
            string destinationPath = $"{folderPath}/{sceneName}.unity";

            if (!SceneUtils.CheckSceneExisted(sceneName, folderPath)) return;

            if (CreateFromTemplate)
            {
                CreateSceneFromTemplate(destinationPath);
            }
            else
            {
                CreateNewScene(sceneName, folderPath);
            }

            Debug.Log($"Scene '{sceneName}' created successfully at '{folderPath}'.");

            if (OpenSceneAfterCreated)
            {
                SceneUtils.OpenScene(sceneName, folderPath, OpenSceneMode.Single);
            }
            
            SceneAsset newScene = SceneUtils.GetSceneAsset(sceneName, folderPath);
            
            SceneOutput.TrySave(Blackboard, newScene);
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

        private void CreateSceneFromTemplate(string destinationPath)
        {
            if (TemplateScene == null)
            {
                Debug.LogWarning("[CreateNewSceneAction] Template scene is null.");
                return;
            }
            
            AssetDatabaseUtils.CopyAsset(TemplateScene, destinationPath);
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