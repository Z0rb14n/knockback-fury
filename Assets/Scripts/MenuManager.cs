using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsMenu;

    public void ToggleSettingsMenu()
    {
        bool isActive = settingsMenu.activeSelf;
        settingsMenu.SetActive(!isActive);
    }
}