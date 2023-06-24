using System.Collections;
using UnityEngine;

namespace Enemies
{
    public class JumperMovement : PatrolMovement
    {

        public float jumpForce;
 

        private Rigidbody2D _body;
        private bool _canJump;
        private bool _isTouchingSurface;
        private ContactFilter2D _groundFilter;
        private ContactFilter2D _leftWallFilter;
        private ContactFilter2D _rightWallFilter;
        private int _physicsCheckMask;
        private bool _canResetVelocity;
        private float _direction;
        

        private bool Grounded => _body.IsTouching(_groundFilter);
        private bool IsOnLeftWall => _body.IsTouching(_leftWallFilter);
        private bool IsOnRightWall => _body.IsTouching(_rightWallFilter);


        private void Start()
        {
            _target = 0;
            _targetPos = new Vector2(patrolPoints[0].position.x, transform.position.y);
            _body = GetComponent<Rigidbody2D>();
            _canJump = true;
            _canResetVelocity = true;
            InitializeContactFilters();
        }
        private void InitializeContactFilters()
        {
            _physicsCheckMask = LayerMask.GetMask("Default");
            _groundFilter = new ContactFilter2D
            {
                layerMask = _physicsCheckMask,
                useLayerMask = true,
                useNormalAngle = true,
                minNormalAngle = 30,
                maxNormalAngle = 150
            };
            _leftWallFilter = new ContactFilter2D
            {
                layerMask = _physicsCheckMask,
                useLayerMask = true,
                useNormalAngle = true,
                minNormalAngle = -60,
                maxNormalAngle = 60
            };
            _rightWallFilter = new ContactFilter2D
            {
                layerMask = _physicsCheckMask,
                useLayerMask = true,
                useNormalAngle = true,
                minNormalAngle = 120,
                maxNormalAngle = 240
            };
        }

        private void Update()
        {
            _isTouchingSurface = Grounded || IsOnLeftWall || IsOnRightWall;
            StickOnWall();
            DetermineDirection();

            // jump if jumping cooldown is over and entity is in contact with a surface
            if (_canJump && _isTouchingSurface)
            {
                jump();
            }

            if (Vector2.Distance(transform.position, _targetPos) < 1f)
            {
                SwitchTargets();
                StartCoroutine(PauseAtDestination());
            }
        }

        private void StickOnWall()
        {
            if (IsOnLeftWall || IsOnRightWall)
            {
                _body.gravityScale = 0;
                // safeguard against accidental calls to reset velocity after jumping off walls
                if (_canResetVelocity) _body.velocity = new Vector2(0, 0);
                _canResetVelocity = false;
            } 
            else _body.gravityScale = 1;
        }

        // Determines which direction the jumper jumps towards; _direction should only be either -1 or 1
        private void DetermineDirection()
        {
            _direction = Mathf.Sign(patrolPoints[_target].position.x - _body.position.x);
        }

        // Jumping: if on ground, simply add force
        //          if touching wall, wall jump
        // TODO: jump directions/targeting, sprite flipping
        private void jump()
        {
            if (Grounded)
            {
                _body.AddForce(new Vector2(100f * _direction, jumpForce * 100));
            } 
            else if (IsOnLeftWall)
            {
                _body.AddForce(new Vector2(100f, jumpForce * 100)); 
            } 
            else
            {
                _body.AddForce(new Vector2(-100f, jumpForce * 100));
            }
            _canResetVelocity = true;
            StartCoroutine(JumpCooldown());
        }

        private IEnumerator JumpCooldown()
        {
            _canJump = false;
            yield return new WaitForSeconds(4);
            _canJump = true;
        }



    }
}