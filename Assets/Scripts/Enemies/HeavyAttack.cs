using UnityEngine;

namespace Enemies
{
    public class HeavyAttack : MonoBehaviour
    {
        public float attackDistance;
        public float attackWidth;
        public float knockback;

        [SerializeField] private CapsuleCollider2D _collider;
        [SerializeField] private LayerMask _playerLayer;
        private Vector2 _attackBoxCenter;
        private Vector2 _attackBoxSize;
        private Transform _player;


        private void Awake()
        {
            // _playerLayer = LayerMask.NameToLayer("Player");

        }

        private void Update()
        {
            if (PlayerInRange())
            {
                PerformAttack();
            }
        }

        /// <summary>
        ///  Determines whether player is within attack range
        /// </summary>
        private bool PlayerInRange()
        {
            _attackBoxCenter = _collider.bounds.center + transform.right * attackDistance * transform.localScale.x;
            _attackBoxSize = new Vector3(_collider.bounds.size.x * attackWidth, _collider.bounds.size.y, _collider.bounds.size.z);

            RaycastHit2D hit = Physics2D.BoxCast(_attackBoxCenter, _attackBoxSize, 0,
                Vector2.left, 0, _playerLayer);

            if (hit.collider != null)
            {
                _player = hit.collider.transform;
            }

            return hit.collider != null;
        }

        /// <summary>
        /// Visualization of attack range
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_collider.bounds.center + transform.right * attackDistance * transform.localScale.x,
                new Vector3(_collider.bounds.size.x * attackWidth, _collider.bounds.size.y, _collider.bounds.size.z));
        }

        /// <summary>
        /// attacks player after set amount of delay depending on animation, goes into cooldown afterwards
        /// </summary>
        private void PerformAttack()
        {

        }

    }
}