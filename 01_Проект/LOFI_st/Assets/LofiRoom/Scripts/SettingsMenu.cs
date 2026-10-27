using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LofiRoom
{
    public class SettingsMenu : MonoBehaviour
    {
        public GameObject settingsPanel;

        public GameObject confirmPanel;

        public Slider masterSlider;

        public Slider ambienceSlider;

        public Slider sfxSlider;

        public Text masterValueLabel;
        public Text ambienceValueLabel;
        public Text sfxValueLabel;

        public Toggle widgetModeToggle;

        public WidgetModeController widgetMode;

        public GameObject widgetModeRow;

        public Button resumeButton;
        public Button quitButton;
        public Button confirmYesButton;
        public Button confirmNoButton;

        public AmbienceMixer mixer;

        public AudioSource sfxSource;

        public bool pauseGameTime = false;

        public Key toggleKey = Key.Escape;

        public bool IsOpen { get; private set; }
        float _saveDelay = -1f;

        const string KeyMaster = "lofi.volume.master";
        const string KeyAmbience = "lofi.volume.ambience";
        const string KeySfx = "lofi.volume.sfx";

        void Awake()
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(Close);
            if (quitButton != null) quitButton.onClick.AddListener(AskQuit);
            if (confirmYesButton != null) confirmYesButton.onClick.AddListener(QuitGame);
            if (confirmNoButton != null) confirmNoButton.onClick.AddListener(CancelQuit);

            if (widgetModeToggle != null)
                widgetModeToggle.onValueChanged.AddListener(OnWidgetModeChanged);

            if (masterSlider != null) masterSlider.onValueChanged.AddListener(OnMasterChanged);
            if (ambienceSlider != null) ambienceSlider.onValueChanged.AddListener(OnAmbienceChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        }

        void Start()
        {
            LoadVolumes();
            SetupWindowMode();

            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (confirmPanel != null) confirmPanel.SetActive(false);
            IsOpen = false;
        }

        void Update()
        {
            if (_saveDelay >= 0f)
            {
                _saveDelay -= Time.unscaledDeltaTime;
                if (_saveDelay < 0f) SaveVolumes();
            }

            if (Keyboard.current == null) return;

            if (Keyboard.current[toggleKey].wasPressedThisFrame)
            {
                if (confirmPanel != null && confirmPanel.activeSelf) CancelQuit();
                else Toggle();
            }
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public void Open()
        {
            IsOpen = true;
            if (settingsPanel != null) settingsPanel.SetActive(true);
            if (pauseGameTime) Time.timeScale = 0f;
        }

        public void Close()
        {
            IsOpen = false;
            if (confirmPanel != null) confirmPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (pauseGameTime) Time.timeScale = 1f;
            SaveVolumes();
        }

        void SetupWindowMode()
        {
            if (widgetModeRow != null)
                widgetModeRow.SetActive(widgetMode != null);

            if (widgetModeToggle != null && widgetMode != null)
                widgetModeToggle.SetIsOnWithoutNotify(widgetMode.IsWidgetMode);
        }

        public void OnWidgetModeChanged(bool value)
        {
            if (widgetMode == null) return;

            widgetMode.SetWidgetMode(value);

            if (value) Close();
        }

        void LoadVolumes()
        {
            float master = PlayerPrefs.GetFloat(KeyMaster, 1f);
            float ambience = PlayerPrefs.GetFloat(KeyAmbience, mixer != null ? mixer.masterVolume : 1f);
            float sfx = PlayerPrefs.GetFloat(KeySfx, sfxSource != null ? sfxSource.volume : 0.9f);

            if (masterSlider != null) masterSlider.SetValueWithoutNotify(master);
            if (ambienceSlider != null) ambienceSlider.SetValueWithoutNotify(ambience);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(sfx);

            OnMasterChanged(master);
            OnAmbienceChanged(ambience);
            OnSfxChanged(sfx);
        }

        void SaveVolumes()
        {
            if (masterSlider != null) PlayerPrefs.SetFloat(KeyMaster, masterSlider.value);
            if (ambienceSlider != null) PlayerPrefs.SetFloat(KeyAmbience, ambienceSlider.value);
            if (sfxSlider != null) PlayerPrefs.SetFloat(KeySfx, sfxSlider.value);
            PlayerPrefs.Save();
        }

        public void OnMasterChanged(float value)
        {
            AudioListener.volume = value;
            SetPercent(masterValueLabel, value);
            PlayerPrefs.SetFloat(KeyMaster, value);
            _saveDelay = 0.5f;
        }

        public void OnAmbienceChanged(float value)
        {
            if (mixer != null) mixer.masterVolume = value;
            SetPercent(ambienceValueLabel, value);
            PlayerPrefs.SetFloat(KeyAmbience, value);
            _saveDelay = 0.5f;
        }

        public void OnSfxChanged(float value)
        {
            if (sfxSource != null) sfxSource.volume = value;
            SetPercent(sfxValueLabel, value);
            PlayerPrefs.SetFloat(KeySfx, value);
            _saveDelay = 0.5f;
        }

        static void SetPercent(Text label, float value01)
        {
            if (label != null) label.text = Mathf.RoundToInt(value01 * 100f) + "%";
        }

        public void AskQuit()
        {
            if (confirmPanel != null) confirmPanel.SetActive(true);
        }

        public void CancelQuit()
        {
            if (confirmPanel != null) confirmPanel.SetActive(false);
        }

        public void QuitGame()
        {
            SaveVolumes();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
