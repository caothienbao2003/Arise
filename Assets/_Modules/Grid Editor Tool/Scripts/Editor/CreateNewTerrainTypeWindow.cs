#if UNITY_EDITOR
using CTB;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using System.IO;

namespace GridTool
{
    public class CreateNewTerrainTypeWindow
    {
        [SerializeField]
        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        private TerrainTypeSO terrainTypeSO = ScriptableObject.CreateInstance<TerrainTypeSO>();

        public string TerrainTypePath { get; set; }


        [BoxGroup("Actions")]
        [Button("Add New Cell Type", ButtonSizes.Medium), GUIColor(0.4f, 1f, 0.4f), HorizontalGroup("Actions/Buttons")]
        private void CreateNewData()
        {
            Debug.Log(terrainTypeSO.DisplayName);

            string folderPath = TerrainTypePath;

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string assetPath = $"{folderPath}/{terrainTypeSO.DisplayName}.asset";

            if (File.Exists(assetPath))
            {
                if (!EditorUtility.DisplayDialog(
                    "Cell Type already exist",
                    $"A Cell Type name '{terrainTypeSO.DisplayName} already exist at: \n{assetPath}'",
                    "Override",
                    "Cancel"))
                {
                    return;
                }
                AssetDatabase.DeleteAsset(assetPath);
            }

            AssetDatabase.CreateAsset(terrainTypeSO, assetPath);
            AssetDatabase.SaveAssets();

            terrainTypeSO = ScriptableObject.CreateInstance<TerrainTypeSO>();
        }
    }

    public class CreateNewTerrainTypeWindowBuilder
    {
        private CreateNewTerrainTypeWindow _window = new CreateNewTerrainTypeWindow();

        public CreateNewTerrainTypeWindowBuilder WithNewTerrainTypePath(string terrainTypePath)
        {
            _window.TerrainTypePath = terrainTypePath;
            return this;
        }

        public CreateNewTerrainTypeWindow Build() => _window;
    }
}
#endif