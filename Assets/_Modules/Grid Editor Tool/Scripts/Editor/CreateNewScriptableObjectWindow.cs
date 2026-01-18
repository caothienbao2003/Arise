using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using System.IO;
using Sirenix.Serialization;

namespace GridTool
{
    public class CreateNewScriptableObjectWindow<T> where T : ScriptableObject
    {
        [BoxGroup("Options")]
        [PropertyOrder(1)]
        [SerializeField] private bool createNewInstance = true;
        
        [BoxGroup("Options")]
        [PropertyOrder(2)]
        [ShowIf(nameof(createNewInstance))]
        [SerializeField] private string displayName;
        
        [PropertyOrder(3)]
        [BoxGroup("Instance")] 
        [OdinSerialize]
        [ShowInInspector]
        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        [ShowIf(nameof(createNewInstance))]
        private T _instance;

        private T instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = ScriptableObject.CreateInstance<T>();
                }
                return _instance;
            }
        }
        
        [OnInspectorInit]
        private void OnInspectorInit()
        {
            if (_instance == null)
            {
                _instance = ScriptableObject.CreateInstance<T>();
            }
        }
        
        private string folderPath { get; set; }

        public CreateNewScriptableObjectWindow(string folderPath)
        {
            this.folderPath = folderPath;
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
            {
                EditorUtility.DisplayDialog("Error", "Folder not valid", "Cancel");
                return false;
            }

            if (string.IsNullOrEmpty(displayName))
            {
                EditorUtility.DisplayDialog("Error", "Display name cannot null", "OK");
                return false;
            }

            return true;
        }

        [BoxGroup("Actions")]
        [PropertyOrder(999)]
        [ShowIf(nameof(createNewInstance))]
        [Button("Create", ButtonSizes.Large), GUIColor(0.4f, 1f, 0.4f), HorizontalGroup("Actions/Buttons")]
        private void CreateNewData()
        {
            if (!ValidateInput())
            {
                return;
            }
            
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string typeName = typeof(T).Name;
            string nameToUse = string.IsNullOrEmpty(displayName) ? "New_" + typeName : displayName;
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
        }
    }
}