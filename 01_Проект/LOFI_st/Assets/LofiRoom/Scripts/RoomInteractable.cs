using System.Collections;
using UnityEngine;

namespace LofiRoom
{
    public class RoomInteractable : ClickableProp
    {
        [TextArea(2, 4)]
        public string message = "Что-то тёплое и знакомое.";

        [TextArea(1, 3)]
        public string[] extraMessages;

        public AudioClip[] clickSounds;
        public AudioSource sfxSource;
        [Range(0f, 1f)] public float clickVolume = 0.8f;

        public AudioSource loopSource;
        [Range(0f, 1f)] public float loopVolume = 0.6f;
        [Min(0.05f)] public float loopFadeSpeed = 1.5f;

        [Min(0f)] public float loopDuration = 0f;

        public SpriteFlipbook reactionFlipbook;

        public bool reactionFollowsLoop = false;

        [Range(0f, 0.4f)] public float punchScale = 0.07f;
        [Min(0.05f)] public float punchDuration = 0.25f;

        public MessageBanner banner;
        public DiscoveryTracker tracker;

        public string toggleTrackId = "";
        public AmbienceMixer mixer;

        Vector3 _baseScale;
        int _messageCursor;
        Coroutine _punch;
        bool _loopOn;
        float _loopLeft;

        public bool Discovered { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            _baseScale = transform.localScale;
        }

        void Start()
        {
            if (tracker != null) tracker.Register(this);

            if (loopSource != null)
            {
                loopSource.loop = false;
                loopSource.playOnAwake = false;
                loopSource.pitch = 1f;
                loopSource.volume = 0f;
                loopSource.Stop();
            }

            if (reactionFlipbook != null && reactionFollowsLoop)
                reactionFlipbook.SetPlaying(false);
        }

        void Update()
        {
            if (loopSource == null) return;

            if (_loopOn && loopDuration > 0f)
            {
                _loopLeft -= Time.unscaledDeltaTime;
                if (_loopLeft <= 0f) SetLoop(false);
            }

            if (_loopOn && !loopSource.loop && !loopSource.isPlaying)
                SetLoop(false);

            float goal = _loopOn ? loopVolume : 0f;
            loopSource.volume = Mathf.MoveTowards(
                loopSource.volume, goal, loopFadeSpeed * Time.unscaledDeltaTime);

            if (!_loopOn && loopSource.volume <= 0.001f && loopSource.isPlaying)
                loopSource.Stop();
        }

        public override void Interact()
        {
            if (!Discovered)
            {
                Discovered = true;
                if (tracker != null) tracker.MarkDiscovered(this);
            }

            if (banner != null) banner.Show(NextMessage());

            PlayClickSound();

            if (loopSource != null) SetLoop(!_loopOn);

            if (reactionFlipbook != null && !reactionFollowsLoop)
                reactionFlipbook.Restart();

            if (mixer != null && !string.IsNullOrEmpty(toggleTrackId))
                mixer.ToggleById(toggleTrackId);

            if (_punch != null) StopCoroutine(_punch);
            _punch = StartCoroutine(Punch());
        }

        void SetLoop(bool value)
        {
            _loopOn = value;
            _loopLeft = value ? loopDuration : 0f;

            if (value && loopSource != null && !loopSource.isPlaying && loopSource.clip != null)
                loopSource.Play();

            if (reactionFlipbook != null && reactionFollowsLoop)
                reactionFlipbook.SetPlaying(value);
        }

        void PlayClickSound()
        {
            if (sfxSource == null || clickSounds == null || clickSounds.Length == 0) return;

            AudioClip clip = clickSounds[Random.Range(0, clickSounds.Length)];
            if (clip != null) sfxSource.PlayOneShot(clip, clickVolume);
        }

        string NextMessage()
        {
            if (extraMessages == null || extraMessages.Length == 0) return message;

            if (_messageCursor == 0)
            {
                _messageCursor = 1;
                return message;
            }

            string result = extraMessages[(_messageCursor - 1) % extraMessages.Length];
            _messageCursor++;
            return result;
        }

        IEnumerator Punch()
        {
            float t = 0f;
            while (t < punchDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / punchDuration));
                transform.localScale = _baseScale * (1f + punchScale * k);
                yield return null;
            }
            transform.localScale = _baseScale;
            _punch = null;
        }
    }
}
