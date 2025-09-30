using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace MapTool
{
    public class MapToolEditor : OdinMenuEditorWindow
    {
        [BoxGroup("Cell types settings"), SerializeField]
        private string dataPath = "Assets/Modules/MapTool/Data/CellTypes";

        [MenuItem("Tools/Map Tool")]
        private static void OpenWindow()
        {
            var window = GetWindow<MapToolEditor>();
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();

            tree.Add("Cell Types/ Create New Type", new CreateNewCellTypeData(dataPath));
            tree.AddAllAssetsAtPath("Cell Types/ All Cell Types", dataPath, typeof(CellTypeSO));

            tree.Add("Level editor/ Create New Level", new CreateNewLevelData());

            tree.Add("Settings", this);
            return tree;
        }
    }

    public class CreateNewCellTypeData
    {
        public CreateNewCellTypeData(string dataPath)
        {
            cellTypeSO = ScriptableObject.CreateInstance<CellTypeSO>();
            this.dataPath = dataPath;
        }

        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public CellTypeSO cellTypeSO;

        private string dataPath = "";

        [BoxGroup("Actions")]
        [Button("Add New Cell Type", ButtonSizes.Medium), GUIColor(0.4f, 1f, 0.4f), HorizontalGroup("Actions/Buttons")]
        private void CreateNewData()
        {
            Debug.Log(dataPath);
            Debug.Log(cellTypeSO.DisplayName);
            AssetDatabase.CreateAsset(cellTypeSO, $"{dataPath}/{cellTypeSO.DisplayName}.asset");
            AssetDatabase.SaveAssets();

            cellTypeSO = ScriptableObject.CreateInstance<CellTypeSO>();
        }
    }

    public class CreateNewLevelData
    {
        public CreateNewLevelData()
        {
            
        }
    }
}