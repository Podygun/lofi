using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LofiRoom
{
    public class SplashScreen : MonoBehaviour
    {
        public CanvasGroup group;

        public Image logo;

        public Text titleLabel;

        public Text creditsLabel;

        public Text hintLabel;

        [Min(0.1f)] public float fadeInSeconds = 1.2f;
        [Min(0.1f)] public float holdSeconds = 2.6f;
        [Min(0.1f)] public float fadeOutSeconds = 1.0f;

        [Min(0f)] public float hintDelay = 1.5f;

        public string nextScene = "LofiRoom";

        public bool allowSkip = true;

        enum Phase { FadeIn, Hold, FadeOut, Done }

        Phase _phase = Phase.FadeIn;
        float _timer;
        bool _loading;

        void Start()
        {
            if (group != null) group.alpha = 0f;
            if (hintLabel != null) hintLabel.canvasRenderer.SetAlpha(0f);
        }

        void Update()
        {
            _timer += Time.unscaledDeltaTime;

            if (allowSkip && _phase != Phase.Done && SkipPressed())
                StartFadeOut();

            switch (_phase)
            {
                case Phase.FadeIn:
                    SetAlpha(Mathf.Clamp01(_timer / fadeInSeconds));
                    if (_timer >= fadeInSeconds) SwitchTo(Phase.Hold);
                    break;

                case Phase.Hold:
                    SetAlpha(1f);
                    if (hintLabel != null && _timer >= hintDelay)
                    {
                        float k = Mathf.Clamp01((_timer - hintDelay) / 0.8f);
                        hintLabel.canvasRenderer.SetAlpha(k * PulseAlpha());
                    }
                    if (_timer >= holdSeconds) StartFadeOut();
                    break;

                case Phase.FadeOut:
                    SetAlpha(1f - Mathf.Clamp01(_timer / fadeOutSeconds));
                    if (_timer >= fadeOutSeconds) Load();
                    break;
            }
        }

        float PulseAlpha()
        {
            return 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 2f);
        }

        bool SkipPressed()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) return true;
            if (Touchscreen.current != null &&
                Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
            return false;
        }

        void StartFadeOut()
        {
            if (_phase == Phase.FadeOut || _phase == Phase.Done) return;
            SwitchTo(Phase.FadeOut);
        }

        void SwitchTo(Phase next)
        {
            _phase = next;
            _timer = 0f;
        }

        void SetAlpha(float a)
        {
            if (group != null) group.alpha = a;
        }

        void Load()
        {
            if (_loading) return;
            _loading = true;
            _phase = Phase.Done;

            if (string.IsNullOrWhiteSpace(nextScene))
            {
                Debug.LogError("[SplashScreen] Не указана следующая сцена.", this);
                return;
            }

            SceneManager.LoadScene(nextScene);
        }
    }
}
