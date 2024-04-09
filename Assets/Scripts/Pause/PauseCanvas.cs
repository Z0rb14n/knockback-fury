using System.Linq;
using Player;
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
        
        private void Awake()
        {
            actualPauseMenu.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            if (actualPauseMenu.gameObject.activeSelf) Hide();
            else if (!UIUtil.IsUIOpen) Show();
        }

        private void Show()
        {
            UIUtil.OpenUI();
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
            UIUtil.CloseUI();
            actualPauseMenu.gameObject.SetActive(false);
        }

        public void OnResumeButtonClicked()
        {
            Hide();
        }

        public void OnRestartButtonClicked()
        {
            MiscUtil.EnablePlayerEnemyCollision();
            Hide();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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