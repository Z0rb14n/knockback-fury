using System.Collections;
using UnityEngine;

namespace Enemies
{
    public class JumperMovement : PatrolMovement
    {

        public float jumpForce;
 

        private bool _canJump;
        private bool _isTouchingSurface;
        private ContactFilter2D _groundFilter;
        private ContactFilter2D _leftWallFilter;
        private ContactFilter2D _rightWallFilter;
        private int _physicsCheckMask;
        private bool _canResetVelocity;
        private bool _isStunned;
        

        private bool Grounded => _body.IsTouching(_groundFilter);
        private bool IsOnLeftWall => _body.IsTouching(_leftWallFilter);
        private bool IsOnRightWall => _body.IsTouching(_rightWallFilter);


        private void Start()
        {
            InitializeCommonVariables();
            InitializeContactFilters();
            _canJump = true;
            _canResetVelocity = true;
            _switchTargetDistance = 1f;
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
            if (patrolPoints.Length > 0)
                DoCommonUpdates();
            // jump if jumping cooldown is over and entity is in contact with a surface
            if (_canJump && _isTouchingSurface)
            {
                jump();
            }
        }

        private void StickOnWall()
        {
            if ((IsOnLeftWall || IsOnRightWall) && !_isStunned)
            {
                _body.gravityScale = 0;
                // safeguard against accidental calls to reset velocity after jumping off walls
                if (_canResetVelocity) _body.velocity = new Vector2(0, 0);
                _canResetVelocity = false;
            } 
            else _body.gravityScale = 1;
        }

        // Jumping: if on ground, simply add force
        //          if touching wall, wall jump
        // TODO: jump directions/targeting
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
            CheckIfFlip();
        }

        public void Stun()
        {
            StartCoroutine(StunDuration());
        }

        private IEnumerator StunDuration()
        {
            _isStunned = true;
            _body.gravityScale = 1;
            yield return new WaitForSeconds(0.5f);
            _isStunned = false;
        }


    }
}