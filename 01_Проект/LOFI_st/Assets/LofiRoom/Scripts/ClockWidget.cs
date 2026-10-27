using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace LofiRoom
{
    public class ClockWidget : MonoBehaviour
    {
        public Text timeLabel;
        public Text dateLabel;

        public string timeFormat = "HH:mm";

        public string dateFormat = "dddd, d MMMM";

        public string culture = "ru-RU";

        public bool blinkSeparator = false;

        [Min(0.05f)] public float updateInterval = 1f;

        CultureInfo _culture;
        float _timer;

        void Awake()
        {
            _culture = string.IsNullOrWhiteSpace(culture)
                ? CultureInfo.CurrentCulture
                : CultureInfo.GetCultureInfo(culture);
        }

        void Start()
        {
            Refresh();
        }

        void Update()
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer < updateInterval) return;

            _timer = 0f;
            Refresh();
        }

        void Refresh()
        {
            DateTime now = DateTime.Now;

            if (timeLabel != null)
            {
                string text = now.ToString(timeFormat, _culture);

                if (blinkSeparator && now.Second % 2 == 1)
                    text = text.Replace(":", " ");

                timeLabel.text = text;
            }

            if (dateLabel != null)
                dateLabel.text = now.ToString(dateFormat, _culture);
        }
    }
}
