using FreelancerNecromancer;
using System;

namespace MapTool
{
    [Serializable]
    public class CellData
    {
        private CellTypeSO cellType;

        private Grid<CellData> grid;
        private int x;
        private int y;

        public CellData(Grid<CellData> grid, int x, int y)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;
        }

        public CellTypeSO GetCellType()
        {
            return cellType;
        }

        public void SetCellType(CellTypeSO newCellType)
        {
            cellType = newCellType;
            grid.TriggerGridObjectChanged(x, y);
        }

        public override string ToString()
        {
            if(cellType == null)
                return "-";
            return cellType.DisplayName;
        }
    }
}