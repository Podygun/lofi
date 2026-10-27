using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LofiRoom
{
    public class CollapsibleWidget : MonoBehaviour
    {
        public List<GameObject> fullOnly = new List<GameObject>();

        public Image background;

        public bool hideBackgroundWhenMini = true;

        public GameObject miniBackground;

        public List<GameObject> miniOnly = new List<GameObject>();

        public RectTransform primary;

        public RectTransform miniControls;

        [Min(64f)] public float miniControlsWidth = 176f;

        [Min(0f)] public float miniControlsGap = 12f;

        public Vector2 fullSize = new Vector2(400, 224);
        public Vector2 miniSize = new Vector2(200, 64);

        [Min(0f)] public float miniIconWidth = 56f;

        [Min(0f)] public float miniRightPadding = 26f;

        public Button toggleButton;

        public RectTransform toggleIcon;

        public bool startCollapsed = false;

        public bool remember = true;

        public string prefsKey = "lofi.widget";

        public bool IsCollapsed { get; private set; }

        RectTransform _rect;
        Text _primaryText;
        TextAnchor _primaryAlignment;
        Vector2 _primaryAnchorMin;
        Vector2 _primaryAnchorMax;
        Vector2 _primaryPivot;
        Vector2 _primaryPosition;
        Vector2 _primarySize;
        Vector2 _controlsAnchorMin;
        Vector2 _controlsAnchorMax;
        Vector2 _controlsPivot;
        Vector2 _controlsPosition;
        Vector2 _controlsSize;
        RectTransform _miniBackgroundRect;
        Vector2 _miniBackgroundSize;
        bool _primaryBestFit;
        int _primaryMinFontSize;
        int _primaryMaxFontSize;

        void Awake()
        {
            _rect = GetComponent<RectTransform>();
            if (toggleButton != null) toggleButton.onClick.AddListener(Toggle);

            if (primary != null)
            {
                _primaryText = primary.GetComponent<Text>();
                if (_primaryText != null)
                {
                    _primaryAlignment = _primaryText.alignment;
                    _primaryBestFit = _primaryText.resizeTextForBestFit;
                    _primaryMinFontSize = _primaryText.resizeTextMinSize;
                    _primaryMaxFontSize = _primaryText.resizeTextMaxSize;
                }

                _primaryAnchorMin = primary.anchorMin;
                _primaryAnchorMax = primary.anchorMax;
                _primaryPivot = primary.pivot;
                _primaryPosition = primary.anchoredPosition;
                _primarySize = primary.sizeDelta;
            }

            if (miniControls != null)
            {
                _controlsAnchorMin = miniControls.anchorMin;
                _controlsAnchorMax = miniControls.anchorMax;
                _controlsPivot = miniControls.pivot;
                _controlsPosition = miniControls.anchoredPosition;
                _controlsSize = miniControls.sizeDelta;
            }

            if (miniBackground != null)
            {
                _miniBackgroundRect = miniBackground.GetComponent<RectTransform>();
                if (_miniBackgroundRect != null)
                    _miniBackgroundSize = _miniBackgroundRect.sizeDelta;
            }
        }

        void Start()
        {
            bool collapsed = remember
                ? PlayerPrefs.GetInt(prefsKey, startCollapsed ? 1 : 0) == 1
                : startCollapsed;

            Apply(collapsed);
        }

        public void Toggle()
        {
            Apply(!IsCollapsed);

            if (remember)
            {
                PlayerPrefs.SetInt(prefsKey, IsCollapsed ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public void Apply(bool collapsed)
        {
            IsCollapsed = collapsed;

            foreach (var go in fullOnly)
                if (go != null && (miniControls == null || go != miniControls.gameObject))
                    go.SetActive(!collapsed);

            foreach (var go in miniOnly)
                if (go != null) go.SetActive(collapsed);

            if (miniBackground != null) miniBackground.SetActive(collapsed);

            if (_miniBackgroundRect != null)
                _miniBackgroundRect.sizeDelta = collapsed ? miniSize : _miniBackgroundSize;

            if (background != null)
                background.enabled = !(collapsed && hideBackgroundWhenMini);

            if (_rect != null)
                _rect.sizeDelta = collapsed ? miniSize : fullSize;

            if (primary != null)
            {
                if (collapsed)
                {
                    primary.anchorMin = primary.anchorMax = new Vector2(0f, 0.5f);
                    primary.pivot = new Vector2(0f, 0.5f);
                    primary.anchoredPosition = new Vector2(miniIconWidth, 0f);
                    float controlsReservation = miniControls != null
                        ? miniControlsWidth + miniControlsGap + 8f
                        : miniRightPadding;
                    primary.sizeDelta = new Vector2(
                        Mathf.Max(40f, miniSize.x - miniIconWidth - controlsReservation),
                        miniSize.y - 8f);

                    if (_primaryText != null)
                    {
                        _primaryText.alignment = TextAnchor.MiddleLeft;
                        _primaryText.resizeTextForBestFit = true;
                        _primaryText.resizeTextMinSize = Mathf.Max(10, Mathf.Min(_primaryMinFontSize, 14));
                        _primaryText.resizeTextMaxSize = _primaryMaxFontSize > 0
                            ? _primaryMaxFontSize
                            : _primaryText.fontSize;
                    }
                }
                else
                {
                    if (_primaryText != null)
                    {
                        _primaryText.alignment = _primaryAlignment;
                        _primaryText.resizeTextForBestFit = _primaryBestFit;
                        _primaryText.resizeTextMinSize = _primaryMinFontSize;
                        _primaryText.resizeTextMaxSize = _primaryMaxFontSize;
                    }

                    primary.anchorMin = _primaryAnchorMin;
                    primary.anchorMax = _primaryAnchorMax;
                    primary.pivot = _primaryPivot;
                    primary.anchoredPosition = _primaryPosition;
                    primary.sizeDelta = _primarySize;
                }

                if (collapsed) primary.gameObject.SetActive(true);
            }

            if (miniControls != null)
            {
                if (collapsed)
                {
                    miniControls.gameObject.SetActive(true);
                    miniControls.anchorMin = miniControls.anchorMax = new Vector2(1f, 0.5f);
                    miniControls.pivot = new Vector2(1f, 0.5f);
                    miniControls.anchoredPosition = new Vector2(-8f, 0f);
                    miniControls.sizeDelta = new Vector2(miniControlsWidth, Mathf.Min(42f, miniSize.y - 12f));
                }
                else
                {
                    miniControls.anchorMin = _controlsAnchorMin;
                    miniControls.anchorMax = _controlsAnchorMax;
                    miniControls.pivot = _controlsPivot;
                    miniControls.anchoredPosition = _controlsPosition;
                    miniControls.sizeDelta = _controlsSize;
                }
            }

            if (toggleIcon != null)
                toggleIcon.localRotation = Quaternion.Euler(0f, 0f, collapsed ? 180f : 0f);
        }
    }
}
