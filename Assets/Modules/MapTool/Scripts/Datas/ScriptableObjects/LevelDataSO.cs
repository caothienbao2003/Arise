using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace MapTool
{
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "MapTool/Level Data")]
    public class LevelDataSO : ScriptableObject
    {
        [HorizontalGroup("Header")]
        [VerticalGroup("Header/Left"), LabelWidth(100)]
        public string levelName = "";

        [VerticalGroup("Header/Right"), LabelWidth(100)]
        public SceneAsset levelScene;

        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        [SerializeField]
        public GridDataSO gridData;

        [Title("Grid Editor")]
        [ShowInInspector, TableMatrix(DrawElementMethod = nameof(DrawCell), SquareCells = true, ResizableColumns = false, RowHeight = 18)]
        private bool[,] gridPreview;

        private static Vector2Int? lastCell = null;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gridData == null)
            {
                gridPreview = null;
                return;
            }

            int w = gridData.width;
            int h = gridData.height;

            gridPreview = new bool[h, w];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    gridPreview[y, x] = gridData.GetCell(x, y).walkable;
        }

        [Button("Apply Grid Changes"), GUIColor(0.4f, 1f, 0.4f)]
        private void ApplyChanges()
        {
            if (gridData == null || gridPreview == null) return;

            int w = gridData.width;
            int h = gridData.height;

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    gridData.GetCell(x, y).walkable = gridPreview[y, x];

            UnityEditor.EditorUtility.SetDirty(gridData);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        public static bool DrawCell(Rect rect, bool value)
        {
            if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown)
                && rect.Contains(Event.current.mousePosition))
            {
                int cellX = Mathf.RoundToInt(rect.x);
                int cellY = Mathf.RoundToInt(rect.y);
                var currentCell = new Vector2Int(cellX, cellY);

                if (lastCell != currentCell)
                {
                    value = !value;
                    GUI.changed = true;
                    Event.current.Use();
                    lastCell = currentCell;
                }
            }

            if (Event.current.type == EventType.MouseUp)
            {
                lastCell = null;
            }

            EditorGUI.DrawRect(rect.Padding(1), value ? new Color(0.2f, 0.9f, 0.3f) : new Color(0.15f, 0.15f, 0.15f));
            return value;
        }
#endif
    }
}

