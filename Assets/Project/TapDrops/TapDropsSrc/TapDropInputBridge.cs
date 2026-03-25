using UnityEngine;
using UnityEngine.InputSystem;

namespace RainbowTower.TapDrops
{
    public sealed class TapDropInputBridge
    {
        public bool TryGetTapWorldPosition(Camera worldCamera, out Vector2 worldPosition)
        {
            worldPosition = default;
            if (worldCamera == null)
            {
                return false;
            }

            if (!TryGetTapScreenPosition(out var screenPosition))
            {
                return false;
            }

            var worldPoint = worldCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -worldCamera.transform.position.z));
            worldPosition = new Vector2(worldPoint.x, worldPoint.y);
            return true;
        }

        private static bool TryGetTapScreenPosition(out Vector2 screenPosition)
        {
            if (Touchscreen.current != null)
            {
                var touches = Touchscreen.current.touches;
                for (var index = 0; index < touches.Count; index++)
                {
                    var touch = touches[index];
                    if (!touch.press.wasPressedThisFrame)
                    {
                        continue;
                    }

                    screenPosition = touch.position.ReadValue();
                    return true;
                }
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screenPosition = Mouse.current.position.ReadValue();
                return true;
            }

            screenPosition = default;
            return false;
        }
    }
}
