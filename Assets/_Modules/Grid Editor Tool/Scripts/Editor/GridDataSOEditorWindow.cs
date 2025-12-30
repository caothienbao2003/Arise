using UnityEngine;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace GridTool
{
    [CustomEditor(typeof(GridDataSO))]
    public class GridDataSOEditorWindow : OdinEditor
    {
        private const float CELL_SIZE = 40f;
        private const float PADDING = 2f;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GridDataSO gridData = (GridDataSO)target;

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Visual Grid Preview", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // DrawGridVisualization(gridData);
        }

        // private void DrawGridVisualization(GridDataSO gridData)
        // {
        //     CellData[,] matrix = gridData.GeneratePreviewMatrix();
        //
        //     if (matrix == null || matrix.Length == 0)
        //     {
        //         EditorGUILayout.HelpBox("No grid data to preview. Add cells to see visualization.", MessageType.Info);
        //         return;
        //     }
        //
        //     int height = matrix.GetLength(0);
        //     int width = matrix.GetLength(1);
        //
        //     EditorGUILayout.BeginVertical(GUI.skin.box);
        //
        //     for (int y = 0; y < height; y++)
        //     {
        //         EditorGUILayout.BeginHorizontal();
        //
        //         for (int x = 0; x < width; x++)
        //         {
        //             CellData cell = matrix[y, x];
        //             DrawCell(cell);
        //         }
        //
        //         EditorGUILayout.EndHorizontal();
        //         GUILayout.Space(PADDING);
        //     }
        //
        //     EditorGUILayout.EndVertical();
        //
        //     EditorGUILayout.Space(10);
        //     EditorGUILayout.LabelField($"Grid Size: {width} x {height}", EditorStyles.miniLabel);
        // }
        //
        // private void DrawCell(CellData cell)
        // {
        //     Rect rect = GUILayoutUtility.GetRect(CELL_SIZE, CELL_SIZE);
        //
        //     if (cell != null)
        //     {
        //         Color prevColor = GUI.backgroundColor;
        //         GUI.backgroundColor = GetCellColor(cell);
        //
        //         GUI.Box(rect, GUIContent.none);
        //
        //         GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        //         {
        //             alignment = TextAnchor.MiddleCenter,
        //             fontSize = 8,
        //             fontStyle = FontStyle.Bold,
        //             normal = { textColor = Color.black }
        //         };
        //
        //         string label = $"{cell.GridPosition.x},{cell.GridPosition.y}";
        //         GUI.Label(rect, label, labelStyle);
        //
        //         GUI.backgroundColor = prevColor;
        //
        //         if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        //         {
        //             Debug.Log($"Clicked cell at {cell.GridPosition}");
        //             Event.current.Use();
        //         }
        //     }
        //     else
        //     {
        //         Color prevColor = GUI.color;
        //         GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.3f);
        //         GUI.Box(rect, GUIContent.none);
        //         GUI.color = prevColor;
        //     }
        //
        //     GUILayout.Space(PADDING);
        // }
        //
        // private Color GetCellColor(CellData cellData)
        // {
        //     // Customize based on your CellData properties
        //     // Example:
        //     // if (cellData.IsWalkable) return new Color(0.6f, 0.9f, 0.6f);
        //     // if (cellData.IsObstacle) return new Color(0.9f, 0.4f, 0.4f);
        //     // if (cellData.IsSpawnPoint) return new Color(0.4f, 0.7f, 0.9f);
        //     
        //     return new Color(0.6f, 0.9f, 0.6f, 1f);
        // }
    
    }
}