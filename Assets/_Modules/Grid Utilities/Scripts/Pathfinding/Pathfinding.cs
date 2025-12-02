using GridTool;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GridUtilities
{
    public class Pathfinding
    {
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;

        private Grid<CellData> grid;
        private List<CellData> toProcessList;
        private List<CellData> processedList;

        public Pathfinding(Grid<CellData> grid)
        {
            this.grid = grid;
        }

        public List<CellData> FindPath(CellData startCell, CellData endCell)
        {
            toProcessList = new List<CellData> { startCell };
            processedList = new List<CellData>();

            //Reset all cells to start find path
            foreach (CellData cell in grid.GetAllGridObjects())
            {
                cell.GCost = int.MaxValue;
                cell.PreviousCell = null;
            }

            startCell.GCost = 0;
            startCell.HCost = CalculateMovementCost(startCell, endCell);

            while (toProcessList.Count > 0)
            {
                CellData currentCell = GetNextCurrentCell(toProcessList);
                if (currentCell == endCell)
                {
                    return CalculatePath(endCell);
                }

                toProcessList.Remove(currentCell);
                processedList.Add(currentCell);

                List<CellData> validNeighborCells = GetValidNeighborCells(currentCell);

                foreach (CellData neighborCell in validNeighborCells)
                {
                    int tentativeGCost = currentCell.GCost + CalculateMovementCost(currentCell, neighborCell);

                    bool isNeighborInToSearchList = toProcessList.Contains(neighborCell);

                    if (!isNeighborInToSearchList || tentativeGCost < neighborCell.GCost)
                    {
                        neighborCell.PreviousCell = currentCell;
                        neighborCell.GCost = tentativeGCost;
                        neighborCell.HCost = CalculateMovementCost(neighborCell, endCell);

                        if (!isNeighborInToSearchList)
                        {
                            toProcessList.Add(neighborCell);
                        }
                    }
                }
            }
            return null;
        }

        private List<CellData> GetValidNeighborCells(CellData cell)
        {
            return cell.GetNeighborCells().Where(t => !processedList.Contains(t)).ToList();
        }

        public List<CellData> CalculatePath(CellData endNode)
        {
            List<CellData> path = new List<CellData>();
            path.Add(endNode);
            CellData currentNode = endNode;
            while (currentNode.PreviousCell != null)
            {
                path.Add(currentNode.PreviousCell);
                currentNode = currentNode.PreviousCell;
            }
            path.Reverse();
            return path;
        }

        private int CalculateMovementCost(CellData cellA, CellData cellB)
        {
            int xDistance = Mathf.Abs(cellA.CellPosition.x - cellB.CellPosition.x);
            int yDistance = Mathf.Abs(cellA.CellPosition.y - cellB.CellPosition.y);

            int numberOfDiagonalMoves = Mathf.Min(xDistance, yDistance);
            int numberOfStraightMove = Math.Abs(xDistance - yDistance);

            return MOVE_DIAGONAL_COST * numberOfDiagonalMoves + MOVE_STRAIGHT_COST * numberOfStraightMove;
        }

        private CellData GetNextCurrentCell(List<CellData> cellList)
        {
            CellData lowestFCostCell = cellList[0];

            foreach (CellData cell in cellList)
            {
                if ((cell.FCost < lowestFCostCell.FCost) || (cell.FCost == lowestFCostCell.FCost && cell.HCost < lowestFCostCell.HCost))
                {
                    lowestFCostCell = cell;
                }
            }

            return lowestFCostCell;
        }
    }
}