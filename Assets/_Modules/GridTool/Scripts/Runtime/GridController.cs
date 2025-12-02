using UnityEngine;
using GridUtilities;

namespace GridTool
{
    public class GridController : MonoBehaviour
    {
        [SerializeField] private GridDataSO gridDataSo;

        private Pathfinding pathfinding;
        
        private Grid<CellData> grid;
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
            grid = gridDataSo.CreateRuntimeGrid();
            pathfinding = new Pathfinding(grid);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleClick();
            }
        }
        
        private void HandleClick()
        {
            Vector2Int cellXYPos = grid.GetCellXYPos(GetMouseToWorldPosition());
            CellData cellData = grid.GetCellGridObject(cellXYPos);

            if (cellData != null)
            {
                Debug.Log($"Clicked on {cellData.TerrainType.DisplayName}, Cell XY Position: {cellXYPos}");
            }
        }   
        
        private Vector2 GetMouseToWorldPosition()
        {
            if (!mainCamera) return Vector2.zero;

            Vector3 vec = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            vec.z = 0f;
            return vec;
        }
    }
}