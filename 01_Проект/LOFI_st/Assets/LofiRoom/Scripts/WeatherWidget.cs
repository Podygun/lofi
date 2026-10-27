using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace LofiRoom
{
    public class WeatherWidget : MonoBehaviour
    {
        [Serializable]
        public class WeatherIcons
        {
            public Sprite clear;
            public Sprite cloudy;
            public Sprite rain;
            public Sprite snow;
        }

        public double latitude = 55.7558;

        public double longitude = 37.6173;

        public string cityName = "";

        [Min(1f)] public float refreshMinutes = 15f;

        public bool fetchOnStart = true;

        [Min(1)] public int timeoutSeconds = 10;

        public Image iconImage;
        public Text temperatureLabel;
        public Text conditionLabel;

        public WeatherIcons icons = new WeatherIcons();

        public string temperatureFormat = "{0}°C";

        public string loadingText = "загрузка…";

        public string offlineText = "нет связи";

        public float Temperature { get; private set; }
        public string Condition { get; private set; } = "";
        public bool HasData { get; private set; }

        Coroutine _loop;

        [Serializable]
        class CurrentBlock
        {
            public float temperature_2m;
            public int weather_code;
        }

        [Serializable]
        class ForecastResponse
        {
            public CurrentBlock current;
        }

        [Serializable] class MetResponse { public MetProperties properties; }
        [Serializable] class MetProperties { public MetStep[] timeseries; }
        [Serializable] class MetStep { public MetData data; }
        [Serializable] class MetData { public MetInstant instant; public MetPeriod next_1_hours; public MetPeriod next_6_hours; }
        [Serializable] class MetInstant { public MetDetails details; }
        [Serializable] class MetDetails { public float air_temperature; }
        [Serializable] class MetPeriod { public MetSummary summary; }
        [Serializable] class MetSummary { public string symbol_code; }

        const string UserAgent = "LofiRoom/1.0 unity-lofi-room";

        void Start()
        {
            if (conditionLabel != null) conditionLabel.text = loadingText;
            if (temperatureLabel != null) temperatureLabel.text = "";

            if (fetchOnStart) _loop = StartCoroutine(RefreshLoop());
        }

        void OnDisable()
        {
            if (_loop != null) StopCoroutine(_loop);
            _loop = null;
        }

        IEnumerator RefreshLoop()
        {
            while (true)
            {
                yield return Fetch();
                yield return new WaitForSecondsRealtime(refreshMinutes * 60f);
            }
        }

        public void RefreshNow()
        {
            StartCoroutine(Fetch());
        }

        IEnumerator Fetch()
        {
            bool ok = false;

            yield return FetchMetNo(result => ok = result);
            if (ok) yield break;

            yield return FetchOpenMeteo(result => ok = result);
            if (ok) yield break;

            if (!HasData && conditionLabel != null) conditionLabel.text = offlineText;
        }

        IEnumerator FetchMetNo(Action<bool> done)
        {
            string url = string.Format(CultureInfo.InvariantCulture,
                "https://api.met.no/weatherapi/locationforecast/2.0/compact?lat={0:F4}&lon={1:F4}",
                latitude, longitude);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = timeoutSeconds;
                request.SetRequestHeader("User-Agent", UserAgent);
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[WeatherWidget] MET Norway: {request.error}", this);
                    done(false);
                    yield break;
                }

                MetResponse data = null;
                try { data = JsonUtility.FromJson<MetResponse>(request.downloadHandler.text); }
                catch (Exception e) { Debug.LogWarning($"[WeatherWidget] MET Norway: {e.Message}", this); }

                MetStep[] series = data?.properties?.timeseries;
                MetData first = series != null && series.Length > 0 ? series[0].data : null;
                if (first == null || first.instant == null || first.instant.details == null)
                {
                    done(false);
                    yield break;
                }

                string symbol = first.next_1_hours?.summary?.symbol_code
                                ?? first.next_6_hours?.summary?.symbol_code
                                ?? "";

                Apply(first.instant.details.air_temperature, DescribeSymbol(symbol), IconForSymbol(symbol));
                done(true);
            }
        }

        IEnumerator FetchOpenMeteo(Action<bool> done)
        {
            string url = string.Format(CultureInfo.InvariantCulture,
                "https://api.open-meteo.com/v1/forecast" +
                "?latitude={0}&longitude={1}&current=temperature_2m,weather_code&timezone=auto",
                latitude, longitude);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = timeoutSeconds;
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[WeatherWidget] Open-Meteo: {request.error}", this);
                    done(false);
                    yield break;
                }

                ForecastResponse data = null;
                try { data = JsonUtility.FromJson<ForecastResponse>(request.downloadHandler.text); }
                catch (Exception e) { Debug.LogWarning($"[WeatherWidget] Open-Meteo: {e.Message}", this); }

                if (data == null || data.current == null)
                {
                    done(false);
                    yield break;
                }

                int code = data.current.weather_code;
                Apply(data.current.temperature_2m, DescribeWmo(code), IconFor(code));
                done(true);
            }
        }

        static string DescribeSymbol(string symbol)
        {
            string key = (symbol ?? "").Split('_')[0];

            if (key == "clearsky") return "ясно";
            if (key == "fair") return "малооблачно";
            if (key == "partlycloudy") return "переменная облачность";
            if (key == "cloudy") return "пасмурно";
            if (key == "fog") return "туман";
            if (key.Contains("thunder")) return "гроза";
            if (key.Contains("sleet")) return "дождь со снегом";
            if (key.Contains("snow")) return key.StartsWith("light") ? "небольшой снег" : key.StartsWith("heavy") ? "сильный снег" : "снег";
            if (key.Contains("showers")) return "ливень";
            if (key.Contains("rain")) return key.StartsWith("light") ? "небольшой дождь" : key.StartsWith("heavy") ? "сильный дождь" : "дождь";
            return "погода";
        }

        Sprite IconForSymbol(string symbol)
        {
            string key = (symbol ?? "").Split('_')[0];

            if (key == "clearsky" || key == "fair") return icons.clear;
            if (key.Contains("snow") || key.Contains("sleet")) return icons.snow;
            if (key.Contains("rain") || key.Contains("thunder")) return icons.rain;
            return icons.cloudy;
        }
        void Apply(float temperature, string condition, Sprite sprite)
        {
            Temperature = temperature;
            Condition = condition;
            HasData = true;

            if (temperatureLabel != null)
            {
                int rounded = Mathf.RoundToInt(temperature);
                string sign = rounded > 0 ? "+" : "";
                temperatureLabel.text = string.Format(temperatureFormat, sign + rounded);
            }

            if (conditionLabel != null)
            {
                conditionLabel.text = string.IsNullOrWhiteSpace(cityName)
                    ? Condition
                    : $"{Condition}, {cityName}";
            }

            if (iconImage != null)
            {
                iconImage.sprite = sprite;
                iconImage.enabled = sprite != null;
            }
        }

        static string DescribeWmo(int code)
        {
            switch (code)
            {
                case 0: return "ясно";
                case 1: return "малооблачно";
                case 2: return "переменная облачность";
                case 3: return "пасмурно";
                case 45:
                case 48: return "туман";
                case 51:
                case 53:
                case 55: return "морось";
                case 56:
                case 57: return "ледяная морось";
                case 61: return "небольшой дождь";
                case 63: return "дождь";
                case 65: return "сильный дождь";
                case 66:
                case 67: return "ледяной дождь";
                case 71: return "небольшой снег";
                case 73: return "снег";
                case 75: return "сильный снег";
                case 77: return "снежные зёрна";
                case 80:
                case 81: return "ливень";
                case 82: return "сильный ливень";
                case 85:
                case 86: return "снегопад";
                case 95: return "гроза";
                case 96:
                case 99: return "гроза с градом";
                default: return "погода";
            }
        }

        Sprite IconFor(int code)
        {
            if (code == 0 || code == 1) return icons.clear;

            if ((code >= 71 && code <= 77) || code == 85 || code == 86)
                return icons.snow;

            if ((code >= 51 && code <= 67) || (code >= 80 && code <= 82) || code >= 95)
                return icons.rain;

            return icons.cloudy;
        }
    }
}
