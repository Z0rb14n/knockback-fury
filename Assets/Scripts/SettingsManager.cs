using UI.Options;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    private void Awake()
    {
        FindAnyObjectByType<OptionsMenu>(FindObjectsInactive.Include)?.LoadSettings();
    }
}