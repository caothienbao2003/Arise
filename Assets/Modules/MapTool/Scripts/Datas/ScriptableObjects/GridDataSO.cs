using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MapTool
{
    [CreateAssetMenu(fileName = "NewGridData", menuName = "MapTool/Grid Data")]
    public class GridDataSO : ScriptableObject
    {
        [HorizontalGroup("Grid Settings")]
        [VerticalGroup("Grid Settings/Width")]
        public int width = 10;

        [HorizontalGroup("Grid Settings")]
        [VerticalGroup("Grid Settings/Height")]
        public int height = 10;

        [SerializeField, HideInInspector]
        public List<CellData> cellDatas = new();

#if UNITY_EDITOR
        [ShowInInspector]
        [ValueDropdown(nameof(GetAllCellTypes))]
        [LabelText("Current Cell Type (Editor Only)")]
        [NonSerialized] // <-- makes sure this is NOT saved with the asset
        public CellTypeSO CurrentCellType;
#endif

        [Space(20)]
        [ShowInInspector, TableMatrix(DrawElementMethod = nameof(DrawCell), SquareCells = true, RowHeight = 20, ResizableColumns = false)]
        private CellData[,] previewMatrix;

#if UNITY_EDITOR

        private void OnValidate()
        {
            EnsureGridSize();
            RebuildPreview();
        }

        public IEnumerable<CellTypeSO> GetAllCellTypes()
        {
            string cellTypePath = MapToolSettingsSO.Instance.cellTypePath;

            if (string.IsNullOrEmpty(cellTypePath))
            {
                return Enumerable.Empty<CellTypeSO>();
            }

            //Find all CellTypeSO assets in the specified path, of type GUID
            GUID[] guids = AssetDatabase.FindAssetGUIDs("t:CellTypeSO", new[] { cellTypePath });

            //Load and return the assets
            List<CellTypeSO> cellTypes = new List<CellTypeSO>();

            foreach (GUID guid in guids)
            {
                CellTypeSO cellType = AssetDatabase.LoadAssetByGUID<CellTypeSO>(guid);

                if (cellType != null)
                {
                    cellTypes.Add(cellType);
                }
            }

            return cellTypes.OrderBy(ct => ct.DisplayName);
        }

        private void EnsureGridSize()
        {
            int targetCount = width * height;
            if (cellDatas == null)
                cellDatas = new List<CellData>(targetCount);

            if (cellDatas.Count != targetCount)
            {
                var newList = new List<CellData>(targetCount);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = y * width + x;
                        if (index < cellDatas.Count)
                            newList.Add(cellDatas[index]);
                        else
                            newList.Add(new CellData { Position = new Vector2Int(x, y) });
                    }
                }
                cellDatas = newList;
            }
        }

        private void RebuildPreview()
        {
            previewMatrix = new CellData[height, width];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    previewMatrix[y, x] = GetCell(x, y);
        }

        public static CellData DrawCell(Rect rect, CellData cell)
        {
            if (cell == null)
                return null;

            // Color based on the assigned CellTypeSO
            Color color = cell.CellType != null ? cell.CellType.CellColor : new Color(0.2f, 0.2f, 0.2f);
            EditorGUI.DrawRect(rect.Padding(1), color);

            // Draw first letter of cell type name for quick visual aid
            if (cell.CellType != null && !string.IsNullOrEmpty(cell.CellType.DisplayName))
            {
                var label = cell.CellType.DisplayName.Substring(0, 1).ToUpper();
                var centered = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white },
                    fontStyle = FontStyle.Bold
                };
                EditorGUI.LabelField(rect, label, centered);
            }

            return cell;
        }

        public CellData GetCell(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                Debug.LogError("GetCellData: Index out of range");
                return null;
            }
            int index = y * width + x;
            if (index < 0 || index >= cellDatas.Count)
            {
                Debug.LogError("GetCellData: Index out of range");
                return null;
            }
            return cellDatas[index];
        }
#endif
    }
}