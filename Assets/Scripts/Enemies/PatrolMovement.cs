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
        protected int _direction;
        protected Rigidbody2D _body;
        protected SpriteRenderer _sprite;
        protected int _spriteDirection;
        protected float _switchTargetDistance;
        protected bool _canMove;
        protected Collider2D _collider2D;
        protected Vector2 _position;
        protected bool _isAttacking;

        private float _originalSpeed;
        private Vector2 _colliderSize;
        

        protected virtual void Start()
        {
            InitializeCommonVariables();
            _originalSpeed = speed;
            _switchTargetDistance = 0.2f;
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
                _targetPos = new Vector2(patrolPoints[0].position.x, transform.position.y);   
            }
            _spriteDirection = 1;
            _isAttacking = false;
        }


         protected virtual void Update()
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
            _targetPos.y = transform.position.y;
            _position = _collider2D.bounds.center;
            _canMove = DetermineCanMove();
            if (!_isAttacking) DetermineDirection();
            if (Vector2.Distance(transform.position, _targetPos) < _switchTargetDistance)
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
                        if (_direction == 1)
                        {
                            // Debug.Log("movepos right");
                            // Debug.Log("obstacleHeight: " + obstacleHeight.ToString());
                            _body.MovePosition(_body.position + new Vector2(0.5f, obstacleHeight + 0.1f));
                        } else
                        {
                            // Debug.Log("movepos left");
                            // Debug.Log("obstacleHeight: " + obstacleHeight.ToString());
                            _body.MovePosition(_body.position + new Vector2(-0.5f, obstacleHeight + 0.1f));
                        }
                    }
                }
                else
                {
                    transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
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
            if (_direction == 1)
            {
                frontFootPos = _position + new Vector2(_colliderSize.x / 2, -_colliderSize.y / 2 + 0.1f);
                rayDirection = Vector2.right;
            }
            else
            {
                frontFootPos = _position + new Vector2(-_colliderSize.x / 2, -_colliderSize.y / 2 + 0.1f);
                rayDirection = Vector2.left;
            }
            int layerMask = ~(1 << LayerMask.NameToLayer("Enemy")
                | 1 << LayerMask.NameToLayer("Player"));

            Debug.DrawRay(frontFootPos, rayDirection, Color.blue);

            RaycastHit2D hit = Physics2D.Raycast(frontFootPos, rayDirection, 0.1f, layerMask);
            if (hit.collider != null)
            {
                // Debug.Log("Collided with: " + hit.collider.name);
                return true;
            }

            return false;
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
            int layerMask = ~(1 << LayerMask.NameToLayer("Enemy")
                | 1 << LayerMask.NameToLayer("Player"));
            if (_direction == 1)
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
            Debug.DrawRay(frontTopCornerPos, rayDirection, Color.cyan);
            if (Physics2D.Raycast(frontTopCornerPos, rayDirection, checkAheadDist, layerMask)) return -1.0f;
            
            // vertical rays up and down from a little bit ahead of head level
            RaycastHit2D hitDown = Physics2D.Raycast(verticalCheckOrigin, Vector2.down, colliderHeight, layerMask);
            Debug.DrawRay(verticalCheckOrigin, Vector2.down, Color.cyan);
            float distanceToObstacle = hitDown.distance;
            float obstacleHeight = colliderHeight - distanceToObstacle;
            
            // Debug.Log("distance: " + distanceToObstacle.ToString());
            // Debug.Log("maxFallHeight: " + maxFallHeight.ToString());
            // Debug.Log("colliderHeight: " + colliderHeight.ToString());
            // Debug.Log("obstacleHeight: " + obstacleHeight.ToString());
            if (obstacleHeight >= maxFallHeight) return -1.0f;
            
            float rayUpDistance = colliderHeight - distanceToObstacle + 0.2f;
            RaycastHit2D hitUp = Physics2D.Raycast(verticalCheckOrigin, Vector2.up, rayUpDistance, layerMask);
            Debug.DrawRay(verticalCheckOrigin, Vector2.up, Color.cyan);

            if (hitUp) return -1.0f;
            else return obstacleHeight;
        }

        // checks if sprite needs flipping; if intended movement direction and sprite direction don't match,
        // flip sprite and update _spriteDirection to match
        public void CheckIfFlip()
        {
            if (_direction != _spriteDirection)
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
                _targetPos = new Vector2(patrolPoints[0].position.x, transform.position.y);
            }
            else
            {
                _target = 1;
                _targetPos = new Vector2(patrolPoints[1].position.x, transform.position.y);
            }
        }

        protected bool DetermineCanMove()
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

        protected void DetermineDirection()
        {
            _direction = (int) Mathf.Sign(patrolPoints[_target].position.x - _body.position.x);
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

        public int GetDirection()
        {
            return _direction;
        }
    }
}