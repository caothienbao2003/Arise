#if UNITY_EDITOR
using UnityEngine;
using Sirenix.OdinInspector;

using UnityEditor;
using System.IO;

namespace MapTool
{
    public class CreateNewTerrainTypeWindow
    {
        public CreateNewTerrainTypeWindow(MapToolSettingsSO settings)
        {
            cellTypeSO = ScriptableObject.CreateInstance<TerrainTypeSO>();
            this.settings = settings;
        }

        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public TerrainTypeSO cellTypeSO;

        private MapToolSettingsSO settings;

        [BoxGroup("Actions")]
        [Button("Add New Cell Type", ButtonSizes.Medium), GUIColor(0.4f, 1f, 0.4f), HorizontalGroup("Actions/Buttons")]
        private void CreateNewData()
        {
            Debug.Log(cellTypeSO.DisplayName);

            string folderPath = settings.terrainTypePath;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string assetPath = $"{folderPath}/{cellTypeSO.DisplayName}.asset";

            if (File.Exists(assetPath))
            {
                if (!EditorUtility.DisplayDialog(
                    "Cell Type already exist",
                    $"A Cell Type name '{cellTypeSO.DisplayName} already exist at: \n{assetPath}'",
                    "Override",
                    "Cancel"))
                {
                    return;
                }
                AssetDatabase.DeleteAsset(assetPath);
            }

            AssetDatabase.CreateAsset(cellTypeSO, assetPath);
            AssetDatabase.SaveAssets();

            cellTypeSO = ScriptableObject.CreateInstance<TerrainTypeSO>();
        }
    }
}
#endif