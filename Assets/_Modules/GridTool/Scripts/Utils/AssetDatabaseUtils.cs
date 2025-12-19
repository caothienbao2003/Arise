#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class AssetDatabaseUtils
{
    public static void EnsureFolderExists(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string parentFolder = Path.GetDirectoryName(folderPath)?.Replace("\\", "/"); 
    
        string newFolderName = Path.GetFileName(folderPath); 
        
        if (!AssetDatabase.IsValidFolder(parentFolder))
        {
            EnsureFolderExists(parentFolder); 
        }

        string guid = AssetDatabase.CreateFolder(parentFolder, newFolderName);
    
        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogError($"Failed to create folder: {folderPath}. Check permissions or name validity.");
        }

        AssetDatabase.Refresh(); 
    }
    
    public static void CreateAsset<T>(T asset, string assetPath, bool pingAsset = true) where T : Object
    {
        string folderPath = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");

        EnsureFolderExists(folderPath);

        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        if (pingAsset)
        {
            EditorGUIUtility.PingObject(asset);
        }
    }
    
    public static T CreateScriptableAsset<T>(string assetName, string assetPath, bool pingAsset = true) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();
        asset.name = assetName;
        CreateAsset(asset, assetPath, pingAsset);

        return asset;
    }
    
    public static T[] GetAllAssetsInFolder<T>(string folderPath) where T : ScriptableObject
    {
        if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
            return Array.Empty<T>();

        GUID[] guids = AssetDatabase.FindAssetGUIDs($"t:{typeof(T).Name}", new[] { folderPath});
        T[] assets = new T[guids.Length];
        for(int i = 0; i < guids.Length; i++)
        {
            assets[i] = AssetDatabase.LoadAssetByGUID(guids[i], typeof(T)) as T;
        }

        return assets;
    }
    
    public static void DeleteAsset(string assetPath)
    {
        if (AssetDatabase.LoadAssetAtPath<Object>(assetPath) != null)
        {
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    
    public static bool CopyAsset(string sourcePath, string destinationPath)
    {
        bool success = false;
        if (AssetDatabase.LoadAssetAtPath<Object>(sourcePath) != null)
        {
            EnsureFolderExists(Path.GetDirectoryName(destinationPath)?.Replace("\\", "/"));
            success = AssetDatabase.CopyAsset(sourcePath, destinationPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        return success;
    }
}
#endif