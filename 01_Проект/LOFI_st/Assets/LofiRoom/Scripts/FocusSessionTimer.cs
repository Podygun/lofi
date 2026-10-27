using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LofiRoom
{
    public class FocusSessionTimer : MonoBehaviour
    {
        [Serializable]
        public class Milestone
        {
            [Min(0.1f)] public float minutes = 5f;

            public string message = "5 минут фокуса. Хорошее начало.";

            [NonSerialized] public bool reached;
        }

        public Button focusButton;

        public Text focusButtonLabel;

        public Text sessionTimerLabel;

        public Text totalMinutesLabel;

        public Image progressBar;

        public MessageBanner banner;

        public string startLabel = "Focus Mode";
        public string stopLabel = "Завершить сессию";

        public List<Milestone> milestones = new List<Milestone>();

        public string autoEnableTrackId = "";
        public AmbienceMixer mixer;

        public bool IsRunning { get; private set; }
        public float SessionSeconds { get; private set; }
        public float TotalSeconds { get; private set; }

        public event Action<bool> FocusStateChanged;

        static readonly Color IdleColor = new Color(0.25f, 0.16f, 0.085f, 0.97f);

        static readonly Color ActiveColor = new Color(0.56f, 0.34f, 0.13f, 0.98f);

        Image _buttonImage;

        void Awake()
        {
            if (focusButton != null)
            {
                _buttonImage = focusButton.GetComponent<Image>();
                focusButton.onClick.AddListener(ToggleFocus);
            }
        }

        void Start()
        {
            RefreshVisual();
            RefreshLabels();
        }

        void Update()
        {
            if (!IsRunning) return;

            SessionSeconds += Time.deltaTime;
            TotalSeconds += Time.deltaTime;

            CheckMilestones();
            RefreshLabels();
        }

        public void ToggleFocus()
        {
            if (IsRunning) StopFocus();
            else StartFocus();
        }

        public void StartFocus()
        {
            IsRunning = true;
            SessionSeconds = 0f;

            for (int i = 0; i < milestones.Count; i++)
                milestones[i].reached = false;

            if (mixer != null && !string.IsNullOrEmpty(autoEnableTrackId))
            {
                int idx = mixer.IndexOf(autoEnableTrackId);
                if (idx >= 0) mixer.SetOn(idx, true);
            }

            if (banner != null) banner.Show("Сессия фокуса началась.");
            FocusStateChanged?.Invoke(true);
            RefreshVisual();
            RefreshLabels();
        }

        public void StopFocus()
        {
            IsRunning = false;

            int minutes = Mathf.FloorToInt(SessionSeconds / 60f);
            if (banner != null)
            {
                banner.Show(minutes > 0
                    ? $"Сессия завершена: {minutes} мин. Всего: {Mathf.FloorToInt(TotalSeconds / 60f)} мин."
                    : "Сессия завершена. Меньше минуты — тоже считается.");
            }

            FocusStateChanged?.Invoke(false);
            RefreshVisual();
            RefreshLabels();
        }

        void CheckMilestones()
        {
            for (int i = 0; i < milestones.Count; i++)
            {
                var m = milestones[i];
                if (m.reached) continue;
                if (SessionSeconds >= m.minutes * 60f)
                {
                    m.reached = true;
                    if (banner != null) banner.Show(m.message);
                }
            }
        }

        void RefreshVisual()
        {
            if (_buttonImage != null) _buttonImage.color = IsRunning ? ActiveColor : IdleColor;
            if (focusButtonLabel != null) focusButtonLabel.text = IsRunning ? stopLabel : startLabel;
        }

        void RefreshLabels()
        {
            if (sessionTimerLabel != null)
            {
                var span = TimeSpan.FromSeconds(SessionSeconds);
                sessionTimerLabel.text = $"{(int)span.TotalMinutes:00}:{span.Seconds:00}";
            }

            if (totalMinutesLabel != null)
                totalMinutesLabel.text = $"Всего в комнате: {Mathf.FloorToInt(TotalSeconds / 60f)} мин";

            if (progressBar != null)
                progressBar.fillAmount = NextMilestoneProgress();
        }

        float NextMilestoneProgress()
        {
            float prev = 0f;
            for (int i = 0; i < milestones.Count; i++)
            {
                float goal = milestones[i].minutes * 60f;
                if (SessionSeconds < goal)
                {
                    float span = Mathf.Max(0.001f, goal - prev);
                    return Mathf.Clamp01((SessionSeconds - prev) / span);
                }
                prev = goal;
            }
            return 1f;
        }
    }
}
