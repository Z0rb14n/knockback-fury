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

        private void Awake()
        {
            LoadSettings();
            vsyncToggle.isOn = PlayerPrefs.GetInt("VSync", defaultVSync) == 1;
            int fpsValue = PlayerPrefs.GetInt("TargetFPS", defaultTargetFPS);
            frameRateSlider.value = fpsValue == -1 ? frameRateSlider.maxValue : fpsValue;
            frameRateValueText.text = fpsValue == -1 ? unlimitedFrameRateString : fpsValue.ToString();
            gameObject.SetActive(false);
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
            frameRateValueText.text = newFps == -1 ? unlimitedFrameRateString : newFps.ToString();
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

            Application.targetFrameRate = PlayerPrefs.GetInt("TargetFPS", defaultTargetFPS);

            int vsync = PlayerPrefs.GetInt("VSync", defaultVSync);
            QualitySettings.vSyncCount = vsync;
        }
    }
}
