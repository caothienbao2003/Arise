using CTB;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/InputReader")]
public class InputReader : ScriptableSingleton<InputReader>, PlayerInputActions.IGameplayActions
{
    #region Singleton
    private const string RESOURCE_PATH = "Settings/Input/InputReader";
    public static InputReader Instance => GetInstance(RESOURCE_PATH);
    #endregion
    
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