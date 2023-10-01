using FileSave;
using Player;
using UnityEngine;

namespace PermUpgrade
{
    /// <summary>
    /// Canvas to purchase permanent upgrades.
    /// </summary>
    public class PermUpgradeCanvas : MonoBehaviour
    {
        [SerializeField, Tooltip("Object to enable on interaction")] private RectTransform toEnable;
        
        /// <summary>
        /// Opens the UI and disables player movement/controls.
        /// </summary>
        public void Open()
        {
            PlayerMovementScript.Instance.CanMove = false;
            PlayerWeaponControl.Instance.enabled = false;
            CameraScript.Instance.enabled = false;
            Time.timeScale = 0;
            toEnable.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Closes the UI and enables player movement/controls.
        /// </summary>
        public void Close()
        {
            PlayerMovementScript.Instance.CanMove = true;
            PlayerWeaponControl.Instance.enabled = true;
            CameraScript.Instance.enabled = true;
            Time.timeScale = 1;
            toEnable.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Handles actions on sub-button click (i.e. purchase).
        /// </summary>
        /// <param name="type">Permanent upgrade type purchased</param>
        /// <param name="weaponName">Weapon name purchased (if applicable)</param>
        /// <param name="cost">Cost of upgrade</param>
        public void OnClick(PermUpgradeType type, string weaponName, int cost)
        {
            CrossRunInfo.Instance.BuyUpgrade(type, weaponName, cost);
        }
    }
}