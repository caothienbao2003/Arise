using GridTool;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GridUtilities
{
    public class Pathfinding<TPathNode> where TPathNode : class, IPathNode
    {
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;

        private Grid<TPathNode> grid;
        private List<TPathNode> toProcessList;
        private List<TPathNode> processedList;

        public Pathfinding(Grid<TPathNode> grid)
        {
            this.grid = grid;
        }

        public List<TPathNode> FindPath(TPathNode startNode, TPathNode endNode)
        {
            toProcessList = new List<TPathNode> { startNode };
            processedList = new List<TPathNode>();

            //Reset all cells to start find path
            foreach (TPathNode node in grid.GetAllGridObjects())
            {
                node.GCost = int.MaxValue;
                node.PreviousNode = null;
            }

            startNode.GCost = 0;
            startNode.HCost = CalculateMovementCost(startNode, endNode);

            while (toProcessList.Count > 0)
            {
                TPathNode currentNode = GetNextCurrentCell(toProcessList);
                if (currentNode == endNode)
                {
                    return CalculatePath(endNode);
                }

                toProcessList.Remove(currentNode);
                processedList.Add(currentNode);

                List<TPathNode> validNeighborNodes = GetValidNeighborCells(currentNode);

                foreach (TPathNode neighborNode in validNeighborNodes)
                {
                    int tentativeGCost = currentNode.GCost + CalculateMovementCost(currentNode, neighborNode);

                    bool isNeighborInToSearchList = toProcessList.Contains(neighborNode);

                    if (!isNeighborInToSearchList || tentativeGCost < neighborNode.GCost)
                    {
                        neighborNode.PreviousNode = currentNode;
                        neighborNode.GCost = tentativeGCost;
                        neighborNode.HCost = CalculateMovementCost(neighborNode, endNode);

                        if (!isNeighborInToSearchList)
                        {
                            toProcessList.Add(neighborNode);
                        }
                    }
                }
            }
            return null;
        }

        private List<TPathNode> GetValidNeighborCells(TPathNode node)
        {
            return grid.GetNeighbors(node.NodePosition).Where(cell => !processedList.Contains(cell)).ToList();
        }

        public List<TPathNode> CalculatePath(TPathNode endNode)
        {
            List<TPathNode> path = new List<TPathNode>();
            path.Add(endNode);
            TPathNode currentNode = endNode;
            while (currentNode?.PreviousNode != null)
            {
                path.Add(currentNode.PreviousNode as TPathNode);
                currentNode = currentNode.PreviousNode as TPathNode;
            }
            path.Reverse();
            return path;
        }

        private int CalculateMovementCost(TPathNode cellA, TPathNode cellB)
        {
            int xDistance = Mathf.Abs(cellA.NodePosition.x - cellB.NodePosition.x);
            int yDistance = Mathf.Abs(cellA.NodePosition.y - cellB.NodePosition.y);

            int numberOfDiagonalMoves = Mathf.Min(xDistance, yDistance);
            int numberOfStraightMove = Math.Abs(xDistance - yDistance);

            return MOVE_DIAGONAL_COST * numberOfDiagonalMoves + MOVE_STRAIGHT_COST * numberOfStraightMove;
        }

        private TPathNode GetNextCurrentCell(List<TPathNode> cellList)
        {
            TPathNode lowestFCostCell = cellList[0];

            foreach (TPathNode cell in cellList)
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