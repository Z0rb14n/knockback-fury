using TMPro;
using UnityEngine;

namespace Pause
{
    /// <summary>
    /// Container for each upgrade in the pause upgrade list.
    /// </summary>
    [DisallowMultipleComponent]
    public class PauseUpgradeObject : MonoBehaviour
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI body;
    }
}