using CTB;
using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace SceneSetupTool
{
    [Serializable]
    public class AddComponentAction : SequenceAction
    {
        public enum ComponentSource { BuiltIn, CustomScript }

        [Title("Target Settings")]
        public bool UseGameObjectInBlackboard = true;
        
        [HideIf(nameof(UseGameObjectInBlackboard))]
        public string TargetGameObjectName;
        
        [ShowIf(nameof(UseGameObjectInBlackboard))]
        [ValueDropdown(nameof(AvailableKeys))]
        public string TargetKey;

        [Title("Component Selection")]
        [EnumToggleButtons]
        public ComponentSource Source = ComponentSource.BuiltIn;

        [SerializeField, HideInInspector] 
        private string _builtInTypeQualifiedName;
        
        [ShowInInspector] // Forces visibility
        [ShowIf(nameof(Source), ComponentSource.BuiltIn)]
        [ValueDropdown(nameof(GetBuiltInComponents))]
        [LabelText("Select Type")]
        public Type BuiltInType
        {
            get => string.IsNullOrEmpty(_builtInTypeQualifiedName) ? null : Type.GetType(_builtInTypeQualifiedName);
            set => _builtInTypeQualifiedName = value?.AssemblyQualifiedName;
        }
        
        [ShowIf(nameof(Source), ComponentSource.CustomScript)]
        [SerializeField, LabelText("Script File")]
        public MonoScript componentToAdd;

        [Title("Output")]
        public bool SaveToBlackboard = false;
        
        [ShowIf(nameof(SaveToBlackboard))]
        [ValueDropdown(nameof(AvailableKeys))]
        public string OutputKey;
        

        public override void Execute()
        {
            GameObject targetGo = UseGameObjectInBlackboard 
                ? GetGameObjectFromBlackboard(Blackboard) 
                : GetGameObjectByName();

            if (targetGo == null) return;

            Type typeToRuntime = (Source == ComponentSource.BuiltIn) ? BuiltInType : componentToAdd.GetClass();

            if (typeToRuntime != null && typeof(Component).IsAssignableFrom(typeToRuntime))
            {
                var newComp = targetGo.AddComponent(typeToRuntime);
                if (SaveToBlackboard && !string.IsNullOrEmpty(OutputKey))
                {
                    Blackboard.Set(OutputKey, newComp);
                }
            }
        }

        private IEnumerable<Type> GetBuiltInComponents()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.Contains("UnityEngine") && !a.FullName.Contains("Editor"))
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(Component).IsAssignableFrom(t) && !t.IsAbstract && t.IsPublic);
        }

        private GameObject GetGameObjectFromBlackboard(Blackboard blackboard) => string.IsNullOrEmpty(TargetKey) ? null : blackboard.Get<GameObject>(TargetKey);
        private GameObject GetGameObjectByName() => string.IsNullOrEmpty(TargetGameObjectName) ? null : GameObject.Find(TargetGameObjectName);
    }
}