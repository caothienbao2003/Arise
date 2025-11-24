#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class EditorAssetUtils : MonoBehaviour
{
    /// <summary>
    /// Ensures that a folder exists in the project. Creates parent folders recursively if needed.
    /// 
    /// <param name="folderPath">Project-relative folder path, e.g., "Assets/Modules/MapTool/Data/CellTypes"</param>
    public static void EnsureFolderExists(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string parentFolder = Path.GetDirectoryName(folderPath).Replace("\\", "/"); //Sometimes Path.GetDirectoryName returns "\\" instead of "/"
        string newFolder = Path.GetFileName(parentFolder); // Get the last part of the path

        if (!AssetDatabase.IsValidFolder(parentFolder))
        {
            EnsureFolderExists(parentFolder); // Recursively ensure parent folder exists
        }

        AssetDatabase.CreateFolder(parentFolder, newFolder); // Create the new folder
        AssetDatabase.Refresh(); // Refresh the AssetDatabase to reflect changes
    }

    /// <summary>
    /// Creates a ScriptableObject asset at the specified path, ensuring the folder structure exists.
    /// 
    /// <typeparamref name="T"/>: Type of ScriptableObject to create.
    /// <param name="asset">The ScriptableObject instance to save as an asset.</param>
    /// <param name="assetPath">Project-relative path where the asset should be created, e.g., "Assets/Modules/MapTool/Data/CellTypes/MyCellType.asset"</paramref>
    public static void CreateAsset<T>(T asset, string assetPath) where T : ScriptableObject
    {
        string folderPath = Path.GetDirectoryName(assetPath).Replace("\\", "/");

        EnsureFolderExists(folderPath);

        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    /// <summary>
    /// Get all assets of type T in the specified folder.
    /// 
    /// <typeparamref name="T"/>: Type of ScriptableObject to find.
    /// <param name="folderPath">Project-relative folder path to search in, e.g., "Assets/Modules/MapTool/Data/CellTypes"</param>"
    public static T[] GetAllAssetsInFolder<T>(string folderPath) where T : ScriptableObject
    {
        if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
            return new T[0];

        GUID[] guids = AssetDatabase.FindAssetGUIDs($"t:{typeof(T).Name}", new[] { folderPath});
        T[] assets = new T[guids.Length];
        for(int i = 0; i < guids.Length; i++)
        {
            assets[i] = AssetDatabase.LoadAssetByGUID(guids[i], typeof(T)) as T;
        }

        return assets;
    }

    /// <summary>
    /// Deletes an asset at the given path safely.
    /// </summary>
    /// <param name="assetPath">Full asset path including file name</param>
    public static void DeleteAsset(string assetPath)
    {
        if (AssetDatabase.LoadAssetAtPath<Object>(assetPath) != null)
        {
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    /// <summary>
    /// Copies an asset from sourcePath to destinationPath safely, ensuring the destination folder exists.
    /// </summary> 
    /// <param name="sourcePath">Full source path including file name</param>
    /// <param name="destinationPath">Full destination path including file name</param>
    public static void CopyAsset(string sourcePath, string destinationPath)
    {
        if (AssetDatabase.LoadAssetAtPath<Object>(sourcePath) != null)
        {
            EnsureFolderExists(Path.GetDirectoryName(destinationPath).Replace("\\", "/"));
            AssetDatabase.CopyAsset(sourcePath, destinationPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif