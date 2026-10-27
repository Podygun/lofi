using UnityEngine;
using UnityEngine.Video;

namespace LofiRoom
{
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoFollowsToggle : MonoBehaviour
    {
        public ToggleableProp toggleSource;

        public AmbienceMixer mixer;

        public string trackId = "";

        public bool pauseInsteadOfStop = true;

        public bool hideRendererWhenOff = false;

        VideoPlayer _player;
        Renderer _renderer;
        bool _lastState;
        bool _initialised;

        void Awake()
        {
            _player = GetComponent<VideoPlayer>();
            _renderer = GetComponent<Renderer>();
        }

        void Update()
        {
            bool wanted = ShouldPlay();

            if (_initialised && wanted == _lastState) return;

            _initialised = true;
            _lastState = wanted;
            Apply(wanted);
        }

        bool ShouldPlay()
        {
            if (toggleSource != null) return toggleSource.IsOn;
            if (mixer != null && !string.IsNullOrEmpty(trackId)) return mixer.IsOn(trackId);
            return true;
        }

        void Apply(bool play)
        {
            if (_player.clip == null && string.IsNullOrEmpty(_player.url))
                return;

            if (play) _player.Play();
            else if (pauseInsteadOfStop) _player.Pause();
            else _player.Stop();

            if (hideRendererWhenOff && _renderer != null)
                _renderer.enabled = play;
        }
    }
}
