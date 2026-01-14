using GridTool;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GridUtilities
{
    public class Pathfinding<TPathNode>
        where TPathNode : class, IPathNode, IGridObject
    {
        private readonly Grid<TPathNode> grid;

        private List<TPathNode> openList;
        private List<TPathNode> closedList;

        public Pathfinding(Grid<TPathNode> grid)
        {
            this.grid = grid;
        }

        public Grid<TPathNode> GetGrid() => grid;

        // --------------------------------------------------
        // WORLD POSITION API
        // --------------------------------------------------

        public List<Vector3> FindPath(
            Vector3 startWorldPos,
            Vector3 endWorldPos,
            PathfindingProfileSO profileSO)
        {
            if (profileSO == null)
            {
                Debug.LogWarning("[Pathfinding] Profile is null.");
                return null;
            }

            TPathNode startNode = grid.GetCellGridObject(startWorldPos);
            TPathNode endNode = grid.GetCellGridObject(endWorldPos);

            if (startNode == null || endNode == null)
                return null;

            List<TPathNode> nodePath =
                FindPath(startNode, endNode, profileSO);

            if (nodePath == null)
                return null;

            List<Vector3> worldPath = new();
            foreach (TPathNode node in nodePath)
            {
                worldPath.Add(
                    grid.GetCellCenterWorldPos(node.GridPosition)
                );
            }

            return worldPath;
        }

        // --------------------------------------------------
        // NODE API
        // --------------------------------------------------

        public List<TPathNode> FindPath(
            TPathNode startNode,
            TPathNode endNode,
            PathfindingProfileSO profileSO)
        {
            if (startNode == null || endNode == null || profileSO == null)
                return null;

            openList = new List<TPathNode> { startNode };
            closedList = new List<TPathNode>();

            // Reset nodes
            foreach (TPathNode node in grid.GetAllGridObjects())
            {
                node.GCost = int.MaxValue;
                node.PreviousNode = null;
            }

            startNode.GCost = 0;
            startNode.HCost =
                CalculateMovementCost(startNode, endNode, profileSO);

            while (openList.Count > 0)
            {
                TPathNode currentNode =
                    GetLowestFCostNode(openList);

                if (currentNode == endNode)
                    return BuildPath(endNode);

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (TPathNode neighbor
                         in GetValidNeighbors(currentNode, profileSO))
                {
                    if (!neighbor.IsWalkable ||
                        closedList.Contains(neighbor))
                        continue;

                    int tentativeGCost =
                        currentNode.GCost +
                        CalculateMovementCost(
                            currentNode,
                            neighbor,
                            profileSO);

                    if (tentativeGCost < neighbor.GCost)
                    {
                        neighbor.PreviousNode = currentNode;
                        neighbor.GCost = tentativeGCost;
                        neighbor.HCost =
                            CalculateMovementCost(
                                neighbor,
                                endNode,
                                profileSO);

                        if (!openList.Contains(neighbor))
                            openList.Add(neighbor);
                    }
                }
            }

            return null;
        }

        // --------------------------------------------------
        // NEIGHBORS
        // --------------------------------------------------

        private List<TPathNode> GetValidNeighbors(
            TPathNode node,
            PathfindingProfileSO profileSO)
        {
            List<TPathNode> neighbors = new();
            Vector2Int pos = node.GridPosition;

            // Straight
            TryAddNeighbor(pos + Vector2Int.up, neighbors);
            TryAddNeighbor(pos + Vector2Int.down, neighbors);
            TryAddNeighbor(pos + Vector2Int.left, neighbors);
            TryAddNeighbor(pos + Vector2Int.right, neighbors);

            if (!profileSO.AllowDiagonal)
                return neighbors;

            // Diagonals
            TryAddDiagonal(
                pos,
                Vector2Int.up + Vector2Int.right,
                Vector2Int.up,
                Vector2Int.right,
                neighbors,
                profileSO);

            TryAddDiagonal(
                pos,
                Vector2Int.up + Vector2Int.left,
                Vector2Int.up,
                Vector2Int.left,
                neighbors,
                profileSO);

            TryAddDiagonal(
                pos,
                Vector2Int.down + Vector2Int.right,
                Vector2Int.down,
                Vector2Int.right,
                neighbors,
                profileSO);

            TryAddDiagonal(
                pos,
                Vector2Int.down + Vector2Int.left,
                Vector2Int.down,
                Vector2Int.left,
                neighbors,
                profileSO);

            return neighbors;
        }

        private void TryAddNeighbor(
            Vector2Int gridPos,
            List<TPathNode> neighbors)
        {
            TPathNode node = grid.GetCellGridObject(gridPos);
            if (node == null) return;
            if (closedList.Contains(node)) return;

            neighbors.Add(node);
        }

        private void TryAddDiagonal(
            Vector2Int from,
            Vector2Int diagonalOffset,
            Vector2Int straightA,
            Vector2Int straightB,
            List<TPathNode> neighbors,
            PathfindingProfileSO profileSO)
        {
            TPathNode diagonalNode =
                grid.GetCellGridObject(from + diagonalOffset);

            if (diagonalNode == null) return;
            if (closedList.Contains(diagonalNode)) return;

            if (profileSO.BlockDiagonalCorner)
            {
                TPathNode a =
                    grid.GetCellGridObject(from + straightA);
                TPathNode b =
                    grid.GetCellGridObject(from + straightB);

                if ((a != null && !a.IsWalkable) ||
                    (b != null && !b.IsWalkable))
                    return;
            }

            neighbors.Add(diagonalNode);
        }

        // --------------------------------------------------
        // HELPERS
        // --------------------------------------------------

        private List<TPathNode> BuildPath(TPathNode endNode)
        {
            List<TPathNode> path = new();
            TPathNode current = endNode;

            while (current != null)
            {
                path.Add(current);
                current = current.PreviousNode as TPathNode;
            }

            path.Reverse();
            return path;
        }

        private int CalculateMovementCost(
            TPathNode a,
            TPathNode b,
            PathfindingProfileSO profileSO)
        {
            int dx = Mathf.Abs(
                a.GridPosition.x - b.GridPosition.x);
            int dy = Mathf.Abs(
                a.GridPosition.y - b.GridPosition.y);

            int diagonalMoves = Mathf.Min(dx, dy);
            int straightMoves = Mathf.Abs(dx - dy);

            return diagonalMoves * profileSO.MoveDiagonalCost +
                   straightMoves * profileSO.MoveStraightCost;
        }

        private TPathNode GetLowestFCostNode(
            List<TPathNode> list)
        {
            TPathNode best = list[0];

            foreach (TPathNode node in list)
            {
                if (node.FCost < best.FCost ||
                   (node.FCost == best.FCost &&
                    node.HCost < best.HCost))
                {
                    best = node;
                }
            }

            return best;
        }
    }
}
