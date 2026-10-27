using UnityEngine;
using UnityEngine.UI;

namespace LofiRoom
{
    public class MessageBanner : MonoBehaviour
    {
        public Text label;

        public CanvasGroup group;

        [Min(0.1f)] public float holdSeconds = 3.5f;
        [Min(0.05f)] public float fadeSeconds = 0.6f;

        float _timer;

        void Awake()
        {
            if (group != null) group.alpha = 0f;
        }

        void Update()
        {
            if (group == null) return;

            if (_timer > 0f)
            {
                _timer -= Time.unscaledDeltaTime;
                group.alpha = Mathf.MoveTowards(group.alpha, 1f, Time.unscaledDeltaTime / fadeSeconds);
            }
            else
            {
                group.alpha = Mathf.MoveTowards(group.alpha, 0f, Time.unscaledDeltaTime / fadeSeconds);
            }
        }

        public void Show(string message)
        {
            if (label != null) label.text = message;
            _timer = holdSeconds;
        }
    }
}
