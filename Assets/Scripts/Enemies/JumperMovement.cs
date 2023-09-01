using System.Collections;
using UnityEngine;

namespace Enemies
{
    [RequireComponent(typeof(Animator))]
    public class JumperMovement : PatrolMovement
    {
        [Tooltip("Jump force (x100)"), Min(0)]
        public float jumpForce;

        [SerializeField] private Collider2D normalCollider;
        [SerializeField] private Collider2D colliderOnWall;
        [SerializeField] private Collider2D colliderOnRightWall;
        [SerializeField, Tooltip("scuffed. Completely scuffed. We're completely scuffed.")] private Vector2 wallPositionOffset;
        
        private bool _canJump;
        private bool _isTouchingSurface;
        private ContactFilter2D _groundFilter;
        private ContactFilter2D _leftWallFilter;
        private ContactFilter2D _rightWallFilter;
        private int _physicsCheckMask;
        private bool _canResetVelocity;
        private bool _isStunned;
        private State _state = State.Idle;
        private Animator _animator;
        private static readonly int AnimationJumpHash = Animator.StringToHash("Jump");
        private static readonly int AnimationWallHash = Animator.StringToHash("IsOnWall");

        private bool Grounded => _body.IsTouching(_groundFilter);
        private bool IsOnLeftWall => _body.IsTouching(_leftWallFilter);
        private bool IsOnRightWall => _body.IsTouching(_rightWallFilter);

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

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
            if (_state == State.Jump)
                _state = State.JumpNextTick; // due to body.IsTouching only updating in 2 ticks
            // jump if jumping cooldown is over and entity is in contact with a surface
            if (_canJump && (_isTouchingSurface || _state == State.OnWall))
            {
                Jump();
            }
        }

        private void StickOnWall()
        {
            if (_isStunned || (!IsOnLeftWall && !IsOnRightWall)) return;
            if (_state == State.JumpNextTick)
            {
                _state = State.OnWall;
                _animator.SetBool(AnimationWallHash, true);
                if (IsOnRightWall)
                {
                    colliderOnRightWall.enabled = true;
                    transform.position -= (Vector3) wallPositionOffset;
                    _sprite.flipX = true;
                }
                else
                {
                    colliderOnWall.enabled = true;
                    _sprite.flipX = false;
                    transform.position += (Vector3)wallPositionOffset;
                }
                normalCollider.enabled = false;
            }
            _body.gravityScale = 0;
            // safeguard against accidental calls to reset velocity after jumping off walls
            if (_canResetVelocity)
            {
                _body.velocity = new Vector2(0, 0);
                _body.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            _canResetVelocity = false;
        }

        // Jumping: if on ground, simply add force
        //          if touching wall, wall jump
        // TODO: jump directions/targeting
        private void Jump()
        {
            _body.constraints = RigidbodyConstraints2D.FreezeRotation;
            bool shouldFlipSprite = false;
            if (Grounded)
            {
                _body.AddForce(new Vector2(100f * _direction, jumpForce * 100));
                shouldFlipSprite = _direction >= 0;
            } 
            else if (IsOnLeftWall)
            {
                _body.AddForce(new Vector2(100f, jumpForce * 100));
                shouldFlipSprite = true;
            } 
            else
            {
                _body.AddForce(new Vector2(-100f, jumpForce * 100));
            }

            _sprite.flipX = shouldFlipSprite;
            _animator.SetTrigger(AnimationJumpHash);
            if (_state == State.OnWall)
            {
                _animator.SetBool(AnimationWallHash, false);
                normalCollider.enabled = true;
                colliderOnWall.enabled = false;
                colliderOnRightWall.enabled = false;
                _body.constraints = RigidbodyConstraints2D.FreezeRotation;
                _body.gravityScale = 1;
            }
            _state = State.Jump;
            
            normalCollider.enabled = true;
            colliderOnWall.enabled = false;
            colliderOnRightWall.enabled = false;
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

        private enum State
        {
            OnWall, Idle, Jump, JumpNextTick
        }

    }
}