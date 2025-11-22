using Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies
{
    /// <summary>
    /// MonoBehaviour to move enemies if stacked upon another enemy.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyAntiStack : MonoBehaviour
    {
        [SerializeField] private bool moveToPlayer;
        [SerializeField] private float force = 2;
        private Rigidbody2D _body;
        private ContactFilter2D _enemyBelowFilter;
        private PlayerMovementScript _player;
        private Vector2 _forceDir;
        private bool _isMotionActive = false;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _player = PlayerMovementScript.Instance;
            _enemyBelowFilter = new ContactFilter2D
            {
                layerMask = LayerMask.GetMask("Enemy", "EnemyIgnorePlatform"),
                useLayerMask = true,
                useNormalAngle = true,
                minNormalAngle = 225,
                maxNormalAngle = 315
            };
        }

        private void FixedUpdate()
        {
            if (_body.IsTouching(_enemyBelowFilter))
            {
                if (!_isMotionActive)
                {
                    _isMotionActive = true;
                    _forceDir = new Vector2(force * Mathf.Sign(Random.Range(-1f,1f)), 0);
                }
                Vector2 dir = moveToPlayer ? new Vector2(force * Mathf.Sign(_player.Pos.x - _body.position.x), 0) : _forceDir;
                _body.MovePosition(_body.position + dir * Time.fixedDeltaTime);
            }
            else
            {
                _isMotionActive = false;
            }
        }
    }
}