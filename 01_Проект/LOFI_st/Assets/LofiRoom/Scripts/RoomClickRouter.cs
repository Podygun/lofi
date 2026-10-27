using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace LofiRoom
{
    public class RoomClickRouter : MonoBehaviour
    {
        public Camera worldCamera;

        public UnityEngine.UI.Text hoverLabel;

        public LayerMask interactableLayers = ~0;

        ClickableProp _hovered;

        void Awake()
        {
            if (worldCamera == null) worldCamera = Camera.main;
        }

        void Update()
        {
            if (worldCamera == null) return;

            Vector2? screenPoint = CurrentPointer();
            if (screenPoint == null)
            {
                SetHovered(null);
                return;
            }

            bool overUI = EventSystem.current != null &&
                          EventSystem.current.IsPointerOverGameObject();

            ClickableProp found = overUI ? null : Pick(screenPoint.Value);
            SetHovered(found);

            if (!overUI && found != null && WasClicked())
                found.Interact();
        }

        Vector2? CurrentPointer()
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
                return Touchscreen.current.primaryTouch.position.ReadValue();

            if (Mouse.current != null)
                return Mouse.current.position.ReadValue();

            return null;
        }

        bool WasClicked()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
            return false;
        }

        ClickableProp Pick(Vector2 screenPoint)
        {
            Vector3 world = worldCamera.ScreenToWorldPoint(screenPoint);

            Collider2D[] hits = Physics2D.OverlapPointAll(world, interactableLayers);
            if (hits == null || hits.Length == 0) return null;

            ClickableProp best = null;
            int bestPriority = int.MinValue;

            for (int i = 0; i < hits.Length; i++)
            {
                var prop = hits[i].GetComponentInParent<ClickableProp>();
                if (prop == null) continue;

                if (best == null || prop.clickPriority > bestPriority)
                {
                    best = prop;
                    bestPriority = prop.clickPriority;
                }
            }

            return best;
        }

        void SetHovered(ClickableProp next)
        {
            if (_hovered == next) return;

            if (_hovered != null) _hovered.SetHover(false);
            _hovered = next;
            if (_hovered != null) _hovered.SetHover(true);

            if (hoverLabel != null)
                hoverLabel.text = _hovered != null ? _hovered.title : string.Empty;
        }
    }
}
