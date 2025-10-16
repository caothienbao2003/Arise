using UnityEngine;

namespace MapTool
{
    public class GridMap : MonoBehaviour
    {
        [SerializeField] private Canvas gridCanvas;
        [SerializeField] private Transform gridParent;

        [SerializeField] private Vector3 originPosition;

        private FN.Grid grid;
        private void Start()
        {
            grid = new FN.Grid(10, 10, 1f, canvas: gridCanvas, gridParent: gridParent, originPosition: originPosition);
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                grid.SetCellValue(worldPosition, 1);
            }

            if (Input.GetMouseButtonDown(1))
            {
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Debug.Log(grid.GetCellValue(worldPosition));
            }
        }
    }
}