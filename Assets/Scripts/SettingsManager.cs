using UI.Options;
using UnityEngine;

/// <summary>
/// Settings Manager. Currently only used to load settings.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    /// <remarks>
    /// Has to be Start instead of Awake as AudioMixer doesn't register our settings changes in Awake.
    /// </remarks>
    private void Start()
    {
        FindAnyObjectByType<OptionsMenu>(FindObjectsInactive.Include)?.LoadSettings();
    }
}