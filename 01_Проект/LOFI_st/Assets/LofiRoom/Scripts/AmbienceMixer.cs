using System;
using System.Collections.Generic;
using UnityEngine;

namespace LofiRoom
{
    public class AmbienceMixer : MonoBehaviour
    {
        [Serializable]
        public class Track
        {
            public string id = "track";

            public string displayName = "Дорожка";

            public AudioSource source;

            [Range(0f, 1f)]
            public float targetVolume = 0.6f;

            public bool onAtStart;

            [NonSerialized] public bool isOn;
        }

        public List<Track> tracks = new List<Track>();

        [Range(0f, 1f)] public float masterVolume = 1f;

        [Min(0.05f)] public float fadeSpeed = 1.5f;

        public event Action MixChanged;

        public int TrackCount => tracks.Count;

        void Start()
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                var t = tracks[i];
                if (t.source == null)
                {
                    Debug.LogWarning($"[AmbienceMixer] У дорожки '{t.displayName}' не назначен AudioSource.", this);
                    continue;
                }

                t.source.loop = true;
                t.source.playOnAwake = false;
                t.source.volume = 0f;
                t.isOn = t.onAtStart;

                if (t.source.clip != null)
                    t.source.Play();
            }

            MixChanged?.Invoke();
        }

        void Update()
        {
            float step = fadeSpeed * Time.deltaTime;

            for (int i = 0; i < tracks.Count; i++)
            {
                var t = tracks[i];
                if (t.source == null) continue;

                float goal = t.isOn ? t.targetVolume * masterVolume : 0f;
                t.source.volume = Mathf.MoveTowards(t.source.volume, goal, step);
            }
        }

        public bool IsOn(int index)
        {
            return index >= 0 && index < tracks.Count && tracks[index].isOn;
        }

        public bool IsOn(string id)
        {
            int i = IndexOf(id);
            return i >= 0 && tracks[i].isOn;
        }

        public int IndexOf(string id)
        {
            for (int i = 0; i < tracks.Count; i++)
                if (string.Equals(tracks[i].id, id, StringComparison.OrdinalIgnoreCase))
                    return i;
            return -1;
        }

        public string DisplayName(int index)
        {
            return index >= 0 && index < tracks.Count ? tracks[index].displayName : string.Empty;
        }

        public void SetOn(int index, bool value)
        {
            if (index < 0 || index >= tracks.Count) return;
            if (tracks[index].isOn == value) return;

            tracks[index].isOn = value;
            MixChanged?.Invoke();
        }

        public void Toggle(int index)
        {
            if (index < 0 || index >= tracks.Count) return;
            tracks[index].isOn = !tracks[index].isOn;
            MixChanged?.Invoke();
        }

        public void ToggleById(string id)
        {
            Toggle(IndexOf(id));
        }

        public void MuteAll()
        {
            bool changed = false;
            for (int i = 0; i < tracks.Count; i++)
            {
                if (tracks[i].isOn) { tracks[i].isOn = false; changed = true; }
            }
            if (changed) MixChanged?.Invoke();
        }

        public int ActiveCount()
        {
            int n = 0;
            for (int i = 0; i < tracks.Count; i++)
                if (tracks[i].isOn) n++;
            return n;
        }
    }
}
