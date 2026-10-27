using UnityEngine;
using UnityEngine.UI;

namespace LofiRoom
{
    [RequireComponent(typeof(Button))]
    public class SoundToggleButton : MonoBehaviour
    {
        public AmbienceMixer mixer;

        public int trackIndex;

        public Image background;

        public Image icon;

        public Text label;

        public Image stateDot;

        public Color onColor = new Color(0.85f, 0.65f, 0.45f, 0.95f);
        public Color offColor = new Color(0.22f, 0.21f, 0.28f, 0.80f);
        public Color onTextColor = new Color(0.14f, 0.12f, 0.16f, 1f);
        public Color offTextColor = new Color(0.85f, 0.83f, 0.88f, 1f);

        public Color iconOnColor = new Color(0.95f, 0.76f, 0.52f, 1f);
        public Color iconOffColor = new Color(1f, 1f, 1f, 0.45f);

        Button _button;

        void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        void OnEnable()
        {
            if (mixer != null) mixer.MixChanged += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            if (mixer != null) mixer.MixChanged -= Refresh;
        }

        void OnClick()
        {
            if (mixer == null) return;
            mixer.Toggle(trackIndex);
        }

        public void Refresh()
        {
            if (mixer == null) return;

            bool on = mixer.IsOn(trackIndex);

            if (background != null) background.color = on ? onColor : offColor;
            if (stateDot != null) stateDot.color = on ? onColor : new Color(offColor.r, offColor.g, offColor.b, 0.5f);
            if (icon != null) icon.color = on ? iconOnColor : iconOffColor;

            if (label != null)
            {
                label.color = on ? onTextColor : offTextColor;
                string title = mixer.DisplayName(trackIndex);
                if (!string.IsNullOrEmpty(title)) label.text = title;
            }
        }
    }
}
