using Player;
using UnityEngine;

namespace CustomTiles
{
    /// <summary>
    /// A script to be placed on a tile to allow movement through.
    ///
    /// Can alternatively be placed on custom diagonal structures.
    /// </summary>
    [RequireComponent(typeof(PlatformEffector2D))]
    public class PlatformTileScript : MonoBehaviour
    {
        private PlayerMovementScript _playerMovement;
        private Collider2D _playerCollider;
        private Collider2D _collider;
        private bool _temporaryIgnore;
        private PlatformEffector2D _platformEffector;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _playerMovement = PlayerMovementScript.Instance;
            _playerCollider = _playerMovement.GetComponent<Collider2D>();
        }

        private void FixedUpdate()
        {
            if (_temporaryIgnore && !_collider.Distance(_playerCollider).isOverlapped)
            {
                _temporaryIgnore = false;
                Physics2D.IgnoreCollision(_collider, _playerCollider, false);
            }
        }

        /// <summary>
        /// Temporarily ignore the collision; will be un-ignored when the player exits.
        /// </summary>
        public void TemporarilyIgnore()
        {
            _temporaryIgnore = true;
            Physics2D.IgnoreCollision(_collider, _playerCollider, true);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider == _playerCollider) _playerMovement.AddPlatformOn(this);
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.collider == _playerCollider) _playerMovement.RemovePlatformOn(this);
        }
    }
}