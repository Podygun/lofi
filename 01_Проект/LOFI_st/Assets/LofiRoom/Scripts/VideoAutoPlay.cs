using UnityEngine;
using UnityEngine.Video;

namespace LofiRoom
{
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoAutoPlay : MonoBehaviour
    {
        [Min(0.2f)] public float firstCheckDelay = 1.5f;

        [Min(0f)] public float watchInterval = 3f;

        [Min(0)] public int maxRetries = 3;

        public bool logRetries = true;

        VideoPlayer _player;
        float _nextCheck;
        int _retries;
        double _lastTime = -1d;

        void Awake()
        {
            _player = GetComponent<VideoPlayer>();

            _player.playOnAwake = false;
            _player.prepareCompleted += OnPrepared;
            _player.errorReceived += OnError;
        }

        void OnDestroy()
        {
            if (_player == null) return;
            _player.prepareCompleted -= OnPrepared;
            _player.errorReceived -= OnError;
        }

        void OnEnable()
        {
            Begin();
        }

        void Begin()
        {
            if (_player == null) return;
            if (_player.clip == null && string.IsNullOrEmpty(_player.url)) return;

            _nextCheck = Time.unscaledTime + firstCheckDelay;

            if (_player.isPrepared) _player.Play();
            else _player.Prepare();
        }

        void OnPrepared(VideoPlayer source)
        {
            source.Play();
        }

        void OnError(VideoPlayer source, string message)
        {
            Debug.LogWarning($"[VideoAutoPlay] {name}: {message}", this);
        }

        void Update()
        {
            if (_player == null || watchInterval <= 0f) return;
            if (_player.clip == null && string.IsNullOrEmpty(_player.url)) return;
            if (Time.unscaledTime < _nextCheck) return;

            _nextCheck = Time.unscaledTime + watchInterval;

            bool moving = _player.isPlaying && _player.time > _lastTime;
            _lastTime = _player.time;

            if (moving)
            {
                _retries = 0;
                return;
            }

            if (_retries >= maxRetries) return;

            _retries++;
            if (logRetries)
                Debug.LogWarning($"[VideoAutoPlay] {name}: видео стоит, перезапуск {_retries}.", this);

            _player.Stop();
            _player.Prepare();
        }
    }
}
