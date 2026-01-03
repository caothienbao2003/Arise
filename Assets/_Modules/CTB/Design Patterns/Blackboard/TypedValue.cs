using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable, InlineProperty, HideReferenceObjectPicker]
public class TypedValue
{
    [HideInInspector] [OdinSerialize] private Type cachedType;

    [HideLabel]
    [ShowIf(nameof(IsUnityObjectType))]
    [AssetsOnly]
    [ShowInInspector]
    [ValueDropdown(nameof(GetValueDropdown), AppendNextDrawer = true, DisableListAddButtonBehaviour = true)]
    public UnityEngine.Object ValueAsset;

    [HideLabel] [OdinSerialize] [HideIf(nameof(IsUnityObjectType))] [ShowInInspector]
    public object ValuePrimitive;

    public Type Type
    {
        get => cachedType;
        set
        {
            if (cachedType == value) return;
            cachedType = value;
            InitializeValue();
        }
    }

    public object Value
    {
        get => IsUnityObjectType ? (object)ValueAsset : ValuePrimitive;
        set
        {
            if (cachedType != null && typeof(UnityEngine.Object).IsAssignableFrom(cachedType))
            {
                ValueAsset = value as UnityEngine.Object;
                ValuePrimitive = null;
            }
            else
            {
                ValuePrimitive = value;
                ValueAsset = null;
            }
        }
    }

    private bool IsUnityObjectType => cachedType != null && typeof(UnityEngine.Object).IsAssignableFrom(cachedType);

    private IEnumerable<ValueDropdownItem<object>> GetValueDropdown()
    {
        if (cachedType == null) yield break;

        if (typeof(UnityEngine.Object).IsAssignableFrom(cachedType))
        {
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:{cachedType.Name}");

            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                UnityEngine.Object asset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, cachedType);

                if (asset != null)
                {
                    yield return new ValueDropdownItem<object>(asset.name, asset);
                }
            }

            var loadedObjects = Resources.FindObjectsOfTypeAll(cachedType)
                .Where(obj =>
                    UnityEditor.AssetDatabase.Contains(obj) &&
                    (obj.hideFlags & HideFlags.HideInInspector) == 0);

            foreach (var obj in loadedObjects)
            {
                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(assetPath) &&
                    !guids.Any(g => UnityEditor.AssetDatabase.GUIDToAssetPath(g) == assetPath))
                {
                    yield return new ValueDropdownItem<object>(obj.name, obj);
                }
            }
#else
                var objects = Resources.FindObjectsOfTypeAll(cachedType)
                    .Where(obj => (obj.hideFlags & HideFlags.HideInInspector) == 0);

                foreach (var obj in objects)
                {
                    yield return new ValueDropdownItem<object>(obj.name, obj);
                }
#endif
        }
    }

    private void InitializeValue()
    {
        if (cachedType == null)
        {
            ValueAsset = null;
            ValuePrimitive = null;
            return;
        }

        if (Value != null && cachedType.IsAssignableFrom(Value.GetType())) return;

        if (typeof(UnityEngine.Object).IsAssignableFrom(cachedType))
        {
            ValueAsset = null;
            ValuePrimitive = null;
        }
        else if (cachedType == typeof(string))
        {
            ValuePrimitive = "";
            ValueAsset = null;
        }
        else if (cachedType == typeof(int))
        {
            ValuePrimitive = 0;
            ValueAsset = null;
        }
        else if (cachedType == typeof(float))
        {
            ValuePrimitive = 0f;
            ValueAsset = null;
        }
        else if (cachedType == typeof(bool))
        {
            ValuePrimitive = false;
            ValueAsset = null;
        }
        else if (cachedType == typeof(double))
        {
            ValuePrimitive = 0.0;
            ValueAsset = null;
        }
        else if (cachedType == typeof(long))
        {
            ValuePrimitive = 0L;
            ValueAsset = null;
        }
        else if (cachedType == typeof(byte))
        {
            ValuePrimitive = (byte)0;
            ValueAsset = null;
        }
        else if (cachedType == typeof(short))
        {
            ValuePrimitive = (short)0;
            ValueAsset = null;
        }
        else if (cachedType.IsEnum)
        {
            ValuePrimitive = Enum.GetValues(cachedType).GetValue(0);
            ValueAsset = null;
        }
        else if (cachedType.IsPrimitive)
        {
            ValuePrimitive = Activator.CreateInstance(cachedType);
            ValueAsset = null;
        }
        else
        {
            try
            {
                ValuePrimitive = Activator.CreateInstance(cachedType);
                ValueAsset = null;
            }
            catch
            {
                ValuePrimitive = null;
                ValueAsset = null;
            }
        }
    }
}