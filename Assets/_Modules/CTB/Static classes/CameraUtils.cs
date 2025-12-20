using UnityEngine;
using UnityEngine.InputSystem;

namespace CTB
{
    public static class CameraUtils
    {
        public static Vector3 GetMouseWorldPosition2D(Vector2 mouseScreenPosition, float zDistance = 10f)
        {
            if (Camera.main == null) return Vector3.zero;
            return Camera.main.ScreenToWorldPoint(mouseScreenPosition) + Vector3.forward * zDistance;
        }
    }
}