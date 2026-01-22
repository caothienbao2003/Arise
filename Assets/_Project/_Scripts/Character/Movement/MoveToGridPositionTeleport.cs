using GridTool;
using GridUtilities;
using UnityEngine;
using VContainer;

public class MoveToGridPositionTeleport : MonoBehaviour, IMoveToPosition
{
    [Inject] private IGridService gridService;
    
    public void SetMoveTargetPosition(Vector3 movePosition)
    {
        if (gridService == null)
        {
            Debug.LogWarning("[MoveToGridPositionTeleport] grid is null]");
            return;
        }
        
        transform.position = gridService.RuntimeGrid.GetCellCenterWorldPos(movePosition);
    }
}
