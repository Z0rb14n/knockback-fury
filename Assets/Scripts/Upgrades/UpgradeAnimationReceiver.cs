using UnityEngine;

namespace Upgrades
{
    /// <summary>
    /// Due to animator shenanigans, this receives animation events.
    /// </summary>
    public class UpgradeAnimationReceiver : MonoBehaviour
    {
        /// <summary>
        /// Called by animaton on animation end.
        /// </summary>
        public void OnAnimationEnd()
        {
            UpgradeCanvas.Instance.HideUI();
        }
    }
}