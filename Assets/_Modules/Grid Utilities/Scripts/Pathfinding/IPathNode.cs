using System.Collections.Generic;
using UnityEngine;

namespace GridUtilities
{
    public interface IPathNode
    {
        int GCost { get; set; }
        int HCost { get; set; }
        int FCost => GCost + HCost;
        IPathNode PreviousNode { get; set;}
        Vector2Int NodePosition { get; set; }
        bool IsWalkable { get; set; }
    }
}