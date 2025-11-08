using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Options
{
    /// <summary>
    /// Main options menu behaviour.
    /// </summary>
    public class OptionsMenu : MonoBehaviour
    {
        [Header("Default Settings")]
        [SerializeField]
        private int defaultWidth = 1920;
        [SerializeField]
        private int defaultHeight = 1080;
        [SerializeField]
        private FullScreenMode defaultScreenMode = FullScreenMode.FullScreenWindow;
        [SerializeField]
        private int defaultTargetFPS = 60;
        [SerializeField]
        private int defaultVSync = 1;

        [Header("UI Settings")]
        [SerializeField] private Toggle vsyncToggle;
        [SerializeField] private Slider frameRateSlider;
        [SerializeField] private TextMeshProUGUI frameRateValueText;
        [SerializeField] private string unlimitedFrameRateString = "Unlimited";

        private int SetFps => PlayerPrefs.GetInt("TargetFPS", defaultTargetFPS);

        private void Awake()
        {
            LoadSettings();
            vsyncToggle.isOn = PlayerPrefs.GetInt("VSync", defaultVSync) == 1;
            frameRateSlider.value = SetFps;
            frameRateValueText.text = SetFps == -1 ? unlimitedFrameRateString : SetFps.ToString();
        }

        // --- Resolution + Fullscreen ---
        public void SetResolution(int width, int height, FullScreenMode mode)
        {
            Screen.SetResolution(width, height, mode);
            PlayerPrefs.SetInt("ResolutionWidth", width);
            PlayerPrefs.SetInt("ResolutionHeight", height);
            PlayerPrefs.SetInt("FullScreenMode", (int)mode);
            PlayerPrefs.Save();
        }

        public void OnFrameRateChanged(float newValue)
        {
            int newFps = Mathf.RoundToInt(newValue);
            if (Mathf.Approximately(newValue, frameRateSlider.maxValue))
            {
                newFps = -1;
            }
            Application.targetFrameRate = newFps;
            frameRateValueText.text = newFps == -1 ? unlimitedFrameRateString : SetFps.ToString();
            PlayerPrefs.SetInt("TargetFPS", newFps);
            PlayerPrefs.Save();
        }

        public void OnVsyncToggle(bool newValue)
        {
            int vsyncCount = newValue ? 1 : 0;
            QualitySettings.vSyncCount = vsyncCount;
            PlayerPrefs.SetInt("VSync", vsyncCount);
            PlayerPrefs.Save();
        }

        public void OnBackButtonClicked()
        {
            gameObject.SetActive(false);
        }

        public void LoadSettings()
        {
            int width = PlayerPrefs.GetInt("ResolutionWidth", defaultWidth);
            int height = PlayerPrefs.GetInt("ResolutionHeight", defaultHeight);
            FullScreenMode mode = (FullScreenMode)PlayerPrefs.GetInt("FullScreenMode", (int)defaultScreenMode);
            Screen.SetResolution(width, height, mode);

            int fps = PlayerPrefs.GetInt("TargetFPS", defaultTargetFPS);
            Application.targetFrameRate = fps;

            int vsync = PlayerPrefs.GetInt("VSync", defaultVSync);
            QualitySettings.vSyncCount = vsync;
        }
    }
}
