using System.Collections;
using DashVFX;
using UnityEngine;
using Upgrades;
using Weapons;

namespace Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D),
         typeof(MeshTrail), 
         typeof(PlayerUpgradeManager))]
    public class PlayerMovementScript : MonoBehaviour
    {
        [Min(0), Tooltip("Affects the speed of the player")]
        public float maxSpeed = 69;
        [Min(0), Tooltip("Smoothness of Speed Changes")]
        public float speedSmoothness = 0.5f;
        [Min(0), Tooltip("Jump Impulse")]
        public float jumpForce = 10;
        [Tooltip("Wall Jump Impulse")]
        public Vector2 wallJumpForce = new(10,5);
        [Min(0), Tooltip("Dash movement per physics update")]
        public float dashSpeed = 1;
        [Min(0), Tooltip("Time in Air Dash")]
        public float dashTime = 1;
        [Min(0), Tooltip("Max number of dashes upon landing")]
        public int maxDashes = 1;
        [Min(0), Tooltip("Slide speed when on a wall")]
        public float slideSpeed = 0.05f;

        private float ActualDashTime => dashTime * (1 + _upgradeManager[UpgradeType.FarStride]);
        
        public static PlayerMovementScript Instance
        {
            get
            {
                if (_instance == null) _instance = FindObjectOfType<PlayerMovementScript>();
                return _instance;
            }
        }
        private static PlayerMovementScript _instance;
        
        public bool IsWallSliding { get; private set; }

        private PlayerUpgradeManager _upgradeManager;
        private Weapon _weapon;
        private int _dashesRemaining = 1;
        private MeshTrail _meshTrail;
        private ContactFilter2D _groundFilter;
        private ContactFilter2D _leftWallFilter;
        private ContactFilter2D _rightWallFilter;
        private Rigidbody2D _body;
        private bool _jumpRequest;
        private bool _knockbackRequest;
        private Vector2 _knockbackVector;
        private float _speed;
        private float _currentVelocity;
        private bool _dashing;
        private Vector2 _dashDirection;
        private int _physicsCheckMask;
        private bool _canMove;
        private bool _hasKeepingInStrideDash;
        private bool _hasMomentumDash;
    
        private bool Grounded => _body.IsTouching(_groundFilter);
        private bool IsOnLeftWall => _body.IsTouching(_leftWallFilter);
        private bool IsOnRightWall => _body.IsTouching(_rightWallFilter);

        private bool HasDash => _dashesRemaining > 0 || _hasMomentumDash || _hasKeepingInStrideDash;

        private void Awake()
        {
            _instance = this;
            _body = GetComponent<Rigidbody2D>();
            _meshTrail = GetComponent<MeshTrail>();
            _weapon = GetComponentInChildren<Weapon>();
            _upgradeManager = GetComponent<PlayerUpgradeManager>();
            _canMove = true;
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
            if (Grounded)
            {
                _dashesRemaining = maxDashes;
                _hasMomentumDash = false;
                _hasKeepingInStrideDash = false;
            }
            if (!_canMove) return;
            float xInput = Input.GetAxisRaw("Horizontal");
            switch (xInput)
            {
                case > 0 when IsOnRightWall:
                case < 0 when IsOnLeftWall:
                    xInput = 0;
                    break;
            }
            if (xInput != 0) 
            {
                _speed = Mathf.SmoothDamp(_speed, xInput * maxSpeed, ref _currentVelocity, speedSmoothness);
                _body.velocity = new Vector2(_speed, _body.velocity.y);
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) _jumpRequest = true;

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                float yInput = Input.GetAxisRaw("Vertical");
                if (!_dashing && HasDash && !Grounded && (xInput != 0 || yInput != 0))
                {
                    _dashing = true;
                    _dashDirection = new Vector2(xInput, yInput).normalized;
                    StartCoroutine(DashCoroutine());
                    if (_dashesRemaining > 0) _dashesRemaining--;
                    else if (_hasKeepingInStrideDash) _hasKeepingInStrideDash = false;
                    else if (_hasMomentumDash) _hasMomentumDash = false;
                }
            }
        }

        private void FixedUpdate()
        {
            if (_jumpRequest)
            {
                if (Grounded)
                    _body.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
                else if (IsOnLeftWall)
                {
                    _body.AddForce(new Vector2(wallJumpForce.x, wallJumpForce.y), ForceMode2D.Impulse);
                    OnWallLaunch();
                }
                else if (IsOnRightWall)
                {
                    _body.AddForce(new Vector2(-wallJumpForce.x,wallJumpForce.y),ForceMode2D.Impulse);
                    OnWallLaunch();
                }
                _jumpRequest = false;
            }

            if (_knockbackRequest)
            {
                _body.AddForce(_knockbackVector, ForceMode2D.Impulse);
                _knockbackVector = Vector2.zero;
                _knockbackRequest = false;
            }
            
            float xInput = Input.GetAxisRaw("Horizontal");
            bool isSlidingThisFrame = false;
            if (!Grounded && _body.velocity.y < 0 && xInput != 0)
            {
                if (IsOnLeftWall && xInput < 0)
                {
                    isSlidingThisFrame = true;
                    WallSlideLogic();
                }
                else if (IsOnRightWall && xInput > 0)
                {
                    isSlidingThisFrame = true;
                    WallSlideLogic();
                }
            }

            if (IsWallSliding && !isSlidingThisFrame)
            {
                PlayerWeaponControl.Instance.OnStopWallSlide();
                IsWallSliding = false;
            }
        }

        public void RequestKnockback(Vector2 dir, float str) => RequestKnockback(dir * str);

        public void RequestKnockback(Vector2 vec)
        {
            // honestly shouldn't really matter if it's here or just an addForce call
            // but this *feels* slower/unclean but idk
            _knockbackRequest = true;
            _knockbackVector += vec;
        }

        public void OnEnemyKill()
        {
            if (!Grounded) return;
            if (_upgradeManager[UpgradeType.KeepingInStride] > 0)
            {
                _hasKeepingInStrideDash = true;
            }
        }

        private void WallSlideLogic()
        {
            _body.velocity = new Vector2(_body.velocity.x, -slideSpeed);
            if (!IsWallSliding)
            {
                PlayerWeaponControl.Instance.OnStartWallSlide();
            }

            IsWallSliding = true;
        }

        private void OnWallLaunch()
        {
            if (PlayerUpgradeManager.Instance[UpgradeType.Momentum] > 0) _hasMomentumDash = true;
            PlayerHealth.Instance.OnWallLaunch();
            PlayerWeaponControl.Instance.OnWallLaunch();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!_dashing || _upgradeManager[UpgradeType.CloakAndDagger] <= 0) return;
            EntityHealth health = other.collider.GetComponent<EntityHealth>();
            if (health == null) return;
            health.TakeDamage(_upgradeManager.GetData(UpgradeType.CloakAndDagger));
        }

        private IEnumerator DashCoroutine()
        {
            _meshTrail.StartDash();
            _body.velocity = Vector2.zero;
            float thisDashTime = ActualDashTime;
            if (_upgradeManager[UpgradeType.SleightOfPaws] > 0) _weapon.ImmediateReload();
            for (float timePassed = 0; timePassed < thisDashTime; timePassed += Time.fixedDeltaTime)
            {
                if (_body.IsTouchingLayers(_physicsCheckMask)) break;
                Vector2 pos = _body.position;
                _body.MovePosition(pos + (_dashDirection * dashSpeed));
                yield return new WaitForFixedUpdate();
            }

            _dashing = false;
            _body.velocity = Vector2.zero;
            _meshTrail.StopDash();
        }

        public void StopMovement()
        {
            _canMove = false;
        }

        public void AllowMovement()
        {
            _canMove = true;
        }
    }
}
