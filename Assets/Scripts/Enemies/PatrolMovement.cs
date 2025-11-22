using System.Collections;
using UnityEngine;

namespace Enemies
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public class PatrolMovement : EnemyBehaviour
    {
        public Transform[] patrolPoints;
        public float speed;
        public float pauseTime;
        public float maxFallHeight;

        protected int _target;
        protected Vector2 _targetPos;
        public int Direction { get; protected set; }
        protected Rigidbody2D _body;
        protected SpriteRenderer _sprite;
        protected int _spriteDirection;
        [SerializeField]
        protected float switchTargetDistance = 0.2f;
        protected bool _canMove;
        protected Collider2D _collider2D;
        protected Vector2 _position;
        protected bool _isAttacking;
        [SerializeField]
        protected LayerMask obstacleLayerMask;
        [SerializeField] private Rigidbody2D.SlideMovement slideMovement = new()
        {
            maxIterations = 2,
            gravity = new Vector2(0, -9.81f)
        };
        
        private float _originalSpeed;
        private Vector2 _colliderSize;

        protected virtual void Start()
        {
            InitializeCommonVariables();
            _originalSpeed = speed;
            _canMove = true;
        }

        protected void InitializeCommonVariables()
        {
            _body = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _collider2D = GetComponent<Collider2D>();
            _colliderSize = _collider2D.bounds.size;
            if (patrolPoints.Length > 0)
            {
                _target = 0;
                _targetPos = new Vector2(patrolPoints[0].position.x, _body.position.y);
            }

            _spriteDirection = 1;
            _isAttacking = false;
        }


        protected virtual void FixedUpdate()
        {
            if (patrolPoints.Length > 0)
            {
                DoCommonUpdates();
                MoveToTarget(_targetPos, speed);
                CheckIfFlip();
            }
        }

        /// <summary>
        /// Determine movement direction, change targets if needed, finds position
        /// </summary>
        protected void DoCommonUpdates()
        {
            _targetPos.y = _body.position.y; // Jon TODO: ?????
            _position = _collider2D.bounds.center;
            _canMove = DetermineCanMove();
            if (!_isAttacking) DetermineDirection();
            if (Vector2.Distance(_body.position, _targetPos) < switchTargetDistance)
            {
                SwitchTargets();
                StartCoroutine(PauseAtDestination());
            }
        }

        protected void MoveToTarget(Vector2 target, float speed)
        {
            if (_canMove && !_isAttacking)
            {
                if (HasObstacle())
                {
                    // Debug.Log("obstacle detected");
                    float obstacleHeight = HasSpaceToMoveUp();
                    if (obstacleHeight != -1f)
                    {
                        // Debug.Log("attempting to move up");
                        if (Direction == 1)
                        {
                            // Debug.Log("movepos right");
                            // Debug.Log("obstacleHeight: " + obstacleHeight.ToString());
                            _body.MovePosition(_body.position + new Vector2(0.5f, obstacleHeight + 0.1f));
                        }
                        else
                        {
                            // Debug.Log("movepos left");
                            // Debug.Log("obstacleHeight: " + obstacleHeight.ToString());
                            _body.MovePosition(_body.position + new Vector2(-0.5f, obstacleHeight + 0.1f));
                        }
                    }
                }
                else
                {
                    Vector2 velocity = (target - _body.position).normalized * speed;
                    _body.Slide(velocity, Time.fixedDeltaTime, slideMovement);
                }
            }
        }

        /// <summary>
        /// check for any obstacles ahead at foot level
        /// </summary>
        /// <returns> if there is an obstacle </returns>
        private bool HasObstacle()
        {
            Vector2 frontFootPos;
            Vector2 rayDirection;
            if (Direction == 1)
            {
                frontFootPos = _position + new Vector2(_colliderSize.x / 2, -_colliderSize.y / 2 + 0.1f);
                rayDirection = Vector2.right;
            }
            else
            {
                frontFootPos = _position + new Vector2(-_colliderSize.x / 2, -_colliderSize.y / 2 + 0.1f);
                rayDirection = Vector2.left;
            }

            // Debug.DrawRay(frontFootPos, rayDirection, Color.blue);

            RaycastHit2D hit = Physics2D.Raycast(frontFootPos, rayDirection, 0.1f, obstacleLayerMask);
            return hit.collider;
        }


        /// <summary>
        /// check if there is space for enemy to move up stairs-like obstacles
        /// REQUIRES: there is an obstacle ahead
        /// </summary>
        /// <returns> height of obstacle that can be moved up, -1 if no space </returns>
        private float HasSpaceToMoveUp()
        {
            float checkAheadDist = 0.1f;
            Vector2 rayDirection;
            Vector2 frontTopCornerPos;
            Vector2 verticalCheckOrigin;
            float colliderHeight = _colliderSize.y;
            if (Direction == 1)
            {
                frontTopCornerPos = _position + new Vector2(_colliderSize.x / 2, colliderHeight / 2);
                rayDirection = Vector2.right;
                verticalCheckOrigin = frontTopCornerPos + new Vector2(checkAheadDist, 0);
            }
            else
            {
                frontTopCornerPos = _position - new Vector2(_colliderSize.x / 2, -colliderHeight / 2);
                rayDirection = Vector2.left;
                verticalCheckOrigin = frontTopCornerPos - new Vector2(checkAheadDist, 0);
            }

            // initial horizontal ray; tests if there are obstacles at head (top corner) level ahead
            // Debug.DrawRay(frontTopCornerPos, rayDirection, Color.cyan);
            if (Physics2D.Raycast(frontTopCornerPos, rayDirection, checkAheadDist, obstacleLayerMask)) return -1.0f;

            // vertical rays up and down from a little bit ahead of head level
            RaycastHit2D hitDown =
                Physics2D.Raycast(verticalCheckOrigin, Vector2.down, colliderHeight, obstacleLayerMask);
            // Debug.DrawRay(verticalCheckOrigin, Vector2.down, Color.cyan);
            float distanceToObstacle = hitDown.distance;
            float obstacleHeight = colliderHeight - distanceToObstacle;

            if (obstacleHeight >= maxFallHeight) return -1.0f;

            float rayUpDistance = colliderHeight - distanceToObstacle + 0.2f;
            RaycastHit2D hitUp = Physics2D.Raycast(verticalCheckOrigin, Vector2.up, rayUpDistance, obstacleLayerMask);
            // Debug.DrawRay(verticalCheckOrigin, Vector2.up, Color.cyan);

            if (hitUp) return -1.0f;
            else return obstacleHeight;
        }

        // checks if sprite needs flipping; if intended movement direction and sprite direction don't match,
        // flip sprite and update _spriteDirection to match
        public void CheckIfFlip()
        {
            if (Direction != _spriteDirection)
            {
                _spriteDirection *= -1;
                _sprite.flipX = _spriteDirection < 0;
                if (_collider2D) _collider2D.offset *= new Vector2(-1, 1);
            }
        }

        protected void SwitchTargets()
        {
            if (_target == 1)
            {
                _target = 0;
                _targetPos = new Vector2(patrolPoints[0].position.x, _body.position.y);
            }
            else
            {
                _target = 1;
                _targetPos = new Vector2(patrolPoints[1].position.x, _body.position.y);
            }
        }

        protected bool DetermineCanMove()
        {
            float xPos = Direction == 1 ? _collider2D.bounds.max.x : _collider2D.bounds.min.x;
            Vector2 position = new(xPos, _collider2D.bounds.center.y);
            // Debug.DrawRay(position, rayDirection, Color.black);
            return Physics2D.Raycast(position, Vector2.down, maxFallHeight, obstacleLayerMask);
        }

        protected void DetermineDirection()
        {
            Direction = (int)Mathf.Sign(patrolPoints[_target].position.x - _body.position.x);
        }

        protected IEnumerator PauseAtDestination()
        {
            speed = 0;
            yield return new WaitForSeconds(pauseTime);
            speed = _originalSpeed;
        }

        public void EnableMovement()
        {
            _canMove = true;
        }

        public void DisableMovement()
        {
            _canMove = false;
        }

        public void StartAttack()
        {
            _isAttacking = true;
        }

        public void EndAttack()
        {
            _isAttacking = false;
        }
    }
}