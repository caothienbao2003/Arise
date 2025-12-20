using UnityEngine;
using UnityEngine.InputSystem;

namespace CTB_Utils
{
    public static class CameraUtils
    {
        public static Vector3 GetMouseWorldPosition2D(float zDistance = 10f)
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current == null)
            {
                return Vector3.zero;
            }

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 mouseScreenPos = new Vector3(mousePos.x, mousePos.y, zDistance);
#else
            mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = zDistance;
#endif
            if (Camera.main != null)
                return Camera.main.ScreenToWorldPoint(mouseScreenPos);
            
            return default;
        }
    }
}