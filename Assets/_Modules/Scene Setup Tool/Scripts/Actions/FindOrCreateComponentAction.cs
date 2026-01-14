using CTB;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using CTB.DesignPatterns.Blackboard;
using UnityEngine;

namespace SceneSetupTool
{
    [Serializable]
    public class FindOrCreateComponentAction : SequenceAction
    {
        [Title("Target GameObject")]
        public BlackboardVariable<string> GameObjectName = new();

        [Title("Component Type")]
        [HideLabel]
        [OdinSerialize]
        public TypedType ComponentType = new TypedType(
            typeof(Component),
            TypeFilter.ComponentOnly |
            TypeFilter.NonAbstract |
            TypeFilter.NonGeneric
        );

        public BlackboardOutput ComponentOutput = new();

        public override void Execute()
        {
            // Resolve GameObject name from blackboard
            string gameObjectName = GameObjectName?.GetValue(key => Blackboard.Get<string>(key));

            if (string.IsNullOrEmpty(gameObjectName))
            {
                Debug.LogWarning("[FindOrCreateComponentAction] Failed: GameObjectName cannot be empty.");
                return;
            }

            // Resolve component type
            Type type = ComponentType?.Type;

            if (type == null)
            {
                Debug.LogWarning("[FindOrCreateComponentAction] Failed: ComponentType is null.");
                return;
            }

            if (!typeof(Component).IsAssignableFrom(type))
            {
                Debug.LogWarning(
                    $"[FindOrCreateComponentAction] Failed: {type.Name} is not a Component."
                );
                return;
            }

            // Call GameObjectUtils.FindOrCreateComponent<T>()
            var method = typeof(GameObjectUtils)
                .GetMethod(nameof(GameObjectUtils.FindOrCreateComponent))
                ?.MakeGenericMethod(type);

            if (method == null)
            {
                Debug.LogWarning(
                    "[FindOrCreateComponentAction] Failed: Could not resolve FindOrCreateComponent."
                );
                return;
            }

            Component component = method.Invoke(
                null,
                new object[] { gameObjectName }
            ) as Component;

            if (component == null)
            {
                Debug.LogWarning(
                    "[FindOrCreateComponentAction] Failed: Component creation failed."
                );
                return;
            }

            // Save to blackboard
            ComponentOutput.TrySave(Blackboard, component);
        }
    }
}
