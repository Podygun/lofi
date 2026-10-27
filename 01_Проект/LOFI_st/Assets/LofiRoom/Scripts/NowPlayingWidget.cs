using System;
using UnityEngine;
using UnityEngine.UI;

namespace LofiRoom
{
    public class NowPlayingWidget : MonoBehaviour
    {
        public RadioPlaylist playlist;

        public Text captionLabel;

        public Text trackLabel;

        public Image progressBar;

        public Text compactLabel;

        public string caption = "сейчас играет:";
        public string idleCaption = "радио выключено";
        public string idleTrack = "—";
        public string emptyPlaylistTrack = "положите музыку в Resources/Music";

        public bool prettifyName = true;

        [Min(0)] public int maxNameLength = 42;

        [Range(6, 32)] public int compactNameLength = 14;

        public bool smoothProgress = true;
        [Min(0.5f)] public float smoothSpeed = 8f;

        float _shownProgress;

        void Start()
        {
            if (captionLabel != null) captionLabel.text = caption;
        }

        void Update()
        {
            RefreshCompactLabel();
            if (playlist == null) return;

            bool empty = playlist.TrackCount == 0;
            bool playing = playlist.IsPlaying;

            if (captionLabel != null)
                captionLabel.text = playing
                    ? caption
                    : playlist.IsPaused && playlist.CurrentClip != null
                        ? "радио на паузе"
                        : idleCaption;

            if (trackLabel != null)
            {
                if (empty) trackLabel.text = emptyPlaylistTrack;
                else if (playlist.CurrentClip == null) trackLabel.text = idleTrack;
                else trackLabel.text = Prettify(playlist.CurrentTrackName);
            }

            if (progressBar != null)
            {
                float target = playing ? playlist.Progress01 : 0f;

                _shownProgress = smoothProgress
                    ? Mathf.Lerp(_shownProgress, target, Time.unscaledDeltaTime * smoothSpeed)
                    : target;

                if (target < _shownProgress - 0.25f) _shownProgress = target;

                progressBar.fillAmount = _shownProgress;
            }
        }

        void RefreshCompactLabel()
        {
            if (compactLabel == null) return;

            string track = playlist == null || playlist.TrackCount == 0
                ? "нет музыки"
                : playlist.CurrentClip == null
                    ? "радио выключено"
                    : Prettify(playlist.CurrentTrackName);

            int limit = Mathf.Max(6, compactNameLength);
            if (track.Length > limit)
                track = track.Substring(0, limit - 1).TrimEnd() + "…";

            compactLabel.text = DateTime.Now.ToString("HH:mm") + "\n♫ " + track;
        }

        string Prettify(string name)
        {
            if (string.IsNullOrEmpty(name)) return idleTrack;

            string result = name;

            if (prettifyName)
                result = result.Replace('_', ' ').Replace('-', '–').Trim();

            if (maxNameLength > 0 && result.Length > maxNameLength)
                result = result.Substring(0, maxNameLength - 1).TrimEnd() + "…";

            return result;
        }
    }
}
