using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MapTool
{
    [CreateAssetMenu(fileName = "NewCellType", menuName = "Map Tool/Cell Type")]
    public class TerrainTypeSO : ScriptableObject
    {
        [BoxGroup("Basic settings")]
        [SerializeField] public string DisplayName;

        [BoxGroup("Basic settings")]
        [SerializeField] public Color CellColor = Color.white;

        [BoxGroup("Basic settings")]
        [SerializeField, TextArea(1, 5)] public string Description = "";

        [BoxGroup("Basic settings")]
        [SerializeField, TextArea(1, 5)] public bool IsRender = true;

        [BoxGroup("Properties")]
        [SerializeField] public bool IsWalkable = true;


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
