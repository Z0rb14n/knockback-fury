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
            else Show();
        }

        private void Show()
        {
            PlayerMovementScript.Instance.CanMove = false;
            PlayerWeaponControl.Instance.enabled = false;
            CameraScript.Instance.enabled = false;
            Time.timeScale = 0;
            actualPauseMenu.gameObject.SetActive(true);
            UpgradePickupData[] types = PlayerUpgradeManager.Instance.upgrades.Where(upgradeCount => upgradeCount.count > 0)
                .Select(count => count.type)
                .Distinct()
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

        private static void EnablePlayerCollision()
        {
            int playerLayerID = LayerMask.NameToLayer("Player");
            int enemyLayerID = LayerMask.NameToLayer("Enemy");
            Physics2D.IgnoreLayerCollision(playerLayerID, enemyLayerID, false);
        }

        public void OnResumeButtonClicked()
        {
            Hide();
        }

        public void OnRestartButtonClicked()
        {
            EnablePlayerCollision();
            Hide();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OnMainMenuButtonClicked()
        {
            EnablePlayerCollision();
            Hide();
            SceneManager.LoadScene("MainMenuScene");
        }

        public void OnQuitToDesktopClicked()
        {
            Application.Quit();
        }
    }
}