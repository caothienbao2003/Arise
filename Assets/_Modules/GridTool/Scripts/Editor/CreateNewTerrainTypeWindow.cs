#if UNITY_EDITOR
using UnityEngine;
using Sirenix.OdinInspector;

using UnityEditor;
using System.IO;

namespace GridTool
{
    public class CreateNewTerrainTypeWindow
    {
        public CreateNewTerrainTypeWindow(GridToolSettingsSO settings)
        {
            cellTypeSO = ScriptableObject.CreateInstance<TerrainTypeSO>();
            this.settings = settings;
        }

        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public TerrainTypeSO cellTypeSO;

        private GridToolSettingsSO settings;

        [BoxGroup("Actions")]
        [Button("Add New Cell Type", ButtonSizes.Medium), GUIColor(0.4f, 1f, 0.4f), HorizontalGroup("Actions/Buttons")]
        private void CreateNewData()
        {
            Debug.Log(cellTypeSO.DisplayName);

            string folderPath = GridToolPaths.TerrainTypes.TERRAIN_TYPE_FOLDER;
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