using CTB;
using Sirenix.OdinInspector;
using System;
using Sirenix.Serialization;
using UnityEngine;

public class PlayerMovementByMouse : MonoBehaviour
{
    [OdinSerialize]
    private IMoveToPosition _moveToPositionComponent;

    private IMoveToPosition moveToPositionComponent => _moveToPositionComponent ??= GetComponent<IMoveToPosition>();
    
    private InputReader _inputReader;
    private InputReader inputReader => _inputReader ??= InputReader.Instance;

    private void OnEnable()
    {
        inputReader.mouseActionEvent += HandleMouseAction;
    }

    private void OnDisable()
    {
        inputReader.mouseActionEvent -= HandleMouseAction;
    }

    private void HandleMouseAction(Vector2 mousePosition)
    {
        Vector2 moveTargetPosition = CameraUtils.GetMouseWorldPosition2D(mousePosition);
        moveToPositionComponent.SetMoveTargetPosition(moveTargetPosition);
    }
}