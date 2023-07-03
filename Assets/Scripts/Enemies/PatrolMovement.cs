using System.Collections;
using UnityEngine;

namespace Enemies
{
    public class PatrolMovement : MonoBehaviour
    {

        public Transform[] patrolPoints;
        public float speed;
        public float pauseTime;

        protected int _target;
        protected Vector2 _targetPos;
        protected float _direction;
        protected Rigidbody2D _body;
        protected SpriteRenderer _sprite;
        protected int _spriteDirection;
        protected float _switchTargetDistance;
        
        private float _originalSpeed;

        

        private void Start()
        {
            InitializeCommonVariables();
            _originalSpeed = speed;
            _switchTargetDistance = 0.2f;
        }

        protected void InitializeCommonVariables()
        {
            _body = GetComponent<Rigidbody2D>();
            _target = 0;
            _targetPos = new Vector2(patrolPoints[0].position.x, transform.position.y);
            _spriteDirection = 1;
        }


        private void Update()
        {
            DoCommonUpdates();
            MoveToTarget();
            CheckIfFlip();
        }

        protected void DoCommonUpdates()
        {
            DetermineDirection();
            if (Vector2.Distance(transform.position, _targetPos) < _switchTargetDistance)
            {
                SwitchTargets();
                StartCoroutine(PauseAtDestination());
            }
        }

        private void MoveToTarget()
        {
            transform.position = Vector2.MoveTowards(transform.position, _targetPos, speed * Time.deltaTime);
        }

        // checks if sprite needs flipping; if intended movement direction and sprite direction don't match,
        // flip sprite and update _spriteDirection to match
        protected void CheckIfFlip()
        {
            if (_direction != _spriteDirection)
            {
                Vector3 currentScale = gameObject.transform.localScale;
                currentScale.x *= -1;
                gameObject.transform.localScale = currentScale;
                _spriteDirection *= -1;
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
            _direction = Mathf.Sign(patrolPoints[_target].position.x - _body.position.x);
        }

        protected IEnumerator PauseAtDestination()
        {
            speed = 0;
            yield return new WaitForSeconds(pauseTime);
            speed = _originalSpeed;
        }

    }
}