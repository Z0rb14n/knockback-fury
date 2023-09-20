using System.Collections;
using Player;
using UnityEngine;

namespace CustomTiles
{
    /// <summary>
    /// A script to be placed on a square tile to allow movement through.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PlatformTileScript : MonoBehaviour
    {
        /// <summary>
        /// Since <see cref="Physics2D.defaultContactOffset"/> is typically > 0,
        /// the player will spend a few physics ticks above the platform when moving down.
        /// Thus, we need a short duration for the player to move below.
        /// </summary>
        [SerializeField, Min(0), Tooltip("Duration for the collision to be ignored")] private float temporaryIgnoreDuration = 0.1f;
        private PlayerMovementScript _playerMovement;
        private Collider2D _playerCollider;
        private Collider2D _collider;
        private bool _temporaryIgnore;

        /// <summary>
        /// Determines if the player is above this platform (i.e. should consider physics collisions).
        /// </summary>
        private bool IsPlayerAbove
        {
            get
            {
                ColliderDistance2D dist;
                if (_collider.usedByComposite) dist = _collider.composite.Distance(_playerCollider);
                else dist = _collider.Distance(_playerCollider);
                //Debug.Log(dist.distance + "," + dist.normal, this);
                return (dist.normal.y < 0 && dist.distance >= 0) || Mathf.Abs(dist.distance) < Physics2D.defaultContactOffset;
            }
        }

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _playerMovement = PlayerMovementScript.Instance;
            _playerCollider = _playerMovement.GetComponent<Collider2D>();
        }

        private void FixedUpdate()
        {
            Physics2D.IgnoreCollision(_collider, _playerCollider, !IsPlayerAbove || _temporaryIgnore);
        }

        /// <summary>
        /// Temporarily ignore the collision for <see cref="temporaryIgnoreDuration"/> seconds.
        /// </summary>
        public void TemporarilyIgnore()
        {
            StartCoroutine(TemporaryIgnoreCoroutine());
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider == _playerCollider) _playerMovement.AddPlatformOn(this);
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.collider == _playerCollider) _playerMovement.RemovePlatformOn(this);
        }

        private IEnumerator TemporaryIgnoreCoroutine()
        {
            _temporaryIgnore = true;
            yield return new WaitForSeconds(temporaryIgnoreDuration);
            _temporaryIgnore = false;
        }
    }
}