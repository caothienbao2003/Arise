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
        public enum ComponentSource
        {
            BuiltIn,
            CustomScript
        }

        public BlackboardVariable<string> GameObjectName = new();

        [Title("Component Selection")] [EnumToggleButtons]
        public ComponentSource Source = ComponentSource.BuiltIn;

        [SerializeField, HideInInspector] private string _builtInTypeQualifiedName;

        [ShowInInspector] // Forces visibility
        [ShowIf(nameof(Source), ComponentSource.BuiltIn)]
        [ValueDropdown(nameof(GetBuiltInComponents))]
        [LabelText("Select Type")]
        public Type BuiltInType
        {
            get => string.IsNullOrEmpty(_builtInTypeQualifiedName) ? null : Type.GetType(_builtInTypeQualifiedName);
            set => _builtInTypeQualifiedName = value?.AssemblyQualifiedName;
        }

        [ShowIf(nameof(Source), ComponentSource.CustomScript)] [SerializeField, LabelText("Script File")]
        public MonoScript componentToAdd;

        public BlackboardOutput BlackboardOutput = new();

        public override void Execute()
        {
            string gameObjectName = GameObjectName.GetValue(key => Blackboard.Get<string>(key));
            
            if (string.IsNullOrEmpty(gameObjectName)) return;
            
            GameObject targetGo = GameObject.Find(gameObjectName);
            
            if (targetGo == null)  return;

            Type typeToRuntime = (Source == ComponentSource.BuiltIn) ? BuiltInType : componentToAdd.GetClass();

            if (typeToRuntime != null && typeof(Component).IsAssignableFrom(typeToRuntime))
            {
                var newComp = targetGo.AddComponent(typeToRuntime);
                BlackboardOutput.TrySave(Blackboard, newComp);
            }
        }

        private IEnumerable<Type> GetBuiltInComponents()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.Contains("UnityEngine") && !a.FullName.Contains("Editor"))
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(Component).IsAssignableFrom(t) && !t.IsAbstract && t.IsPublic);
        }
    }
}