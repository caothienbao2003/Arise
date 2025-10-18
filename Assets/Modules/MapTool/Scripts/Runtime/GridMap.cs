using FreelancerNecromancer;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MapTool
{
    public class GridMap : MonoBehaviour
    {
        [SerializeField] private Vector3 originPosition;

        [SerializeField] private float cellSize;

        private FreelancerNecromancer.Grid<CellData> grid;

        [SerializeField]
        private CellTypeSO currentCellType;

        private void Start()
        {
            grid = new FreelancerNecromancer.Grid<CellData>(10, 10, cellSize: cellSize, originPosition: originPosition, CreateCellData);
        }

        private CellData CreateCellData(Grid<CellData> grid, int x, int y)
        {
            CellData cellData = new CellData(grid, x, y);
            cellData.SetCellType(currentCellType);
            return cellData;
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                grid.GetCellGridObject(worldPosition).SetCellType(currentCellType);
            }

            if (Input.GetMouseButtonDown(1))
            {
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Debug.Log(grid.GetCellGridObject(worldPosition));
            }
        }
    }
}