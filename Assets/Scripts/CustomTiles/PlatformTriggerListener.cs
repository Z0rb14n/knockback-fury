using Player;
using UnityEngine;

namespace CustomTiles
{
    public class PlatformTriggerListener : MonoBehaviour
    {
        private PlayerMovementScript _playerMovement;
        private PlatformTileScript _tile;
        private Collider2D _playerCollider;
        private Collider2D _collider;

        private int _platformLayer;
        private int _platformIgnoredLayer;

        private void Awake()
        {
            _tile = GetComponentInParent<PlatformTileScript>();
            _collider = _tile.GetComponent<Collider2D>();
            _playerMovement = PlayerMovementScript.Instance;
            _playerCollider = _playerMovement.GetComponent<Collider2D>();
            _platformLayer = LayerMask.NameToLayer("Platform");
            _platformIgnoredLayer = LayerMask.NameToLayer("PlatformIgnored");
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other != _playerCollider) return;
            ColliderDistance2D dist = _collider.Distance(_playerCollider);
            float y = dist.normal.y * dist.distance;
            bool shouldBeEnabled = dist.isOverlapped ? _tile.gameObject.layer == _platformLayer : y < 0;
            _tile.gameObject.layer = shouldBeEnabled ? _platformLayer : _platformIgnoredLayer;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other == _playerCollider)
            {
                _tile.gameObject.layer = _platformIgnoredLayer;
            }
        }
    }
}
