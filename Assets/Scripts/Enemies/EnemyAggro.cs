using UnityEngine;
using Player;

namespace Enemies
{
    public class EnemyAggro : MonoBehaviour
    {
        public float aggroRange;
        public float deaggroRange;
        public bool showAggroRanges;

        [SerializeField] private CapsuleCollider2D _collider;
        private Vector3 _position;
        private Transform _player;
        private LayerMask _lineOfSightMask;
        private PlayerMovementScript _playerMovementScript;
        public bool IsAggro { get; private set; }

        private void Awake()
        {
            _lineOfSightMask = ~LayerMask.GetMask("Enemy", "EnemyIgnorePlatform", "EnemyBomb", "Ignore Raycast");
            _playerMovementScript = PlayerMovementScript.Instance;
            _player = _playerMovementScript.transform;
        }

        /// <summary>
        /// Targets player when player enters aggro range. Deaggro only if player exits deaggro range
        /// </summary>
        private void Update()
        {
            _position = _collider.bounds.center;
            if (Vector2.Distance(_position, _playerMovementScript.Pos) <= aggroRange && InLineOfSight())
            {
                IsAggro = true;
            }

            if (Vector2.Distance(_position, _playerMovementScript.Pos) > deaggroRange)
            {
                IsAggro = false;
            }
        }

        private void OnDrawGizmos()
        {
            if (showAggroRanges)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(_collider.bounds.center, aggroRange);

                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_collider.bounds.center, deaggroRange);
            }
        }

        /// <summary>
        /// determines whether enemy can see player without obstructions
        /// </summary>
        private bool InLineOfSight()
        {
            Vector2 rayDirection = _player.position - _position;
            Debug.DrawRay(_position, rayDirection, Color.red);
            RaycastHit2D hit = Physics2D.Raycast(_position, rayDirection, deaggroRange, _lineOfSightMask);
            if (hit.collider != null)
            {
                Debug.DrawLine(_position, hit.point, Color.green);
                return hit.collider.gameObject.layer == LayerMask.NameToLayer("Player");
            }

            return false;
        }
    }
}