using UnityEngine;
using Player;

namespace Enemies
{
    public class EnemyAggro : MonoBehaviour
    {
        public float aggroRange;
        public float deaggroRange;

        [SerializeField] private CapsuleCollider2D _collider;
        private Vector3 _position;
        private Transform _player;
        private LayerMask _playerLayer;
        private PlayerMovementScript _playerMovementScript;
        private bool _isAggro;

        private void Awake()
        {
            _playerLayer = LayerMask.GetMask("Player");
            _playerMovementScript = PlayerMovementScript.Instance;
            _player = _playerMovementScript.transform;
        }

        /// <summary>
        /// Targets player when player enters aggro range. Deaggro only if player exits deaggro range
        /// </summary>
        private void Update()
        {
            _position = _collider.bounds.center;
            if (IsInRange(aggroRange) && InLineOfSight())
            {
                _isAggro = true;

            }
            if (!IsInRange(deaggroRange))
            {
                _isAggro = false;
            }
        }

        private bool IsInRange(float range)
        {
            RaycastHit2D hit = Physics2D.CircleCast(_collider.bounds.center, range, Vector2.left, 0, _playerLayer);

            return hit.collider != null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_collider.bounds.center, aggroRange);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_collider.bounds.center, deaggroRange);
        }

        /// <summary>
        /// determines whether enemy can see player without obstructions
        /// </summary>
        private bool InLineOfSight()
        {
            Vector2 rayDirection = _player.position - _position;
            int layerMask = ~(1 << LayerMask.NameToLayer("Enemy"));
            Debug.DrawRay(_position, rayDirection, Color.red);
            try
            {
                return (Physics2D.Raycast(_position, rayDirection, deaggroRange, layerMask).collider.gameObject.layer
                    == LayerMask.NameToLayer("Player"));
            }
            catch (System.NullReferenceException)
            {
                return false;
            }
        }

        public bool IsAggro()
        {
            return _isAggro;
        }

        

    }
}