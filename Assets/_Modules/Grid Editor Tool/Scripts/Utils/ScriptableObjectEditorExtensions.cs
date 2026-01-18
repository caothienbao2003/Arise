#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace GridTool
{
    public static class ScriptableObjectEditorExtensions
    {
        public static string GetTrimmedAssetName(this ScriptableObject so)
        {
            if (so == null)
                return string.Empty;

            string assetPath = AssetDatabase.GetAssetPath(so);
            if (string.IsNullOrEmpty(assetPath))
                return so.name;

            string fileName = Path.GetFileNameWithoutExtension(assetPath);

            string prefix = so.GetType().Name + "_";
            if (fileName.StartsWith(prefix))
            {
                return fileName.Substring(prefix.Length);
            }

            return fileName;
        }
    }
}
#endif