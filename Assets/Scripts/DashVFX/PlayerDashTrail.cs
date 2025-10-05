using Player;
using UnityEngine;

namespace DashVFX
{
    [RequireComponent(typeof(PlayerMovementScript), typeof(SpriteRenderer))]
    public class PlayerDashTrail : MultiSpriteDashTrail
    {
        private PlayerMovementScript _playerMovementScript;
        
        protected override void Awake()
        {
            base.Awake();
            _playerMovementScript = GetComponent<PlayerMovementScript>();
        }

        protected override Vector2 GetObjectVelocity()
        {
            return _playerMovementScript.Velocity;
        }
    }
}
