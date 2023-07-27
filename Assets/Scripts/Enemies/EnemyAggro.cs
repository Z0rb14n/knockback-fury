using UnityEngine;

namespace Enemies
{
    public class GroundAggro : MonoBehaviour
    {
        public float aggroRange;
        public float deaggroRange;

        [SerializeField] private CapsuleCollider2D _collider;
        private Vector3 _position;
        private Transform _player;
        private LayerMask _playerLayer;

        private void Awake()
        {
            _position = _collider.bounds.center;
            _playerLayer = LayerMask.GetMask("Player");
        }

        /// <summary>
        /// Targets player when player enters aggro range. Deaggro only if player exits deaggro range
        /// </summary>
        private void Update()
        {
            if (IsInRange(aggroRange) && InLineOfSight())
            {
                Debug.Log("aggro");

            }

            if (!IsInRange(deaggroRange))
            {

            }
        }

        private bool IsInRange(float range)
        {
            RaycastHit2D hit = Physics2D.CircleCast(_collider.bounds.center, range, Vector2.left, 0, _playerLayer);

            if (hit.collider != null)
            {
                _player = hit.collider.transform;
            }

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

            return (Physics2D.Raycast(_position, rayDirection).transform == _player);
        }

    }
}