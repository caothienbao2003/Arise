using System;
using UnityEngine;

public class MoveToToDirectionPhysics : MonoBehaviour, IMoveToDirection
{
    [SerializeField] private float moveSpeed;

    private Vector3 moveDirection;
    
    private Rigidbody2D _rigidbody2D;
    
    private Rigidbody2D rb => _rigidbody2D ??= GetComponent<Rigidbody2D>();

    public void SetMoveDirection(Vector3 moveDirection)
    {
        this.moveDirection = moveDirection;
        this.moveDirection.Normalize();
    }
    
    private void FixedUpdate()
    {
        if(moveDirection == Vector3.zero) return;
        rb.linearVelocity = moveDirection * moveSpeed;
    }
}
