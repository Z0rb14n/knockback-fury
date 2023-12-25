using UnityEngine;
using Player;

namespace Enemies
{
    [RequireComponent(typeof(EnemyAggro))]
    public class HeavyMovement : PatrolMovement
    {
        public float aggroSpeedMultiplier;


        private EnemyAggro _aggroScript;
        private bool _isAggro;
        private PlayerMovementScript _playerMovement;
        private Transform _player;
        private Vector2 _playerPos;

        protected override void Start()
        {
            base.Start();
            _aggroScript = GetComponent<EnemyAggro>();
            _playerMovement = PlayerMovementScript.Instance;
            _player = _playerMovement.transform;
        }

        // Update is called once per frame
        protected override void Update()
        {
            _isAggro = _aggroScript.IsAggro();
            _canMove = DetermineCanMove();
            _position = _collider2D.bounds.center;

            if (_isAggro)
            {
                if (!_isAttacking)
                {
                    _direction = (int)Mathf.Sign(_player.position.x - _body.position.x);
                }

                _playerPos = new Vector2(_player.position.x, transform.position.y);
                MoveToTarget(_playerPos, speed * aggroSpeedMultiplier);
                if (_isAttacking) Debug.Log("is attacking");
                if (_canMove && !_isAttacking)
                {
                    Debug.Log(_isAttacking.ToString());
                    CheckIfFlip();
                }
            }
            else
            {
                base.Update();
            }
        }
    }
}