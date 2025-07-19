using System.Collections;
using System.Text;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;

namespace GameEnd
{
    [DisallowMultipleComponent]
    public class GameEndCanvas : MonoBehaviour
    {
        private static GameEndCanvas _instance;

        public static GameEndCanvas Instance
        {
            get
            {
                if (!_instance) _instance = FindAnyObjectByType<GameEndCanvas>(FindObjectsInactive.Include);
                return _instance;
            }
        }

        [SerializeField]
        private RectTransform actualUI;
        [SerializeField]
        private TextMeshProUGUI titleText;
        [SerializeField]
        private TextMeshProUGUI runInfoText;
        [SerializeField]
        private TextMeshProUGUI runInfoNumbers;
        [SerializeField] private string winText = "You Win!";
        [SerializeField] private string loseText = "You Died!";
        
        public GameEndData endData;

        private void Awake()
        {
            endData = new GameEndData();
        }

        /// <summary>
        /// Re-enables physics and player collision and goes to title
        /// </summary>
        public void GoToTitle()
        {
            Time.timeScale = 1;
            PlayerMovementScript.Instance.CanMove = true;
            PlayerWeaponControl.Instance.enabled = true;
            CameraScript.Instance.enabled = true;
            MiscUtil.EnablePlayerEnemyCollision();
            SceneManager.LoadScene("MainMenuScene");
        }

        /// <summary>
        /// Re-enables physics and player collision and goes to lobby
        /// </summary>
        public void GoToLobby()
        {
            Time.timeScale = 1;
            PlayerMovementScript.Instance.CanMove = true;
            PlayerWeaponControl.Instance.enabled = true;
            CameraScript.Instance.enabled = true;
            MiscUtil.EnablePlayerEnemyCollision();
            SceneManager.LoadScene("LobbyScene");
        }

        /// <summary>
        /// Displays this after a delay (realtime seconds).
        /// </summary>
        /// <param name="delay">Delay in realtime seconds.</param>
        /// <param name="didWin">Whether to display the win text or the lose text</param>
        public void DisplayAfterDelay(float delay, bool didWin)
        {
            StartCoroutine(DisplayDelayCoroutine(delay, didWin));
        }

        /// <summary>
        /// Disables all player actions.
        /// </summary>
        private static void DisableEverything()
        {
            PlayerMovementScript.Instance.CanMove = false;
            PlayerWeaponControl.Instance.enabled = false;
            CameraScript.Instance.enabled = false;
            Time.timeScale = 0;
        }

        private IEnumerator DisplayDelayCoroutine(float delay, bool didWin)
        {
            yield return new WaitForSecondsRealtime(delay);
            DisableEverything();
            actualUI.gameObject.SetActive(true);
            if (didWin && PlayerHealth.Instance.health <= 0)
            {
                Debug.LogWarning("Did the player just lose after they won?");
                didWin = false;
            }
            
            int timeSeconds = Mathf.FloorToInt(Time.timeSinceLevelLoad);
            titleText.text = didWin ? winText : loseText;
            runInfoText.text = "Time:\nDamage dealt:\nEnemies killed:\nShots fired:\nHits taken:\nDamage taken:\nUpgrades Acquired:";
            StringBuilder sb = new();
            if (timeSeconds >= 60) sb.Append(timeSeconds / 60).Append("m ");
            sb.Append(timeSeconds % 60).Append("s\n");
            sb.Append(endData.damageDealt).Append('\n');
            sb.Append(endData.enemiesKilled).Append('\n');
            sb.Append(endData.shotsFired).Append('\n');
            sb.Append(endData.hitsTaken).Append('\n');
            sb.Append(endData.damageTaken).Append('\n');
            sb.Append(PlayerUpgradeManager.Instance.TotalUpgradeCount);
            runInfoNumbers.text = sb.ToString();
        }

        /// <summary>
        /// End game data that isn't saved elsewhere (time and upgrade count computed elsewhere).
        /// </summary>
        public struct GameEndData
        {
            public int damageDealt;
            public int enemiesKilled;
            public int shotsFired;
            public int hitsTaken;
            public int damageTaken;
        }
    }
}