using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [Header("Default Settings")]
    public int defaultWidth = 1920;
    public int defaultHeight = 1080;
    public FullScreenMode defaultScreenMode = FullScreenMode.FullScreenWindow;
    public int defaultRefreshRate = 60;
    public int defaultTargetFPS = 60;
    public int defaultVSync = 1;

    private void Awake()
    {
        LoadSettings();

    }

    // --- Resolution + Fullscreen ---
    public void SetResolution(int width, int height, FullScreenMode mode, int refreshRate)
    {
        Screen.SetResolution(width, height, mode, refreshRate);
        PlayerPrefs.SetInt("ResolutionWidth", width);
        PlayerPrefs.SetInt("ResolutionHeight", height);
        PlayerPrefs.SetInt("RefreshRate", refreshRate);
        PlayerPrefs.SetInt("FullScreenMode", (int)mode);
        PlayerPrefs.Save();
    }

    // --- Switch Display ---
    public void ActivateDisplay(int index)
    {
        if (index < Display.displays.Length)
        {
            Display.displays[index].Activate();
            PlayerPrefs.SetInt("ActiveDisplay", index);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogWarning("Display index out of range.");
        }
    }

    // --- FPS Cap ---
    public void SetTargetFPS(int fps)
    {
        Application.targetFrameRate = fps;
        PlayerPrefs.SetInt("TargetFPS", fps);
        PlayerPrefs.Save();
    }

    // --- VSync ---
    public void SetVSync(int count)
    {
        QualitySettings.vSyncCount = count;
        PlayerPrefs.SetInt("VSync", count);
        PlayerPrefs.Save();
    }

    // --- Load saved settings or defaults ---
    public void LoadSettings()
    {
        int width = PlayerPrefs.GetInt("ResolutionWidth", defaultWidth);
        int height = PlayerPrefs.GetInt("ResolutionHeight", defaultHeight);
        int refreshRate = PlayerPrefs.GetInt("RefreshRate", defaultRefreshRate);
        FullScreenMode mode = (FullScreenMode)PlayerPrefs.GetInt("FullScreenMode", (int)defaultScreenMode);
        Screen.SetResolution(width, height, mode, refreshRate);

        int activeDisplay = PlayerPrefs.GetInt("ActiveDisplay", 0);
        if (activeDisplay < Display.displays.Length)
        {
            Display.displays[activeDisplay].Activate();
        }

        int fps = PlayerPrefs.GetInt("TargetFPS", defaultTargetFPS);
        Application.targetFrameRate = fps;

        int vsync = PlayerPrefs.GetInt("VSync", defaultVSync);
        QualitySettings.vSyncCount = vsync;
    }
}