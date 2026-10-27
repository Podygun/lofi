using UnityEngine;

namespace LofiRoom
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFlipbook : MonoBehaviour
    {
        public Sprite[] frames;

        [Min(0.1f)] public float framesPerSecond = 8f;

        public bool loop = true;

        public bool playOnStart = true;

        public bool randomStartFrame = false;

        [Min(0f)] public float pauseBetweenLoops = 0f;

        SpriteRenderer _renderer;
        float _timer;
        float _pauseLeft;
        int _index;

        public bool IsPlaying { get; private set; }

        void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            if (frames != null && frames.Length > 0 && randomStartFrame)
                _index = Random.Range(0, frames.Length);

            IsPlaying = playOnStart;
            Apply();
        }

        void Update()
        {
            if (!IsPlaying || frames == null || frames.Length < 2) return;

            if (_pauseLeft > 0f)
            {
                _pauseLeft -= Time.deltaTime;
                return;
            }

            _timer += Time.deltaTime;
            float step = 1f / framesPerSecond;

            while (_timer >= step)
            {
                _timer -= step;
                _index++;

                if (_index >= frames.Length)
                {
                    if (!loop)
                    {
                        _index = frames.Length - 1;
                        IsPlaying = false;
                        Apply();
                        return;
                    }

                    _index = 0;
                    if (pauseBetweenLoops > 0f) _pauseLeft = pauseBetweenLoops;
                }
                Apply();
            }
        }

        void Apply()
        {
            if (_renderer == null || frames == null || frames.Length == 0) return;
            if (frames[_index] != null) _renderer.sprite = frames[_index];
        }

        public void Play()
        {
            IsPlaying = true;
        }

        public void Stop()
        {
            IsPlaying = false;
        }

        public void Restart()
        {
            _index = 0;
            _timer = 0f;
            _pauseLeft = 0f;
            IsPlaying = true;
            Apply();
        }

        public void SetPlaying(bool value)
        {
            IsPlaying = value;
        }
    }
}
