using CTB;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace SceneSetupTool
{
    [Serializable]
    public class CreateGameObjectAction : SequenceAction
    {
        public BlackboardVariable<string> GameObjectName = new();
        
        [Title("Parent Settings")]
        public bool HasParent = false;

        [ShowIf(nameof(HasParent))] 
        public BlackboardVariable<string> ParentObjectName;
        
        [Title("Prefab Settings")]
        public bool FromPrefab = false;
        [ShowIf(nameof(FromPrefab))]
        public GameObject Prefab;
        
        public BlackboardOutput GameObjectOutput = new();
        
        public override void Execute()
        {
            GameObject newGO;

            string gameObjectName = GameObjectName.GetValue(key => Blackboard.Get<string>(key));

            string parentObjectName = ParentObjectName.GetValue(key => Blackboard.Get<string>(key));
            // if (GetNameFromBlackboard)
            // {
            //     gameObjectName = Blackboard.Get<string>(GameObjectNameKey);
            // }
            // else
            // {
            //     gameObjectName = GameObjectName;
            // }
            
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
                if (string.IsNullOrEmpty(parentObjectName) == true)
                {
                    Debug.LogWarning("[CreateGameObjectAction] Failed: ParentObjectName cannot be empty.");
                    return;
                }
                GameObject parent = GameObject.Find(parentObjectName);

                if (parent == null)
                {
                    parent = new GameObject(parentObjectName);
                }
            
                newGO.transform.SetParent(parent.transform);
            }

            GameObjectOutput.TrySave(Blackboard, newGO);
            
            // if (SaveToBlackboard && string.IsNullOrEmpty(OutputKey) == false)
            // {
            //     Blackboard.Set(OutputKey, newGO);
            // }
        }
    }
}