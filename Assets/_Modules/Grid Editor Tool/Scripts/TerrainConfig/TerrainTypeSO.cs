using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GridTool
{
    [CreateAssetMenu(fileName = "NewCellType", menuName = "Map Tool/Cell Type")]
    public class TerrainTypeSO : ScriptableObject, IDisplayNameable
    {
        [TabGroup("Basic info")] [SerializeField]
        private string displayName;
        
        public string DisplayName => displayName;

        [TabGroup("Basic info")]
        [SerializeField, TextArea(1, 5)] public string Description = "";
        
        [TabGroup("Properties")]
        [SerializeField] public bool IsWalkable = true;
        
        [TabGroup("Properties")]
        [SerializeField] public int Priority = 0;
        
        [TabGroup("Visualization")]
        [SerializeField] public Color CellColor = Color.white;
        
        [TabGroup("Visualization")]
        [SerializeField] public bool IsRender = true;

#if UNITY_EDITOR
        private bool IsSavedAsset => !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(this));

        [BoxGroup("Actions"), PropertyOrder(100)]
        [Button("Open Asset", ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1f)]
        [HorizontalGroup("Actions/Buttons")]
        [ShowIf(nameof(IsSavedAsset))]
        private void OpenAsset()
        {
            Selection.activeObject = this;
            EditorGUIUtility.PingObject(this);
        }

        [BoxGroup("Actions"), PropertyOrder(101)]
        [Button("Delete Asset", ButtonSizes.Medium), GUIColor(1f, 0.4f, 0.4f)]
        [HorizontalGroup("Actions/Buttons")]
        [ShowIf(nameof(IsSavedAsset))]
        private void DeleteAsset()
        {
            string assetPath = AssetDatabase.GetAssetPath(this);

            if (string.IsNullOrEmpty(assetPath))
            {
                EditorUtility.DisplayDialog("Error", "Cannot delete this asset. Asset path not found.", "OK");
                return;
            }

            bool confirmed = EditorUtility.DisplayDialog(
                "Delete Cell Type",
                $"Are you sure you want to delete '{DisplayName}'?\n\nPath: {assetPath}\n\nThis action cannot be undone!",
                "Delete",
                "Cancel"
            );

            if (confirmed)
            {
                bool success = AssetDatabase.DeleteAsset(assetPath);

                if (success)
                {
                    AssetDatabase.SaveAssets();
                    Debug.Log($"Deleted Cell Type: {DisplayName} at {assetPath}");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", $"Failed to delete asset at:\n{assetPath}", "OK");
                }
            }
        }
#endif
    }
}
