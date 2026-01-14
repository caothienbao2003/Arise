using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CTB.DesignPatterns.Blackboard
{
    [Serializable, InlineProperty, HideReferenceObjectPicker]
    public class TypedType
    {
        [HideInInspector] [OdinSerialize] private Type selectedType;

        [HideInInspector] [OdinSerialize] private TypeFilter typeFilters = TypeFilter.None;

        [HideInInspector] [OdinSerialize] private Type baseTypeConstraint;

        public Type Type
        {
            get => selectedType;
            set => selectedType = value;
        }

        public TypedType()
        {
        }

        public TypedType(Type baseType, TypeFilter filters = TypeFilter.None)
        {
            baseTypeConstraint = baseType;
            typeFilters = filters;
        }

#if UNITY_EDITOR

        [ShowInInspector, HideLabel]
        [ValueDropdown(nameof(GetValidTypes))]
        private Type TypePicker
        {
            get => selectedType;
            set => selectedType = value;
        }

        private IEnumerable<Type> GetValidTypes()
        {
            return GetAllTypes()
                .Where(PassesFilter);
        }

        private static IEnumerable<Type> GetAllTypes()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a =>
                {
                    try
                    {
                        return a.GetTypes();
                    }
                    catch
                    {
                        return Type.EmptyTypes;
                    }
                });
        }

        private bool PassesFilter(Type type)
        {
            if (type == null) return false;

            if (baseTypeConstraint != null && !baseTypeConstraint.IsAssignableFrom(type))
                return false;

            if (typeFilters.HasFlag(TypeFilter.NonAbstract) && type.IsAbstract)
                return false;

            if (typeFilters.HasFlag(TypeFilter.NonGeneric) && type.IsGenericTypeDefinition)
                return false;

            if (typeFilters.HasFlag(TypeFilter.ComponentOnly) &&
                !typeof(Component).IsAssignableFrom(type))
                return false;

            if (typeFilters.HasFlag(TypeFilter.MonoBehaviourOnly) &&
                !typeof(MonoBehaviour).IsAssignableFrom(type))
                return false;

            if (typeFilters.HasFlag(TypeFilter.ScriptableObjectOnly) &&
                !typeof(ScriptableObject).IsAssignableFrom(type))
                return false;

            return true;
        }

#endif
    }
}