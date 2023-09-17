using System.Collections;
using UnityEngine;

namespace Enemies
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public class PatrolMovement : MonoBehaviour
    {

        public Transform[] patrolPoints;
        public float speed;
        public float pauseTime;

        protected int _target;
        protected Vector2 _targetPos;
        protected int _direction;
        protected Rigidbody2D _body;
        protected SpriteRenderer _sprite;
        protected int _spriteDirection;
        protected float _switchTargetDistance;
        protected bool _canMove;
        private Collider2D _collider2D;

        private float _originalSpeed;
        

        

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
            if (patrolPoints.Length > 0)
            {
                _target = 0;
                _targetPos = new Vector2(patrolPoints[0].position.x, transform.position.y);   
            }
            _spriteDirection = 1;
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
        /// Determine movement direction, change targets if needed
        /// </summary>
        protected void DoCommonUpdates()
        {
            DetermineDirection();
            if (Vector2.Distance(transform.position, _targetPos) < _switchTargetDistance)
            {
                SwitchTargets();
                StartCoroutine(PauseAtDestination());
            }
        }

        protected void MoveToTarget(Vector2 target, float speed)
        {
            if (_canMove)
            {
                transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
            }
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

        public int GetDirection()
        {
            return _direction;
        }
    }
}