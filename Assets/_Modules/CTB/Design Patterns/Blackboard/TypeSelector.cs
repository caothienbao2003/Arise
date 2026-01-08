using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace SceneSetupTool
{
    [Serializable, InlineProperty, HideReferenceObjectPicker]
    public class TypeSelector
    {
        [SerializeField, HideInInspector, OdinSerialize]
        private TypeFilter typeFilters = TypeFilter.None;

        [SerializeField, HideInInspector, OdinSerialize]
        private Type baseTypeConstraint;

        [SerializeField, HideInInspector, OdinSerialize]
        private List<string> namespaceFilters = new List<string>();

        [SerializeField, HideInInspector, OdinSerialize]
        private List<string> excludedNamespaces = new List<string>();

        [SerializeField, HideInInspector, OdinSerialize]
        private Func<Type, bool> customFilter;

        [SerializeField, HideInInspector, OdinSerialize]
        private bool showValueField = false;

        [SerializeField, HideInInspector, OdinSerialize]
        private bool useHorizontalLayout = true;

        [SerializeField, HideInInspector, OdinSerialize]
        private bool includePrimitives = true;

        [HorizontalGroup("Type", VisibleIf = nameof(useHorizontalLayout))]
        [HideLabel]
        [ValueDropdown(nameof(GetFilteredTypes), NumberOfItemsBeforeEnablingSearch = 10)]
        [OnValueChanged(nameof(OnTypeChanged))]
        [ShowInInspector]
        [OdinSerialize]
        [HideIf(nameof(ShouldHideTypeField))]
        public Type SelectedType;

        [HorizontalGroup("Type", VisibleIf = nameof(useHorizontalLayout))]
        [HideLabel]
        [OdinSerialize]
        [InlineProperty]
        [HideReferenceObjectPicker]
        [ShowInInspector]
        [ShowIf(nameof(ShouldShowValueField))]
        public TypedValue TypedValue = new TypedValue();

        private bool ShouldShowValueField => showValueField && SelectedType != null && useHorizontalLayout;
        private bool ShouldHideTypeField => !useHorizontalLayout;

        public TypeSelector()
        {
            showValueField = false;
            useHorizontalLayout = true;
            includePrimitives = true;
        }

        public TypeSelector(TypeFilter filters, bool withValueField = false, bool horizontal = true)
        {
            typeFilters = filters;
            showValueField = withValueField;
            useHorizontalLayout = horizontal;
            includePrimitives = true;
        }

        public TypeSelector(Type baseType, TypeFilter filters = TypeFilter.None, bool withValueField = false, bool horizontal = true)
        {
            baseTypeConstraint = baseType;
            typeFilters = filters;
            showValueField = withValueField;
            useHorizontalLayout = horizontal;
            includePrimitives = true;
        }

        public TypeSelector SetFilters(TypeFilter filters)
        {
            typeFilters = filters;
            return this;
        }

        public TypeSelector AddFilter(TypeFilter filter)
        {
            typeFilters |= filter;
            return this;
        }

        public TypeSelector RemoveFilter(TypeFilter filter)
        {
            typeFilters &= ~filter;
            return this;
        }

        public TypeSelector SetBaseType(Type baseType)
        {
            baseTypeConstraint = baseType;
            return this;
        }

        public TypeSelector ShowValueField(bool show = true)
        {
            showValueField = show;
            return this;
        }

        public TypeSelector UseHorizontalLayout(bool horizontal = true)
        {
            useHorizontalLayout = horizontal;
            return this;
        }

        public TypeSelector IncludePrimitives(bool include = true)
        {
            includePrimitives = include;
            return this;
        }

        public TypeSelector AddNamespaceFilter(params string[] namespaces)
        {
            namespaceFilters.AddRange(namespaces);
            return this;
        }

        public TypeSelector ExcludeNamespaces(params string[] namespaces)
        {
            excludedNamespaces.AddRange(namespaces);
            return this;
        }

        public TypeSelector SetCustomFilter(Func<Type, bool> filter)
        {
            customFilter = filter;
            return this;
        }

        public void EnsureTypedValueInitialized()
        {
            if (TypedValue == null)
            {
                TypedValue = new TypedValue();
                if (SelectedType != null)
                {
                    TypedValue.Type = SelectedType;
                }
            }
        }

        public object Value
        {
            get
            {
                EnsureTypedValueInitialized();
                return TypedValue.Value;
            }
            set
            {
                EnsureTypedValueInitialized();
                TypedValue.Value = value;
            }
        }

        private void OnTypeChanged()
        {
            EnsureTypedValueInitialized();
            TypedValue.Type = SelectedType;
        }

        private IEnumerable<ValueDropdownItem<Type>> GetFilteredTypes()
        {
            var allTypes = new List<ValueDropdownItem<Type>>();

            // Add primitives if enabled
            if (includePrimitives)
            {
                allTypes.AddRange(new[]
                {
                    new ValueDropdownItem<Type>("Primitives/String", typeof(string)),
                    new ValueDropdownItem<Type>("Primitives/Int", typeof(int)),
                    new ValueDropdownItem<Type>("Primitives/Float", typeof(float)),
                    new ValueDropdownItem<Type>("Primitives/Bool", typeof(bool)),
                    new ValueDropdownItem<Type>("Primitives/Double", typeof(double)),
                    new ValueDropdownItem<Type>("Primitives/Long", typeof(long)),
                    new ValueDropdownItem<Type>("Primitives/Byte", typeof(byte)),
                    new ValueDropdownItem<Type>("Primitives/Short", typeof(short)),
                });
            }

#if UNITY_EDITOR
            // Add editor types
            allTypes.Add(new ValueDropdownItem<Type>("Unity/Editor/SceneAsset", typeof(UnityEditor.SceneAsset)));
            allTypes.Add(new ValueDropdownItem<Type>("Unity/Editor/MonoScript", typeof(UnityEditor.MonoScript)));
            allTypes.Add(new ValueDropdownItem<Type>("Unity/Editor/DefaultAsset", typeof(UnityEditor.DefaultAsset)));
#endif

            // Get relevant assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => 
                {
                    var name = a.GetName().Name;
                    return name.Contains("Assembly-CSharp") || name.Contains("UnityEngine");
                });

            var assemblyTypes = assemblies
                .SelectMany(assembly => 
                {
                    try 
                    { 
                        return assembly.GetTypes(); 
                    }
                    catch 
                    { 
                        return Enumerable.Empty<Type>(); 
                    }
                })
                .Where(t => !IsPrimitiveType(t)) // Exclude primitives if we already added them
                .Where(PassesFilter)
                .OrderBy(t => GetCategory(t))
                .ThenBy(t => t.Name);

            foreach (var type in assemblyTypes)
            {
                string category = GetCategory(type);
                string displayName = $"{category}/{type.Name}";
                
                allTypes.Add(new ValueDropdownItem<Type>(displayName, type));
            }

            return allTypes.OrderBy(item => item.Text);
        }

        private bool IsPrimitiveType(Type type)
        {
            return type == typeof(string) || 
                   type == typeof(int) || 
                   type == typeof(float) || 
                   type == typeof(bool) || 
                   type == typeof(double) || 
                   type == typeof(long) || 
                   type == typeof(byte) || 
                   type == typeof(short);
        }

        private string GetCategory(Type type)
        {
            // Primitives
            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
            {
                return "Primitives";
            }

#if UNITY_EDITOR
            // Editor types
            if (type == typeof(UnityEditor.SceneAsset) || 
                type == typeof(UnityEditor.MonoScript) || 
                type == typeof(UnityEditor.DefaultAsset))
            {
                return "Unity/Editor";
            }
#endif
            
            // ScriptableObjects
            if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                if (type.Namespace != null && type.Namespace.StartsWith("UnityEngine"))
                    return "Unity/ScriptableObjects";
                return "Project/ScriptableObjects";
            }
            
            // MonoBehaviours
            if (typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                if (type.Namespace != null && type.Namespace.StartsWith("UnityEngine"))
                    return "Unity/MonoBehaviours";
                return "Project/MonoBehaviours";
            }
            
            // Components
            if (typeof(Component).IsAssignableFrom(type))
            {
                return "Unity/Components";
            }
            
            // Other Unity Objects
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return "Unity/Other";
            }
            
            // Serializable classes
            if (type.IsSerializable)
            {
                if (type.Namespace != null)
                {
                    if (type.Namespace.StartsWith("UnityEngine"))
                        return "Unity/Classes";
                    return $"Project/{type.Namespace.Split('.').First()}";
                }
                return "Classes";
            }
            
            return "Other";
        }

        private bool PassesFilter(Type type)
        {
            if (type == null) 
                return false;

            // Always allow primitives if no filters are set
            if (typeFilters == TypeFilter.None)
            {
                if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
                    return true;
                    
#if UNITY_EDITOR
                // Allow editor types
                if (type == typeof(UnityEditor.SceneAsset) || 
                    type == typeof(UnityEditor.MonoScript) || 
                    type == typeof(UnityEditor.DefaultAsset))
                    return true;
#endif

                // Allow Unity Objects or serializable types
                if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                    return !type.IsAbstract && !type.IsGenericTypeDefinition;
                    
                if (type.IsSerializable && !type.IsInterface && !type.IsGenericTypeDefinition)
                    return !type.IsAbstract;
                    
                return false;
            }

            // Apply explicit filters
            if (typeFilters.HasFlag(TypeFilter.NonAbstract) && type.IsAbstract)
                return false;

            if (typeFilters.HasFlag(TypeFilter.NonGeneric) && type.IsGenericTypeDefinition)
                return false;

            if (baseTypeConstraint != null && !baseTypeConstraint.IsAssignableFrom(type))
                return false;

            if (typeFilters.HasFlag(TypeFilter.ScriptableObjectOnly) && 
                !typeof(ScriptableObject).IsAssignableFrom(type))
                return false;

            if (typeFilters.HasFlag(TypeFilter.MonoBehaviourOnly) && 
                !typeof(MonoBehaviour).IsAssignableFrom(type))
                return false;

            if (typeFilters.HasFlag(TypeFilter.ComponentOnly) && 
                !typeof(Component).IsAssignableFrom(type))
                return false;

            if (typeFilters.HasFlag(TypeFilter.ProjectScriptsOnly) && 
                type.Assembly.GetName().Name != "Assembly-CSharp")
                return false;

            if (typeFilters.HasFlag(TypeFilter.UnityEngineOnly) && 
                !type.Assembly.FullName.Contains("UnityEngine"))
                return false;

            if (typeFilters.HasFlag(TypeFilter.HasParameterlessConstructor) && 
                type.GetConstructor(Type.EmptyTypes) == null && 
                !typeof(ScriptableObject).IsAssignableFrom(type))
                return false;

            if (namespaceFilters.Count > 0 && 
                !namespaceFilters.Any(ns => type.Namespace != null && type.Namespace.StartsWith(ns)))
                return false;

            if (excludedNamespaces.Count > 0 && 
                excludedNamespaces.Any(ns => type.Namespace != null && type.Namespace.StartsWith(ns)))
                return false;

            if (customFilter != null && !customFilter(type))
                return false;

            return true;
        }
    }

    [Serializable, InlineProperty, HideReferenceObjectPicker]
    public class FilteredTypeSelectorEntry
    {
        [HorizontalGroup("Entry", 0.3f)]
        [HideLabel]
        [LabelText("Type")]
        [ValueDropdown(nameof(GetFilteredTypes), NumberOfItemsBeforeEnablingSearch = 10)]
        [OnValueChanged(nameof(OnTypeChanged))]
        [ShowInInspector]
        [OdinSerialize]
        public Type SelectedType;

        [HorizontalGroup("Entry")]
        [HideLabel]
        [OdinSerialize]
        [InlineProperty]
        [HideReferenceObjectPicker]
        [ShowInInspector]
        [ShowIf(nameof(HasSelectedType))]
        public TypedValue TypedValue = new TypedValue();

        [SerializeField, HideInInspector, OdinSerialize]
        private TypeFilter typeFilters = TypeFilter.None;

        [SerializeField, HideInInspector, OdinSerialize]
        private Type baseTypeConstraint;

        [SerializeField, HideInInspector, OdinSerialize]
        private List<string> namespaceFilters = new List<string>();

        [SerializeField, HideInInspector, OdinSerialize]
        private List<string> excludedNamespaces = new List<string>();

        [SerializeField, HideInInspector, OdinSerialize]
        private Func<Type, bool> customFilter;

        [SerializeField, HideInInspector, OdinSerialize]
        private bool includePrimitives = true;

        private bool HasSelectedType => SelectedType != null;

        public FilteredTypeSelectorEntry()
        {
        }

        public FilteredTypeSelectorEntry(TypeFilter filters)
        {
            typeFilters = filters;
        }

        public FilteredTypeSelectorEntry(Type baseType, TypeFilter filters = TypeFilter.None)
        {
            baseTypeConstraint = baseType;
            typeFilters = filters;
        }

        public FilteredTypeSelectorEntry SetFilters(TypeFilter filters)
        {
            typeFilters = filters;
            return this;
        }

        public FilteredTypeSelectorEntry AddFilter(TypeFilter filter)
        {
            typeFilters |= filter;
            return this;
        }

        public FilteredTypeSelectorEntry SetBaseType(Type baseType)
        {
            baseTypeConstraint = baseType;
            return this;
        }

        public FilteredTypeSelectorEntry IncludePrimitives(bool include = true)
        {
            includePrimitives = include;
            return this;
        }

        public FilteredTypeSelectorEntry AddNamespaceFilter(params string[] namespaces)
        {
            namespaceFilters.AddRange(namespaces);
            return this;
        }

        public FilteredTypeSelectorEntry ExcludeNamespaces(params string[] namespaces)
        {
            excludedNamespaces.AddRange(namespaces);
            return this;
        }

        public FilteredTypeSelectorEntry SetCustomFilter(Func<Type, bool> filter)
        {
            customFilter = filter;
            return this;
        }

        public void EnsureTypedValueInitialized()
        {
            if (TypedValue == null)
            {
                TypedValue = new TypedValue();
                if (SelectedType != null)
                {
                    TypedValue.Type = SelectedType;
                }
            }
        }

        public object Value
        {
            get
            {
                EnsureTypedValueInitialized();
                return TypedValue.Value;
            }
            set
            {
                EnsureTypedValueInitialized();
                TypedValue.Value = value;
            }
        }

        private void OnTypeChanged()
        {
            EnsureTypedValueInitialized();
            TypedValue.Type = SelectedType;
        }

        private IEnumerable<ValueDropdownItem<Type>> GetFilteredTypes()
        {
            var allTypes = new List<ValueDropdownItem<Type>>();

            // Add primitives if enabled
            if (includePrimitives)
            {
                allTypes.AddRange(new[]
                {
                    new ValueDropdownItem<Type>("Primitives/String", typeof(string)),
                    new ValueDropdownItem<Type>("Primitives/Int", typeof(int)),
                    new ValueDropdownItem<Type>("Primitives/Float", typeof(float)),
                    new ValueDropdownItem<Type>("Primitives/Bool", typeof(bool)),
                    new ValueDropdownItem<Type>("Primitives/Double", typeof(double)),
                    new ValueDropdownItem<Type>("Primitives/Long", typeof(long)),
                    new ValueDropdownItem<Type>("Primitives/Byte", typeof(byte)),
                    new ValueDropdownItem<Type>("Primitives/Short", typeof(short)),
                });
            }

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => 
                {
                    try { return assembly.GetTypes(); }
                    catch { return Enumerable.Empty<Type>(); }
                })
                .Where(t => !IsPrimitiveType(t))
                .Where(PassesFilter)
                .OrderBy(t => GetCategory(t))
                .ThenBy(t => t.Name);

            foreach (var type in types)
            {
                string category = GetCategory(type);
                string displayName = $"{category}/{type.Name}";
                
                allTypes.Add(new ValueDropdownItem<Type>(displayName, type));
            }

            return allTypes.OrderBy(item => item.Text);
        }

        private bool IsPrimitiveType(Type type)
        {
            return type == typeof(string) || 
                   type == typeof(int) || 
                   type == typeof(float) || 
                   type == typeof(bool) || 
                   type == typeof(double) || 
                   type == typeof(long) || 
                   type == typeof(byte) || 
                   type == typeof(short);
        }

        private string GetCategory(Type type)
        {
            if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                if (type.Namespace != null && type.Namespace.StartsWith("UnityEngine"))
                    return "Unity/ScriptableObjects";
                return "Project/ScriptableObjects";
            }
            
            if (typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                if (type.Namespace != null && type.Namespace.StartsWith("UnityEngine"))
                    return "Unity/MonoBehaviours";
                return "Project/MonoBehaviours";
            }
            
            if (typeof(Component).IsAssignableFrom(type))
                return "Unity/Components";
            
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                return "Unity/Other";
            
            if (type.Namespace != null)
            {
                if (type.Namespace.StartsWith("UnityEngine"))
                    return "Unity/Classes";
                return $"Project/{type.Namespace.Split('.').First()}";
            }
            
            return "Other";
        }

        private bool PassesFilter(Type type)
        {
            if (type == null) 
                return false;

            if (typeFilters.HasFlag(TypeFilter.NonAbstract) && type.IsAbstract)
                return false;

            if (typeFilters.HasFlag(TypeFilter.NonGeneric) && type.IsGenericTypeDefinition)
                return false;

            if (baseTypeConstraint != null && !baseTypeConstraint.IsAssignableFrom(type))
                return false;

            if (typeFilters.HasFlag(TypeFilter.ScriptableObjectOnly) && 
                !typeof(ScriptableObject).IsAssignableFrom(type))
                return false;

            if (typeFilters.HasFlag(TypeFilter.MonoBehaviourOnly) && 
                !typeof(MonoBehaviour).IsAssignableFrom(type))
                return false;

            if (typeFilters.HasFlag(TypeFilter.ComponentOnly) && 
                !typeof(Component).IsAssignableFrom(type))
                return false;

            if (typeFilters.HasFlag(TypeFilter.ProjectScriptsOnly) && 
                type.Assembly.GetName().Name != "Assembly-CSharp")
                return false;

            if (typeFilters.HasFlag(TypeFilter.UnityEngineOnly) && 
                !type.Assembly.FullName.Contains("UnityEngine"))
                return false;

            if (typeFilters.HasFlag(TypeFilter.HasParameterlessConstructor) && 
                type.GetConstructor(Type.EmptyTypes) == null && 
                !typeof(ScriptableObject).IsAssignableFrom(type))
                return false;

            if (namespaceFilters.Count > 0 && 
                !namespaceFilters.Any(ns => type.Namespace != null && type.Namespace.StartsWith(ns)))
                return false;

            if (excludedNamespaces.Count > 0 && 
                excludedNamespaces.Any(ns => type.Namespace != null && type.Namespace.StartsWith(ns)))
                return false;

            if (customFilter != null && !customFilter(type))
                return false;

            return true;
        }
    }
}