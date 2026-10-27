using UnityEngine;
using UnityEngine.UI;

namespace LofiRoom
{
    public class MusicControls : MonoBehaviour
    {
        public RadioPlaylist playlist;

        public Button previousButton;
        public Button playPauseButton;
        public Button nextButton;

        public Image playPauseIcon;
        public Sprite playSprite;
        public Sprite pauseSprite;

        public Color disabledColor = new Color(1f, 1f, 1f, 0.25f);
        public Color normalColor = new Color(1f, 1f, 1f, 0.85f);

        void Awake()
        {
            ResolveIcon();
            if (previousButton != null) previousButton.onClick.AddListener(OnPrevious);
            if (playPauseButton != null) playPauseButton.onClick.AddListener(OnPlayPause);
            if (nextButton != null) nextButton.onClick.AddListener(OnNext);
        }

        void Update()
        {
            if (playlist == null) return;

            bool empty = playlist.TrackCount == 0;
            SetInteractable(!empty);

            bool playing = playlist.WantsToPlay;
            if (playPauseIcon == null) return;

            Sprite wanted = playing ? pauseSprite : playSprite;
            if (wanted != null && playPauseIcon.sprite != wanted) playPauseIcon.sprite = wanted;
        }

        void ResolveIcon()
        {
            Graphic background = playPauseButton != null ? playPauseButton.targetGraphic : null;
            if (playPauseIcon != null && playPauseIcon != background) return;
            if (playPauseButton == null) return;

            foreach (Image image in playPauseButton.GetComponentsInChildren<Image>(true))
            {
                if (image == background) continue;
                playPauseIcon = image;
                return;
            }
        }

        void SetInteractable(bool value)
        {
            SetOne(previousButton, value);
            SetOne(playPauseButton, value);
            SetOne(nextButton, value);
        }

        void SetOne(Button button, bool value)
        {
            if (button == null || button.interactable == value) return;

            button.interactable = value;

            var icon = button.GetComponentInChildren<Image>();
            if (icon != null && icon != button.targetGraphic)
                icon.color = value ? normalColor : disabledColor;
        }

        public void OnPrevious()
        {
            if (playlist != null) playlist.Previous();
        }

        public void OnNext()
        {
            if (playlist != null) playlist.Next();
        }

        public void OnPlayPause()
        {
            if (playlist != null) playlist.TogglePlayback();
        }
    }
}
