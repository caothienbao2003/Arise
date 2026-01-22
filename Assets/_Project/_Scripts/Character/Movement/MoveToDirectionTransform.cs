using System;
using UnityEngine;

public class MoveToDirectionTransform : MonoBehaviour, IMoveToDirection
{
    [SerializeField] private float moveSpeed;

    private Vector3 moveDirection;

    public void SetMoveDirection(Vector3 moveDirection)
    {
        Debug.Log($"[MovableTransform] SetMoveDirection] {moveDirection}");
        this.moveDirection = moveDirection;
        moveDirection.Normalize();
    }

    private void Update()
    {
        if (moveDirection == Vector3.zero) return;
        transform.position += moveDirection * (moveSpeed * Time.deltaTime);
    }
}