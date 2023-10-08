using UnityEngine;
using Player;

namespace Enemies
{
    [RequireComponent(typeof(EnemyAggro))]
    public class HeavyMovement : PatrolMovement
    {
        public float aggroSpeedMultiplier;
        public float maxFallHeight;

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
            
            if (_isAggro)
            {
                _direction = (int)Mathf.Sign(_player.position.x - _body.position.x);
                _playerPos = new Vector2(_player.position.x, transform.position.y);
                MoveToTarget(_playerPos, speed * aggroSpeedMultiplier);
                if (_canMove)
                {
                    CheckIfFlip();
                }
                
            }
            else
            {
                base.Update();
            }
        }

        private bool DetermineCanMove()
        {
            Vector2 rayDirection = Vector2.down;
            Vector2 position;
            int layerMask = ~(1 << LayerMask.NameToLayer("Enemy") 
                | 1 << LayerMask.NameToLayer("Player"));
            if (_direction == 1)
            {
                position = new Vector2(_collider2D.bounds.max.x, _collider2D.bounds.center.y);
            }
            else
            {
                position = new Vector2(_collider2D.bounds.min.x, _collider2D.bounds.center.y);
            }

            Debug.DrawRay(position, rayDirection, Color.black);
            return Physics2D.Raycast(position, rayDirection, maxFallHeight, layerMask);
        }

    }


}