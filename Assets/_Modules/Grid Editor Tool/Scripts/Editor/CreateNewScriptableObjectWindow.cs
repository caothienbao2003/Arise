#if UNITY_EDITOR
using CTB;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using System.IO;
// ReSharper disable All

namespace GridTool
{
    public class CreateNewScriptableObjectWindow<T> where T : ScriptableObject, IDisplayNameable
    {
        [SerializeField] [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        private T instance = ScriptableObject.CreateInstance<T>();
        
        private string folderPath { get; set; }

        public CreateNewScriptableObjectWindow(string folderPath)
        {
            this.folderPath = folderPath;
        }

        [BoxGroup("Actions")]
        [Button("Create", ButtonSizes.Medium), GUIColor(0.4f, 1f, 0.4f), HorizontalGroup("Actions/Buttons")]
        private void CreateNewData()
        {
            Debug.Log(instance.name);
            
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string typeName = typeof(T).Name;
            string nameToUse = string.IsNullOrEmpty(instance.DisplayName) ? "New_" + typeName : instance.DisplayName;
            string assetPath = $"{folderPath}/{typeName}_{nameToUse}.asset";

            if (File.Exists(assetPath))
            {
                if (!EditorUtility.DisplayDialog(
                    "Terrain Type already exist",
                    $"A Terrain Type name '{nameToUse} already exist at: \n{assetPath}'",
                    "Override",
                    "Cancel"))
                {
                    return;
                }
                AssetDatabase.DeleteAsset(assetPath);
            }

            AssetDatabase.CreateAsset(instance, assetPath);
            AssetDatabase.SaveAssets();

            instance = ScriptableObject.CreateInstance<T>();
        }
    }
}
#endif