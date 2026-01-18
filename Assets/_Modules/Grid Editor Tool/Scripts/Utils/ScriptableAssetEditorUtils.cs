#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ScriptableAssetEditorUtils
{
    public static bool IsSavedAsset(Object asset)
    { 
        return !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(asset));
    }

    public static void Open(Object asset)
    {
        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
    }

    public static bool ConfirmDelete(Object asset)
    {
        string path = AssetDatabase.GetAssetPath(asset);

        return EditorUtility.DisplayDialog(
            "Delete Asset",
            $"Are you sure you want to delete '{asset.name}'?\n\nPath:\n{path}",
            "Delete",
            "Cancel"
        );
    }

    public static void Delete(Object asset)
    {
        string path = AssetDatabase.GetAssetPath(asset);

        if (string.IsNullOrEmpty(path))
            return;

        if (AssetDatabase.DeleteAsset(path))
        {
            AssetDatabase.SaveAssets();
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Error",
                $"Failed to delete asset at:\n{path}",
                "OK"
            );
        }
    }
}
#endif