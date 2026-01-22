using CTB;
using GridTool;
using GridUtilities;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;

public class MoveToPositionPathfinding : MonoBehaviour, IMoveToPosition
{
    [SerializeField] private PathfindingProfileSO pathfindingProfileSO;
    
    [SerializeField] private float reachPathPositionDistance = 0.5f;
    [SerializeField] private float finalSnapLerpSpeed = 50f;
    private int currentPathIndex = -1;

    private List<Vector3> pathPositionList;

    [OdinSerialize] private IMoveToDirection _moveToDirectionComponent;

    private bool finishLerped = false;
    
    private IMoveToDirection moveToDirectionComponent => _moveToDirectionComponent ??= GetComponent<IMoveToDirection>();
    
    [Inject] private IGridService gridService;

    public void SetMoveTargetPosition(Vector3 targetPosition)
    {
        if (gridService == null)
        {
            return;
        }
        
        List<Vector3> tempPathList = gridService.Pathfinding.FindPath(transform.position, targetPosition, pathfindingProfileSO);

        if (tempPathList == null)
        {
            return;
        }

        pathPositionList = tempPathList;
        if (pathPositionList == null) return;

        if (pathPositionList.Count > 0)
        {
            currentPathIndex = 0;
        }

        finishLerped = false;
    }

    private void ProcessPath()
    {
        if (currentPathIndex == -1 || pathPositionList == null || pathPositionList.Count == 0)
        {
            moveToDirectionComponent.SetMoveDirection(Vector3.zero);
            LerpToFinalTarget();
            return;
        }

        Vector3 nextPosition = pathPositionList[currentPathIndex];
        Vector3 moveDirection = (nextPosition - transform.position).normalized;
        moveToDirectionComponent.SetMoveDirection(moveDirection);

        if (Vector3.Distance(transform.position, nextPosition) <= reachPathPositionDistance)
        {
            currentPathIndex++;
            if (currentPathIndex >= pathPositionList.Count)
            {
                currentPathIndex = -1;
            }
        }
    }

    private void LerpToFinalTarget()
    {
        if (finishLerped) return;
        if (pathPositionList == null || pathPositionList.Count <= 0)
        {
            return;
        }

        Vector3 finalTarget = pathPositionList[pathPositionList.Count - 1];
        this.transform.position =
            Vector3.Lerp(this.transform.position, finalTarget, Time.deltaTime * finalSnapLerpSpeed);
        if (Vector3.Distance(transform.position, finalTarget) < 0.001f)
        {
            transform.position = finalTarget;
            pathPositionList = null;
            finishLerped = true;
        }
    }

    private void Update()
    {
        ProcessPath();
    }
}