using TMPro;
using UnityEngine;

namespace Upgrades
{
    /// <summary>
    /// Canvas to render the new upgrade pickups.
    /// </summary>
    [DisallowMultipleComponent]
    public class UpgradeCanvas : MonoBehaviour
    {
        [Tooltip("Title of upgrade")]
        public TextMeshProUGUI title;
        [Tooltip("Upgrade UI body")]
        public TextMeshProUGUI body;
        [Tooltip("Animator to display animation")]
        public Animator animator;
        
        public static UpgradeCanvas Instance
        {
            get
            {
                if (_instance == null) _instance = FindObjectOfType<UpgradeCanvas>();
                return _instance;
            }
        }
        private static UpgradeCanvas _instance;

        private void Awake()
        {
            animator.keepAnimatorStateOnDisable = false;
            transform.GetChild(0).gameObject.SetActive(false);
        }

        /// <summary>
        /// Shows the UI and updates text given an upgrade type.
        /// </summary>
        /// <param name="type">UpgradeType to display the data of.</param>
        public void Show(UpgradeType type)
        {
            title.text = UpgradeManager.Instance.UpgradeMapping[type].displayName;
            body.text = UpgradeManager.Instance.UpgradeMapping[type].infoText;
            transform.GetChild(0).gameObject.SetActive(true);
            animator.Rebind();
            animator.Update(0);
        }

        /// <summary>
        /// Hides the UI, which also resets animation state (hopefully).
        /// </summary>
        public void HideUI()
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}