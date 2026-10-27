using UnityEngine;

namespace LofiRoom
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class ClickableProp : MonoBehaviour
    {
        public string title = "Предмет";

        public Color hoverTint = new Color(1f, 0.95f, 0.85f, 1f);

        public SpriteRenderer highlightTarget;

        public int clickPriority = 0;

        Color _baseColor;
        bool _cached;

        public string Title => title;

        protected virtual void Awake()
        {
            if (highlightTarget == null) highlightTarget = GetComponent<SpriteRenderer>();
            CacheColor();
        }

        void CacheColor()
        {
            if (highlightTarget != null && !_cached)
            {
                _baseColor = highlightTarget.color;
                _cached = true;
            }
        }

        public abstract void Interact();

        public virtual void SetHover(bool hovered)
        {
            CacheColor();
            if (highlightTarget == null) return;
            highlightTarget.color = hovered ? hoverTint : _baseColor;
        }

        protected void RefreshBaseColor()
        {
            if (highlightTarget != null) _baseColor = highlightTarget.color;
        }
    }
}
