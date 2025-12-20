using CTB_Utils;
using GridTool;
using GridUtilities;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveToPositionPathfinding : MonoBehaviour, IMoveToPosition
{
    [SerializeField] private float reachPathPositionDistance = 0.1f;
    private int currentPathIndex = -1;

    private List<Vector3> pathPositionList;

    [ShowInInspector] private IMoveToDirection moveToDirectionComponent;

    private IMoveToDirection MoveToDirectionComponent => moveToDirectionComponent ??= GetComponent<IMoveToDirection>();

    private void Awake()
    {
        moveToDirectionComponent = GetComponent<IMoveToDirection>();
    }

    public void SetMoveTargetPosition(Vector3 targetPosition)
    {
        pathPositionList = GridManager.Instance.CurrentPathfinding.FindPath(transform.position, targetPosition);

        if (pathPositionList.Count > 0)
        {
            currentPathIndex = 0;
        }
    }

    private void StopMoving()
    {
        pathPositionList = null;
    }

    private void HandleMovement()
    {
        if (currentPathIndex != -1)
        {
            Vector3 nextPosition = pathPositionList[currentPathIndex];
            Vector3 moveDirection = (nextPosition - transform.position).normalized;
            MoveToDirectionComponent.SetMoveDirection(moveDirection);

            if (Vector3.Distance(transform.position, nextPosition) <= reachPathPositionDistance)
            {
                currentPathIndex++;
                if (currentPathIndex >= pathPositionList.Count)
                {
                    currentPathIndex = -1;
                }
            }
        }
        else
        {
            MoveToDirectionComponent.SetMoveDirection(Vector3.zero);
        }
    }
}