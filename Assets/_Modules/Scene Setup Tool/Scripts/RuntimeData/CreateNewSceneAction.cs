using CTB;
using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SceneSetupTool
{
    [Serializable]
    public class CreateNewSceneAction : SequenceAction
    {
        [Title("Scene creation settings")]
        public bool createFromTemplate;

        [ShowIf(nameof(createFromTemplate))]
        [AssetsOnly]
        public SceneAsset templateScene;

        public bool getNameFromBlackboard = true;
        
        [ShowIf(nameof(getNameFromBlackboard))]
        [ValueDropdown(nameof(AvailableKeys))]
        public string sceneNameKey;
        
        [HideIf(nameof(getNameFromBlackboard))]
        [SerializeField]
        public string sceneName;

        [FolderPath]
        public string folderPath;

        public NewSceneMode newSceneMode = NewSceneMode.Single;

        public NewSceneSetup newSceneSetup = NewSceneSetup.DefaultGameObjects;

        [Title("Post-creation settings")]
        public bool openSceneAfterCreated;

        public bool saveToBlackboard;

        [ValueDropdown(nameof(AvailableKeys))]
        [ShowIf(nameof(saveToBlackboard))]
        public string outputKey;

        public override void Execute()
        {
            if(!ValidateInputs()) return;
            if (!SceneUtils.CheckSceneExisted(sceneName, folderPath)) return;

            if (createFromTemplate)
            {
                CreateSceneFromTemplate();
            }
            else
            {
                CreateNewScene();
            }

            Debug.Log($"Scene '{sceneName}' created successfully at '{folderPath}'.");
            
            if (openSceneAfterCreated)
            {
                SceneUtils.OpenScene(sceneName, folderPath, OpenSceneMode.Additive);
            }
        }
        private void CreateSceneFromTemplate()
        {
            if (templateScene == null)
            {
                Debug.LogWarning("[CreateNewSceneAction] Template scene is null.");
                return;
            }

            AssetDatabaseUtils.CopyAsset(templateScene, folderPath);
        }
        private void CreateNewScene()
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("[CreateNewSceneAction] Scene name is empty.]");
                return;
            }

            SceneUtils.CreateScene(sceneName, folderPath, newSceneSetup, newSceneMode);
            SceneUtils.PingScene(sceneName, folderPath);
        }
        
        private bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("[CreateNewSceneAction] Scene name is empty.");
                return false;
            }

            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogWarning("[CreateNewSceneAction] Folder path is empty.");
                return false;
            }
            return true;
        }
    }
}