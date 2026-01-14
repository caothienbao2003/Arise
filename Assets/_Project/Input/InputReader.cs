using CTB;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, PlayerInputActions.IGameplayActions
{
    public Action<Vector2> mouseActionEvent { get; set; }
    public Action mousePanEvent { get; set; }
    public Vector2 mousePosition { get; set; }
    public Action<Vector2> cameraMovementEvent { get; set; }
    
    private PlayerInputActions playerInputActions;

    private void OnEnable()
    {
        if (playerInputActions == null)
        {
            playerInputActions = new PlayerInputActions();
            playerInputActions.Gameplay.SetCallbacks(this);
        }

        playerInputActions.Gameplay.Enable();
    }
    private void OnDisable()
    {
        playerInputActions?.Gameplay.Disable();
    }

    public void OnMouseAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            mouseActionEvent?.Invoke(mousePosition);
        }
    }

    public void OnMousePan(InputAction.CallbackContext context)
    {
        
    }
    public void OnMouseZoom(InputAction.CallbackContext context)
    {
        
    }
    public void OnMousePosition(InputAction.CallbackContext context)
    {
        mousePosition = context.ReadValue<Vector2>();
    }
    public void OnCameraMovement(InputAction.CallbackContext context)
    {
        
    }
}