using UnityEngine;

namespace DashVFX
{
    public abstract class AbstractDashTrail : MonoBehaviour
    {
        public abstract void StartDash(bool flipped);

        public abstract void StopDash();
    }
}