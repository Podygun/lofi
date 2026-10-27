using UnityEngine;

namespace LofiRoom
{
    public class ToggleableProp : ClickableProp
    {
        public bool startsOn = false;

        public Sprite onSprite;

        public Sprite offSprite;

        public SpriteRenderer targetRenderer;

        public bool keepVisualSize = true;

        public bool anchorBottom = true;

        public FitAxis fitBy = FitAxis.Width;

        public enum FitAxis
        {
            Width,
            Height
        }

        public GameObject glowObject;

        public SpriteFlipbook flipbook;

        public AudioSource loopSource;

        [Range(0f, 1f)] public float loopVolume = 0.5f;

        [Min(0.05f)] public float fadeSpeed = 2f;

        public AudioClip clickSound;
        public AudioSource sfxSource;

        public string trackId = "";
        public AmbienceMixer mixer;

        public bool syncTrackOnStart = false;

        public MessageBanner banner;
        public DiscoveryTracker tracker;

        [TextArea(1, 3)] public string onMessage = "Включено.";
        [TextArea(1, 3)] public string offMessage = "Выключено.";

        public bool IsOn { get; private set; }

        Vector2 _visualSize;
        float _bottomY;
        float _centerX;

        protected override void Awake()
        {
            if (targetRenderer == null) targetRenderer = GetComponent<SpriteRenderer>();
            if (highlightTarget == null) highlightTarget = targetRenderer;
            base.Awake();

            if (targetRenderer != null && targetRenderer.sprite != null)
            {
                Vector2 spriteSize = targetRenderer.sprite.bounds.size;
                _visualSize = new Vector2(spriteSize.x * transform.localScale.x,
                                          spriteSize.y * transform.localScale.y);
                _bottomY = transform.position.y - _visualSize.y * 0.5f;
                _centerX = transform.position.x;
            }
        }

        void Start()
        {
            if (tracker != null) tracker.Register(this);

            IsOn = startsOn;

            if (loopSource != null)
            {
                loopSource.loop = true;
                loopSource.playOnAwake = false;
                loopSource.volume = 0f;
                if (loopSource.clip != null) loopSource.Play();
            }

            ApplyState(silent: true, touchMixer: syncTrackOnStart);
        }

        void Update()
        {
            if (loopSource == null) return;
            float goal = IsOn ? loopVolume : 0f;
            loopSource.volume = Mathf.MoveTowards(loopSource.volume, goal, fadeSpeed * Time.deltaTime);
        }

        public override void Interact()
        {
            IsOn = !IsOn;

            if (tracker != null) tracker.MarkDiscovered(this);
            if (clickSound != null && sfxSource != null) sfxSource.PlayOneShot(clickSound);

            ApplyState(silent: false, touchMixer: true);
        }

        public void SetOn(bool value)
        {
            if (IsOn == value) return;
            IsOn = value;
            ApplyState(silent: false, touchMixer: true);
        }

        void ApplyState(bool silent, bool touchMixer)
        {
            Sprite wanted = IsOn ? onSprite : offSprite;
            if (targetRenderer != null && wanted != null)
            {
                targetRenderer.sprite = wanted;
                KeepSize(wanted);
            }

            if (glowObject != null) glowObject.SetActive(IsOn);

            if (flipbook != null) flipbook.SetPlaying(IsOn);

            if (touchMixer && mixer != null && !string.IsNullOrEmpty(trackId))
            {
                int idx = mixer.IndexOf(trackId);
                if (idx >= 0) mixer.SetOn(idx, IsOn);
            }

            RefreshBaseColor();

            if (!silent && banner != null)
                banner.Show(IsOn ? onMessage : offMessage);
        }

        void KeepSize(Sprite sprite)
        {
            if (!keepVisualSize || sprite == null) return;
            if (_visualSize.x <= 0f || _visualSize.y <= 0f) return;

            Vector2 spriteSize = sprite.bounds.size;
            if (spriteSize.x <= 0f || spriteSize.y <= 0f) return;

            float scale = fitBy == FitAxis.Width
                ? _visualSize.x / spriteSize.x
                : _visualSize.y / spriteSize.y;

            transform.localScale = new Vector3(scale, scale, 1f);

            Vector3 position = transform.position;
            position.x = _centerX;
            if (anchorBottom) position.y = _bottomY + spriteSize.y * scale * 0.5f;
            transform.position = position;
        }
    }
}
