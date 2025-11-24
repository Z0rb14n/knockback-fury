using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace UI.Options
{
    /// <summary>
    /// Main options menu behaviour.
    /// </summary>
    public class OptionsMenu : MonoBehaviour
    {
        private static readonly List<(string, FullScreenMode)> ObjectNameMappings = new()
        {
            // ExclusiveFullscreen is only supported on Windows
            ("Fullscreen", FullScreenMode.FullScreenWindow),
            // TODO only supported on Desktop
            ("Windowed", FullScreenMode.Windowed)
        };
        private const string SfxVolumeString = "SfxVolume";
        private const string MusicVolumeString = "MusicVolume";
        [Header("Default Settings")]
        [SerializeField]
        private int defaultTargetFPS = 60;
        [SerializeField]
        private int defaultVSync = 1;

        [Header("UI Settings")]
        [SerializeField]
        private TMP_Dropdown resolutionDropdown;
        [SerializeField]
        private Toggle[] windowTypeToggles;
        [SerializeField]
        private Toggle vsyncToggle;
        [SerializeField]
        private Slider frameRateSlider;
        [SerializeField]
        private TextMeshProUGUI frameRateValueText;
        [SerializeField]
        private string unlimitedFrameRateString = "Unlimited";
        [Header("Audio Settings")]
        [SerializeField]
        private AudioMixer audioMixer;
        [SerializeField]
        private Slider sfxVolumeSlider;
        [SerializeField]
        private TextMeshProUGUI sfxVolumeValueText;
        [SerializeField]
        private Slider musicVolumeSlider;
        [SerializeField]
        private TextMeshProUGUI musicVolumeValueText;

        private void Awake()
        {
            LoadSettings();
            vsyncToggle.isOn = PlayerPrefs.GetInt("VSync", defaultVSync) == 1;
            int fpsValue = PlayerPrefs.GetInt("TargetFPS", defaultTargetFPS);
            frameRateSlider.value = fpsValue == -1 ? frameRateSlider.maxValue : fpsValue;
            frameRateValueText.text = fpsValue == -1 ? unlimitedFrameRateString : fpsValue.ToString();
            string targetObjectName = ObjectNameMappings.Find(pair => pair.Item2 == Screen.fullScreenMode).Item1;
            windowTypeToggles.First(toggle => toggle.gameObject.name == targetObjectName).isOn = true;
            Resolution[] resolutions = Screen.resolutions;
            List<TMP_Dropdown.OptionData> options = resolutions
                .Select(res => new TMP_Dropdown.OptionData($"{res.width} x {res.height}")).Reverse().ToList();
            resolutionDropdown.options = options;
            resolutionDropdown.SetValueWithoutNotify(options.IndexOf(new TMP_Dropdown.OptionData($"{Screen.currentResolution.width} x {Screen.currentResolution.height}")));
            float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeString, 100);
            sfxVolumeSlider.value = sfxVolume;
            sfxVolumeValueText.text = Mathf.RoundToInt(sfxVolume).ToString();
            float musicVolume = PlayerPrefs.GetFloat(MusicVolumeString, 100);
            musicVolumeSlider.value = musicVolume;
            musicVolumeValueText.text = Mathf.RoundToInt(musicVolume).ToString();
        }

        /// <summary>
        /// Called when the frame rate slider's value has changed.
        /// </summary>
        /// <param name="newValue">New value</param>
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

        /// <summary>
        /// Sets the audio mixer volume given the param to change.
        /// </summary>
        /// <param name="param">Audio mixer parameter</param>
        /// <param name="displayVolume">Display volume, 0-100</param>
        private void SetVolume(string param, float displayVolume)
        {
            float dbVolume = ConvertDisplayVolumeToDb(displayVolume);
            audioMixer.SetFloat(param, dbVolume);
        }

        /// <summary>
        /// Called when SFX slider value has changed.
        /// </summary>
        /// <param name="newValue">New display volume, 0-100</param>
        public void OnSfxVolumeChanged(float newValue)
        {
            SetVolume(SfxVolumeString, newValue);
            sfxVolumeValueText.text = Mathf.RoundToInt(newValue).ToString();
            PlayerPrefs.SetFloat(SfxVolumeString, newValue);
        }

        /// <summary>
        /// Called when Volume slider value has changed.
        /// </summary>
        /// <param name="newValue">New display volume, 0-100</param>
        public void OnMusicVolumeChanged(float newValue)
        {
            SetVolume(MusicVolumeString, newValue);
            musicVolumeValueText.text = Mathf.RoundToInt(newValue).ToString();
            PlayerPrefs.SetFloat(MusicVolumeString, newValue);
        }

        /// <summary>
        /// Converts a 0-100 display volume to Db Gain (-80, 0)
        /// </summary>
        /// <param name="displayVolume">Display volume, 0-100</param>
        /// <returns>Db gain, -80 to 0</returns>
        private static float ConvertDisplayVolumeToDb(float displayVolume)
        {
            displayVolume /= 100;
            if (displayVolume <= 0)
            {
                return -80;
            }
            return 20 * Mathf.Log10(displayVolume);
        }

        /// <summary>
        /// Called when the resolution dropdown selected value's changed.
        /// </summary>
        /// <param name="selectedValue">Index of dropdown value selected</param>
        public void OnResolutionChanged(int selectedValue)
        {
            string option = resolutionDropdown.options[selectedValue].text;
            string[] split = option.Split(" x ", 2);
            int width = int.Parse(split[0]);
            int height = int.Parse(split[1]);
            Screen.SetResolution(width, height, Screen.fullScreenMode);
        }

        /// <summary>
        /// Called when the user toggles VSync.
        /// </summary>
        /// <param name="newValue">New VSync value</param>
        public void OnVsyncToggle(bool newValue)
        {
            int vsyncCount = newValue ? 1 : 0;
            QualitySettings.vSyncCount = vsyncCount;
            PlayerPrefs.SetInt("VSync", vsyncCount);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Called when a toggle's status changed.
        /// </summary>
        /// <param name="toggle">Toggle whos status has changed</param>
        public void OnWindowSettingToggle(Toggle toggle)
        {
            if (!toggle.isOn) return;
            FullScreenMode fullScreenMode = ObjectNameMappings.Find(pair => pair.Item1 == toggle.gameObject.name).Item2;
            Screen.fullScreenMode = fullScreenMode;
        }

        /// <summary>
        /// Called when clicking the back button: hides this.
        /// </summary>
        public void OnBackButtonClicked()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Loads settings from PlayerPrefs (that unity doesn't save automatically).
        /// </summary>
        /// <remarks>
        /// Unity saves the following settings to PlayerPrefs:
        /// <list type="bullet">
        ///  <item><description>full screen mode</description></item>
        ///  <item><description>Resolution (width and height)</description></item>
        ///  <item><description>display and game window position</description></item>
        /// </list>
        /// See: https://docs.unity3d.com/6000.2/Documentation/ScriptReference/Screen.html
        /// </remarks>
        public void LoadSettings()
        {
            Application.targetFrameRate = PlayerPrefs.GetInt("TargetFPS", defaultTargetFPS);

            int vsync = PlayerPrefs.GetInt("VSync", defaultVSync);
            QualitySettings.vSyncCount = vsync;
            float musicVolume = PlayerPrefs.GetFloat(MusicVolumeString, 100);
            SetVolume(MusicVolumeString, musicVolume);
            float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeString, 100);
            SetVolume(SfxVolumeString, sfxVolume);
        }
    }
}
