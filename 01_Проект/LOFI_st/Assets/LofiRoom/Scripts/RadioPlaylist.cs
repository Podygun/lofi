using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LofiRoom
{
    [RequireComponent(typeof(AudioSource))]
    public class RadioPlaylist : MonoBehaviour
    {
        public string resourcesFolder = "Music";

        public List<AudioClip> extraClips = new List<AudioClip>();

        public bool shuffle = true;

        public bool reshuffleEachLoop = true;

        public bool loopPlaylist = true;

        [Min(0f)] public float gapSeconds = 1.2f;

        [Range(0f, 1f)] public float volume = 0.5f;

        [Min(0.05f)] public float fadeSpeed = 1.2f;

        public ToggleableProp powerSource;

        public MessageBanner banner;

        public Text nowPlayingLabel;

        public bool announceTracks = false;

        public string nowPlayingFormat = "♪ {0}";
        public string emptyPlaylistText = "Плейлист пуст";

        readonly List<AudioClip> _playlist = new List<AudioClip>();
        AudioSource _source;
        int _index = -1;
        float _gapLeft;
        bool _wasPlaying;

        public int TrackCount => _playlist.Count;
        public AudioClip CurrentClip => _index >= 0 && _index < _playlist.Count ? _playlist[_index] : null;

        public string CurrentTrackName => CurrentClip != null ? CurrentClip.name : string.Empty;

        public bool IsPlaying => _source != null && _source.isPlaying;

        public float Progress01
        {
            get
            {
                if (_source == null || _source.clip == null || _source.clip.length <= 0f) return 0f;
                return Mathf.Clamp01(_source.time / _source.clip.length);
            }
        }

        public int CurrentTrackNumber => _index >= 0 ? _index + 1 : 0;

        public bool IsPaused { get; private set; }

        public bool WantsToPlay => (powerSource == null || powerSource.IsOn) && !IsPaused;

        void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.loop = false;
            _source.playOnAwake = false;
            _source.volume = 0f;
        }

        void Start()
        {
            BuildPlaylist();

            if (_playlist.Count == 0)
            {
                Debug.LogWarning($"[RadioPlaylist] В папке Resources/{resourcesFolder} нет аудиофайлов. " +
                                 "Положите туда свои треки — они подхватятся автоматически.", this);
                SetLabel(emptyPlaylistText);
                return;
            }

            if (shuffle) Shuffle();
            SetLabel("");
        }

        void Update()
        {
            bool wantPlay = WantsToPlay;

            if (wantPlay && !_wasPlaying) StartPlaying();
            else if (!wantPlay && _wasPlaying) StopPlaying();

            _wasPlaying = wantPlay;

            float goal = wantPlay ? volume : 0f;
            _source.volume = Mathf.MoveTowards(_source.volume, goal, fadeSpeed * Time.deltaTime);

            if (!wantPlay || _playlist.Count == 0) return;

            if (_gapLeft > 0f)
            {
                _gapLeft -= Time.deltaTime;
                if (_gapLeft <= 0f) Next();
                return;
            }

            if (!_source.isPlaying && _source.clip != null)
                _gapLeft = Mathf.Max(0.01f, gapSeconds);
        }

        public void BuildPlaylist()
        {
            _playlist.Clear();

            if (!string.IsNullOrWhiteSpace(resourcesFolder))
            {
                AudioClip[] found = Resources.LoadAll<AudioClip>(resourcesFolder);
                if (found != null)
                {
                    System.Array.Sort(found, (a, b) =>
                        string.Compare(a != null ? a.name : string.Empty,
                                       b != null ? b.name : string.Empty,
                                       System.StringComparison.OrdinalIgnoreCase));
                    _playlist.AddRange(found);
                }
            }

            foreach (var clip in extraClips)
                if (clip != null && !_playlist.Contains(clip))
                    _playlist.Add(clip);
        }

        void Shuffle()
        {
            for (int i = _playlist.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (_playlist[i], _playlist[j]) = (_playlist[j], _playlist[i]);
            }
        }

        void StartPlaying()
        {
            if (_playlist.Count == 0) return;

            if (_source.clip == null) Next();
            else if (!_source.isPlaying) _source.Play();
        }

        void StopPlaying()
        {
            _source.Pause();
            _gapLeft = 0f;
        }

        public void TogglePlayback()
        {
            if (powerSource != null && !powerSource.IsOn)
            {
                IsPaused = false;
                powerSource.SetOn(true);
                return;
            }

            IsPaused = !IsPaused;
        }

        public void SetPaused(bool value)
        {
            IsPaused = value;
        }

        public void Next()
        {
            if (_playlist.Count == 0) return;

            IsPaused = false;
            _index++;

            if (_index >= _playlist.Count)
            {
                if (!loopPlaylist)
                {
                    _index = _playlist.Count - 1;
                    _source.Stop();
                    return;
                }

                _index = 0;
                if (shuffle && reshuffleEachLoop) Shuffle();
            }

            PlayCurrent();
        }

        public void Previous()
        {
            if (_playlist.Count == 0) return;

            IsPaused = false;
            _index--;
            if (_index < 0) _index = _playlist.Count - 1;

            PlayCurrent();
        }

        void PlayCurrent()
        {
            AudioClip clip = CurrentClip;
            if (clip == null) return;

            _gapLeft = 0f;
            _source.clip = clip;
            _source.time = 0f;
            _source.Play();

            SetLabel(string.Format(nowPlayingFormat, clip.name));

            if (announceTracks && banner != null)
                banner.Show(string.Format(nowPlayingFormat, clip.name));
        }

        void SetLabel(string text)
        {
            if (nowPlayingLabel != null) nowPlayingLabel.text = text;
        }
    }
}
