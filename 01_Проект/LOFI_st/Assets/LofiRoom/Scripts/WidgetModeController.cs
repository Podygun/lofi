using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace LofiRoom
{
    public class WidgetModeController : MonoBehaviour
    {
        const string PrefKey = "lofi.window.widgetMode";

        [Min(240)] public int widgetWidth = 640;
        [Min(135)] public int widgetHeight = 360;

        [Min(0)] public int screenMargin = 24;

        public Corner corner = Corner.BottomRight;

        public List<GameObject> hiddenInWidgetMode = new List<GameObject>();

        public List<CollapsibleWidget> collapsedInWidgetMode = new List<CollapsibleWidget>();

        public bool remember = true;

        public enum Corner
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        public bool IsWidgetMode { get; private set; }

        public static bool CanResizeWindow
        {
            get
            {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        int _normalWidth;
        int _normalHeight;
        bool[] _previousWidgetStates;
        bool[] _previousHiddenStates;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        const uint SWP_SHOWWINDOW = 0x0040;
        const uint SWP_NOSIZE = 0x0001;

        [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")] static extern bool SetWindowPos(
            IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
#endif

        IEnumerator Start()
        {
            _normalWidth = Screen.width;
            _normalHeight = Screen.height;

            bool wanted = remember && PlayerPrefs.GetInt(PrefKey, 0) == 1;
            yield return null;
            if (wanted) Apply(true);
        }

        public void Toggle()
        {
            Apply(!IsWidgetMode);
        }

        public void SetWidgetMode(bool value)
        {
            Apply(value);
        }

        void Apply(bool widgetMode)
        {
            if (widgetMode == IsWidgetMode) return;

            if (widgetMode)
            {
                _previousWidgetStates = new bool[collapsedInWidgetMode.Count];
                for (int i = 0; i < collapsedInWidgetMode.Count; i++)
                    _previousWidgetStates[i] = collapsedInWidgetMode[i] != null
                        && collapsedInWidgetMode[i].IsCollapsed;

                _previousHiddenStates = new bool[hiddenInWidgetMode.Count];
                for (int i = 0; i < hiddenInWidgetMode.Count; i++)
                    _previousHiddenStates[i] = hiddenInWidgetMode[i] != null
                        && hiddenInWidgetMode[i].activeSelf;
            }

            IsWidgetMode = widgetMode;

            for (int i = 0; i < hiddenInWidgetMode.Count; i++)
            {
                var go = hiddenInWidgetMode[i];
                if (go == null) continue;

                bool active = widgetMode
                    ? false
                    : _previousHiddenStates != null && i < _previousHiddenStates.Length
                        ? _previousHiddenStates[i]
                        : true;
                go.SetActive(active);
            }

            for (int i = 0; i < collapsedInWidgetMode.Count; i++)
            {
                var widget = collapsedInWidgetMode[i];
                if (widget == null) continue;

                bool collapsed = widgetMode
                    ? true
                    : _previousWidgetStates != null && i < _previousWidgetStates.Length
                        ? _previousWidgetStates[i]
                        : false;
                widget.Apply(collapsed);
            }

            ApplyWindow(widgetMode);

            if (remember)
            {
                PlayerPrefs.SetInt(PrefKey, widgetMode ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        void ApplyWindow(bool widgetMode)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (widgetMode)
            {
                Screen.SetResolution(widgetWidth, widgetHeight, FullScreenMode.Windowed);
                StartCoroutine(MoveToCornerNextFrame());
            }
            else
            {
                Screen.SetResolution(_normalWidth, _normalHeight, FullScreenMode.Windowed);
                SetTopmost(false);
            }
#endif
        }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        System.Collections.IEnumerator MoveToCornerNextFrame()
        {
            yield return null;
            yield return null;

            SetTopmost(true);
            MoveToCorner();
        }

        void SetTopmost(bool topmost)
        {
            IntPtr window = GetActiveWindow();
            if (window == IntPtr.Zero) return;

            SetWindowPos(window, topmost ? HWND_TOPMOST : HWND_NOTOPMOST,
                         0, 0, 0, 0, SWP_NOSIZE | SWP_SHOWWINDOW);
        }

        void MoveToCorner()
        {
            IntPtr window = GetActiveWindow();
            if (window == IntPtr.Zero) return;

            int screenWidth = Display.main.systemWidth;
            int screenHeight = Display.main.systemHeight;

            int x = screenMargin;
            int y = screenMargin;

            if (corner == Corner.TopRight || corner == Corner.BottomRight)
                x = screenWidth - widgetWidth - screenMargin;
            if (corner == Corner.BottomLeft || corner == Corner.BottomRight)
                y = screenHeight - widgetHeight - screenMargin * 2;

            SetWindowPos(window, HWND_TOPMOST, x, y, widgetWidth, widgetHeight, SWP_SHOWWINDOW);
        }
#endif
    }
}
