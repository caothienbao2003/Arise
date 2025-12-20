using CTB;
using GridTool;
using GridUtilities;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveToPositionPathfinding : MonoBehaviour, IMoveToPosition
{
    [SerializeField] private float reachPathPositionDistance = 0.5f;
    [SerializeField] private float finalSnapLerpSpeed = 50f;
    private int currentPathIndex = -1;

    private List<Vector3> pathPositionList;

    [ShowInInspector] private IMoveToDirection _moveToDirectionComponent;

    private IMoveToDirection moveToDirectionComponent => _moveToDirectionComponent ??= GetComponent<IMoveToDirection>();

    public void SetMoveTargetPosition(Vector3 targetPosition)
    {
        Debug.Log($"[MoveToPositionPathfinding] SetMoveTargetPosition] {targetPosition}");

        pathPositionList = GridManager.Instance.CurrentPathfinding.FindPath(transform.position, targetPosition);

        if (pathPositionList == null) return;

        if (pathPositionList.Count > 0)
        {
            currentPathIndex = 0;
        }
    }

    private void ProcessPath()
    {
        if (currentPathIndex != -1)
        {
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
        else
        {
            moveToDirectionComponent.SetMoveDirection(Vector3.zero);

            LerpToFinalTarget();
        }
    }
    private void LerpToFinalTarget()
    {
        Vector3 finalTarget = pathPositionList[pathPositionList.Count - 1];
        this.transform.position = Vector3.Lerp(this.transform.position, finalTarget, Time.deltaTime * finalSnapLerpSpeed);
        if (Vector3.Distance(transform.position, finalTarget) < 0.001f)
        {
            transform.position = finalTarget;
            pathPositionList = null; 
        }
    }

    private void Update()
    {
        ProcessPath();
    }
}