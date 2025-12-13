using UnityEngine;

namespace GridUtilities
{
    public interface IGridObject
    {
        Vector2Int GridPosition { get; set; }
    }
}