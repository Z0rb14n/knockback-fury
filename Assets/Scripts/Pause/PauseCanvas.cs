using System.Linq;
using Player;
using UI.Options;
using UnityEngine;
using UnityEngine.SceneManagement;
using Upgrades;
using Util;

namespace Pause
{
    /// <summary>
    /// Script for the pause canvas.
    /// </summary>
    [DisallowMultipleComponent]
    public class PauseCanvas : MonoBehaviour
    {
        [SerializeField] private RectTransform actualPauseMenu;
        [SerializeField] private RectTransform upgradeList;
        [SerializeField] private GameObject upgradePrefab;
        
        private OptionsMenu _optionsMenu;
        
        private void Awake()
        {
            actualPauseMenu.gameObject.SetActive(false);
            _optionsMenu = FindAnyObjectByType<OptionsMenu>(FindObjectsInactive.Include);
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            if (actualPauseMenu.gameObject.activeSelf) Hide();
            else Show();
        }

        private void Show()
        {
            PlayerMovementScript.Instance.CanMove = false;
            PlayerWeaponControl.Instance.enabled = false;
            CameraScript.Instance.enabled = false;
            Time.timeScale = 0;
            actualPauseMenu.gameObject.SetActive(true);
            UpgradePickupData[] types = PlayerUpgradeManager.Instance.GetUniqueUpgrades
                .Select(type => UpgradeManager.Instance.UpgradeMapping[type])
                .OrderBy(type => type.displayName)
                .ToArray();
            ObjectUtil.EnsureLength(upgradeList, types.Length, upgradePrefab);
            for (int i = 0; i < types.Length; i++)
            {
                PauseUpgradeObject pauseUpgradeObject = upgradeList.GetChild(i).GetComponent<PauseUpgradeObject>();
                pauseUpgradeObject.title.text = types[i].displayName;
                pauseUpgradeObject.body.text = types[i].infoText;
            }
        }

        private void Hide()
        {
            PlayerMovementScript.Instance.CanMove = true;
            PlayerWeaponControl.Instance.enabled = true;
            CameraScript.Instance.enabled = true;
            Time.timeScale = 1;
            actualPauseMenu.gameObject.SetActive(false);
        }

        public void OnResumeButtonClicked()
        {
            Hide();
        }

        public void OnOptionsButtonClicked()
        {
            _optionsMenu.gameObject.SetActive(true);
        }

        public void OnLobbyButtonClicked()
        {
            MiscUtil.EnablePlayerEnemyCollision();
            Hide();
            SceneManager.LoadScene("LobbyScene");
        }

        public void OnMainMenuButtonClicked()
        {
            MiscUtil.EnablePlayerEnemyCollision();
            Hide();
            SceneManager.LoadScene("MainMenuScene");
        }

        public void OnQuitToDesktopClicked()
        {
            Application.Quit();
        }
    }
}