#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CTB
{
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
                Debug.LogError($"[AssetDatabaseUtils] EnsureFolderExisted - Failed to create folder: {folderPath}. Check permissions or name validity.");
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

            Debug.Log($"[AssetDatabaseUtils] Created asset: {assetPath}");
            
            if (pingAsset)
            {
                EditorGUIUtility.PingObject(asset);
            }
            
            Debug.Log($"[AssetDatabaseUtils] Create assset at path: {assetPath}");
        }

        public static T CreateScriptableAsset<T>(string assetName, string assetPath, bool pingAsset = true) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            asset.name = assetName;
            CreateAsset(asset, assetPath, pingAsset);

            return asset;
        }

        public static T[] GetAllAssetsInFolder<T>(string folderPath) where T : Object
        {
            string filter = $"t:{typeof(T).Name}";
            
            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
                return Array.Empty<T>();

            GUID[] guids = AssetDatabase.FindAssetGUIDs(filter, new[] { folderPath });
            return ConvertsGuidsToAssets<T>(guids);
        }

        public static T GetAssetAtFolder<T>(string folderPath) where T : Object
        {
            T asset = GetAllAssetsInFolder<T>(folderPath).FirstOrDefault();
            
            Debug.Log($"[AssetDatabaseUtils] Get asset at folder: {folderPath}, asset: {asset}");

            return asset;
        }

        public static bool DeleteAsset(string assetPath)
        {
            bool success = false;
            
            if (AssetDatabase.LoadAssetAtPath<Object>(assetPath) != null)
            {
                success = AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            
            if (success)
            {
                EditorUtility.DisplayDialog("Success", $"Deleted level data at {assetPath}", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Failed", "Failed to delete asset.", "OK");
            }
            
            Debug.Log($"[AssetDatabaseUtils] Deleted asset: {assetPath}, success: {success}");
            return success;
        }

        public static bool CopyAsset(Object sourceAsset, string destinationPath)
        {
            return CopyAsset(AssetDatabase.GetAssetPath(sourceAsset), destinationPath);
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
            
            Debug.Log($"[AssetDatabaseUtils] Copy asset: {sourcePath} -> {destinationPath}, success: {success}");
            
            return success;
        }

        public static string GetAssetPath(Object asset)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            Debug.Log($"[AssetDatabaseUtils] Get asset path: {assetPath}");
            return assetPath;
        }

        public static string GetParentFolderPath(string assetPath)
        {
            return Path.GetDirectoryName(assetPath);
        }
        
        private static T[] ConvertsGuidsToAssets<T>(GUID[] guids) where T : Object
        {
            T[] assets = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                assets[i] = AssetDatabase.LoadAssetByGUID(guids[i], typeof(T)) as T;
            }

            return assets;
        }

        public static bool CheckFileExisted(string filePath)
        {
            // Check if the asset actually exists at the given path
            Object existingAsset = AssetDatabase.LoadAssetAtPath<Object>(filePath);

            if (existingAsset != null)
            {
                // Pop up a dialog box to the user
                bool overwrite = EditorUtility.DisplayDialog(
                    "File Already Exists",                   // Title
                    $"An asset already exists at {filePath}. Do you want to overwrite it?", // Message
                    "Yes, Overwrite",                        // Confirm button
                    "No, Cancel"                             // Cancel button
                );

                if (overwrite)
                {
                    // If user says yes, we delete the old one so the new one can take its place
                    AssetDatabase.DeleteAsset(filePath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    return true; // Safe to proceed with creating the new asset
                }
                else
                {
                    Debug.Log($"[AssetDatabaseUtils] Operation cancelled by user. File exists at: {filePath}");
                    return false; // Do not proceed
                }
            }

            // File doesn't exist, safe to proceed
            return true;
        }
    }
}
#endif