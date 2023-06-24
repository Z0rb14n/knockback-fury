using System.Collections;
using UnityEngine;

namespace Enemies
{
    public class PatrolMovement : MonoBehaviour
    {

        public Transform[] patrolPoints;
        public float _speed;
        public float _pauseTime;

        protected int _target;
        protected Vector2 _targetPos;
        private float _originalSpeed;

        private void Start()
        {
            _target = 0;
            _targetPos = new Vector2(patrolPoints[0].position.x, transform.position.y);
            _originalSpeed = _speed;
        }


        private void Update()
        {
            MoveToTarget();
            if (Vector2.Distance(transform.position, _targetPos) < 0.2f)
            {
                SwitchTargets();
                StartCoroutine(PauseAtDestination());
            }
        }

        private void MoveToTarget()
        {
            transform.position = Vector2.MoveTowards(transform.position, _targetPos, _speed * Time.deltaTime);
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

        protected IEnumerator PauseAtDestination()
        {
            _speed = 0;
            yield return new WaitForSeconds(_pauseTime);
            _speed = _originalSpeed;
        }

    }
}