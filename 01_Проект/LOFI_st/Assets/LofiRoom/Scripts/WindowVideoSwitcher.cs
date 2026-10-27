using UnityEngine;
using UnityEngine.Video;

namespace LofiRoom
{
    public class WindowVideoSwitcher : ClickableProp
    {
        public VideoClip[] views;

        [TextArea(1, 2)]
        public string[] viewNames;

        public VideoPlayer player;

        public MessageBanner banner;
        public DiscoveryTracker tracker;

        public AudioClip clickSound;
        public AudioSource sfxSource;

        [TextArea(1, 3)]
        public string singleViewMessage = "Город внизу светится и не слышен. Стекло чуть запотело.";

        public string switchFormat = "За окном: {0}";

        int _index;
        Collider2D _collider;

        public bool CanSwitch => views != null && views.Length > 1;

        protected override void Awake()
        {
            base.Awake();
            _collider = GetComponent<Collider2D>();
        }

        void Start()
        {
            if (player == null)
                player = GetComponentInChildren<VideoPlayer>(true);

            if (!CanSwitch)
            {
                if (_collider != null) _collider.enabled = false;
                return;
            }

            if (tracker != null) tracker.Register(this);
            Apply(0);
        }

        public override void Interact()
        {
            if (!CanSwitch) return;

            if (tracker != null) tracker.MarkDiscovered(this);
            if (clickSound != null && sfxSource != null) sfxSource.PlayOneShot(clickSound);

            Apply((_index + 1) % views.Length);
        }

        void Apply(int index)
        {
            _index = index;

            if (player == null || views == null || index >= views.Length) return;
            if (views[index] == null) return;

            player.clip = views[index];
            player.isLooping = true;
            player.Play();

            if (banner != null) banner.Show(string.Format(switchFormat, NameOf(index)));
        }

        string NameOf(int index)
        {
            if (viewNames != null && index < viewNames.Length &&
                !string.IsNullOrWhiteSpace(viewNames[index]))
                return viewNames[index];

            return views[index] != null ? views[index].name : "другой вид";
        }
    }
}
