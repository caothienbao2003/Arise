using CTB;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace SceneSetupTool
{
    [Serializable]
    public class CreateGameObjectAction : SequenceAction
    {
        [Title("Basic Settings")]
        [SerializeField] public bool getNameFromBlackboard = false;
        
        [ShowIf(nameof(getNameFromBlackboard))]
        [ValueDropdown(nameof(AvailableKeys))]
        public string GameObjectNameKey;
        
        [HideIf(nameof(getNameFromBlackboard))]
        public string GameObjectName;
     
        [Title("Parent Settings")]
        public bool HasParent = false;
        [ShowIf(nameof(HasParent))]
        public string ParentObjectName = "";
        
        [Title("Prefab Settings")]
        public bool FromPrefab = false;
        [ShowIf(nameof(FromPrefab))]
        public GameObject Prefab;
        
        [Title("Output")]
        public bool SaveToBlackboard = true;
        [ValueDropdown(nameof(AvailableKeys))]
        [ShowIf(nameof(SaveToBlackboard))]
        public string OutputKey;
        
        public override void Execute()
        {
            GameObject newGO;

            string gameObjectName;

            if (getNameFromBlackboard)
            {
                gameObjectName = Blackboard.Get<string>(GameObjectNameKey);
            }
            else
            {
                gameObjectName = GameObjectName;
            }
            
            if (FromPrefab && Prefab != null)
            {
                newGO = GameObject.Instantiate(Prefab);
            }
            else
            {
                if (string.IsNullOrEmpty(gameObjectName))
                {
                    Debug.LogWarning("[CreateGameObjectAction] Failed: GameObjectName cannot be empty.");
                    return;
                }
                newGO = new GameObject(gameObjectName);
            }
            
            if(HasParent)
            {
                if (string.IsNullOrEmpty(ParentObjectName) == true)
                {
                    Debug.LogWarning("[CreateGameObjectAction] Failed: ParentObjectName cannot be empty.");
                    return;
                }
                GameObject parent = GameObject.Find(ParentObjectName);

                if (parent == null)
                {
                    parent = new GameObject(ParentObjectName);
                }
            
                newGO.transform.SetParent(parent.transform);
            }

            if (SaveToBlackboard && string.IsNullOrEmpty(OutputKey) == false)
            {
                Blackboard.Set(OutputKey, newGO);
            }
        }
    }
}