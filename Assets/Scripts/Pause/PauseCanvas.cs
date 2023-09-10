using Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pause
{
    [DisallowMultipleComponent]
    public class PauseCanvas : MonoBehaviour
    {
        [SerializeField] private RectTransform actualPauseMenu;
        
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

        public void OnUpgradesButtonClicked()
        {
            
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