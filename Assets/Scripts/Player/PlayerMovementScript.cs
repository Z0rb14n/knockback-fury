using System.Collections;
using System.Collections.Generic;
using CustomTiles;
using DashVFX;
using GameEnd;
using UnityEngine;
using Upgrades;
using Weapons;
using UnityEngine.SceneManagement;

namespace Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D),
         typeof(MeshTrail),
         typeof(PlayerUpgradeManager))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerMovementScript : MonoBehaviour
    {
        [Min(0), Tooltip("Affects the speed of the player")]
        public float maxSpeed = 69;
        [Min(0), Tooltip("Smoothness of Speed Changes")]
        public float speedSmoothness = 0.5f;
        [Min(0), Tooltip("Jump Impulse")]
        public float jumpForce = 10;
        [Tooltip("Wall Jump Impulse")]
        public Vector2 wallJumpForce = new(10, 5);
        [Min(0), Tooltip("Dash movement per physics update")]
        public float dashSpeed = 1;
        [Min(0), Tooltip("Time in Air Dash")]
        public float dashTime = 1;
        [Min(0), Tooltip("Seconds you freeze in the air when initiating a dash")]
        public float dashStartDelay = 1;
        [Min(0), Tooltip("Max number of dashes upon landing")]
        public int maxDashes = 1;
        [Min(0), Tooltip("Slide speed when on a wall")]
        public float slideSpeed = 0.05f;

        [Header("New Movement")]
        [Tooltip("Whether new movement is enabled.")]
        public bool newMovementEnabled = true;
        [Min(0), Tooltip("Acceleration (scaled by time)")]
        public float accel = 50;
        [Min(0), Tooltip("Acceleration multiplier when turning around")]
        public float turnAroundMultiplier = 2f;
        [Min(0), Tooltip("Deceleration (scaled by time) when grounded")]
        public float decel = 20;
        [Min(0), Tooltip("Deceleration (scaled by time) when airborne")]
        public float decelAirborne = 5;
        [Min(0), Tooltip("Deceleration (scaled by time) when above max speed")]
        public float decelWhenAbove = 5;
        [Min(0), Tooltip("The height of the short jump compared to the high jump")]
        public float shortJumpPercentage = 0.75f;
        [Min(0), Tooltip("How much earlier jump can be pressed")]
        public float earlyJumpLeeway = 10;
        [Min(0), Tooltip("How much later jump can be pressed")]
        public float lateJumpLeeway = 3;
        [Min(0), Tooltip("How long jump needs to be held to jump higher")]
        public float highJumpTime = 3;
        [Min(0), Tooltip("Time on a wall before walljump is enabled")]
        public float minTimeBeforeWallJump = 0.15f;

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
        public bool CanMove { get; set; } = true;

        public Vector2 Velocity => _body.velocity;

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
        private bool _hasKeepingInStrideDash;
        private bool _hasMomentumDash;
        private SpriteRenderer _sprite;
        private readonly List<PlatformTileScript> _platformsOn = new();
        private float _earlyJumpTime = 0;
        private float _lateJumpTime = 0;
        private float _jumpTime = 0;
        private bool _isHoldingJump = false;
        private float _timeOnWall = 0;

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
            _sprite = GetComponent<SpriteRenderer>();
            InitializeContactFilters();
        }

        private void InitializeContactFilters()
        {
            _physicsCheckMask = LayerMask.GetMask("Default", "Platform");
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

        private void HorizontalMovementLogic(float xInput)
        {
            if (xInput != 0)
            {
                _sprite.flipX = xInput < 0;
            }
            if (!newMovementEnabled)
            {
                if (xInput != 0)
                {
                    _speed = Mathf.SmoothDamp(_speed, xInput * maxSpeed, ref _currentVelocity, speedSmoothness);
                    _body.velocity = new Vector2(_speed, _body.velocity.y);
                }
            }
            else
            {
                float originalX = _body.velocity.x;
                float newX = originalX;

                if (xInput != 0)
                {
                    float normalAccel = accel * Time.deltaTime;
                    if (originalX > 0 && xInput < 0)
                    {
                        newX -= Mathf.Min(Mathf.Max(originalX, normalAccel), normalAccel * turnAroundMultiplier);
                    } else if (originalX < 0 && xInput > 0)
                    {
                        newX += Mathf.Min(Mathf.Max(-originalX, normalAccel), normalAccel * turnAroundMultiplier);
                    } else if (Mathf.Abs(originalX) < maxSpeed)
                    {
                        newX += xInput * Mathf.Min(maxSpeed - Mathf.Abs(originalX), normalAccel);
                    }
                }
                else
                {
                    float normalDecel = (Grounded ? decel : decelAirborne) * Time.deltaTime;
                    newX -= Mathf.Sign(originalX) * Mathf.Min(Mathf.Abs(originalX), normalDecel);
                }

                if (Mathf.Abs(originalX) > maxSpeed)
                {
                    float diff = Mathf.Abs(originalX) - maxSpeed;
                    float normalDecel = decelWhenAbove * Time.deltaTime;
                    newX -= Mathf.Sign(originalX) * Mathf.Min(normalDecel, diff);
                }

                _body.velocity = new Vector2(newX, _body.velocity.y);
            }
        }

        private void JumpLogic()
        {
            //jump
            if (Grounded || _lateJumpTime > 0)
            {
                if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)
                    || Input.GetKeyDown(KeyCode.Space) || _earlyJumpTime > 0))
                {
                    _body.velocity = new Vector2(_body.velocity.x, jumpForce * shortJumpPercentage);
                    _isHoldingJump = (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)
                        || Input.GetKey(KeyCode.Space));
                    _earlyJumpTime = 0;
                }
            }
            //pressing jump early
            else
            {
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)
                    || Input.GetKeyDown(KeyCode.Space))
                {
                    _earlyJumpTime = earlyJumpLeeway;
                }
            }
            //let go of jump
            if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W)
                || Input.GetKeyUp(KeyCode.Space))
            {
                _isHoldingJump = false;
                _jumpTime = 0;
            }
        }

        private void AdditionalJumpLogic()
        {
            if (!(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)
                || Input.GetKeyDown(KeyCode.Space)))
            {
                _earlyJumpTime -= Time.deltaTime;
                _lateJumpTime -= Time.deltaTime;
            }
            //holding jump
            if (_isHoldingJump)
            {
                _jumpTime += Time.deltaTime;
            }
            //small or big jump
            if (_isHoldingJump && _jumpTime > highJumpTime)
            {
                _body.velocity = new Vector2(_body.velocity.x, jumpForce * 0.9f);
                _isHoldingJump = false;
            }
        }

        private void Update()
        {
            if (Grounded)
            {
                _dashesRemaining = maxDashes;
                _hasMomentumDash = false;
                _hasKeepingInStrideDash = false;
            }
            if (!CanMove) return;
            float xInput = Input.GetAxisRaw("Horizontal");
            switch (xInput)
            {
                case > 0 when IsOnRightWall:
                case < 0 when IsOnLeftWall:
                    xInput = 0;
                    break;
            }

            HorizontalMovementLogic(xInput);

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)
                || Input.GetKeyDown(KeyCode.UpArrow)) _jumpRequest = true;

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

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                foreach (PlatformTileScript platform in _platformsOn) platform.TemporarilyIgnore();
            }

            JumpLogic();
        }

        private void FixedUpdate()
        {
            if (_jumpRequest)
            {
                if (!Grounded && _timeOnWall > minTimeBeforeWallJump)
                {
                    if (IsOnLeftWall)
                    {
                        _body.AddForce(new Vector2(wallJumpForce.x, wallJumpForce.y), ForceMode2D.Impulse);
                        OnWallLaunch();
                    }
                    else if (IsOnRightWall)
                    {
                        _body.AddForce(new Vector2(-wallJumpForce.x, wallJumpForce.y), ForceMode2D.Impulse);
                        OnWallLaunch();
                    }
                }
                _jumpRequest = false;
            }

            //pressed jump early or late or short
            AdditionalJumpLogic();

            if (_knockbackRequest)
            {
                _body.AddForce(_knockbackVector, ForceMode2D.Impulse);
                _knockbackVector = Vector2.zero;
                _knockbackRequest = false;
            }
            float xInput = Input.GetAxisRaw("Horizontal");
            bool isSlidingThisFrame = false;

            bool holdingDown = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
            if (!Grounded && _body.velocity.y < 0 && !holdingDown)
            {
                if (IsOnLeftWall || IsOnRightWall)
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

        public void RequestKnockback(Vector2 dir, float str, bool isWeapon = false) => RequestKnockback(dir * str, isWeapon);

        public void RequestKnockback(Vector2 vec, bool isWeapon = false)
        {
            if (isWeapon && Grounded) return;
            // honestly shouldn't really matter if it's here or just an addForce call
            // but this *feels* slower/unclean but idk
            _knockbackRequest = true;
            _knockbackVector += vec;
        }

        public void OnEnemyKill()
        {
            if (GameEndCanvas.Instance)
            {
                GameEndCanvas.Instance.endData.enemiesKilled++;
            }
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

            _timeOnWall += Time.fixedDeltaTime;
        }

        private void OnWallLaunch()
        {
            if (PlayerUpgradeManager.Instance[UpgradeType.Momentum] > 0) _hasMomentumDash = true;
            PlayerHealth.Instance.OnWallLaunch();
            PlayerWeaponControl.Instance.OnWallLaunch();
            ResetDash();
        }

        private void ResetDash()
        {
            if (_dashesRemaining == 0)
            {
                _dashesRemaining = 1;
            }
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
            _body.constraints = RigidbodyConstraints2D.FreezeAll;
            yield return new WaitForSeconds(dashStartDelay);
            _body.constraints = RigidbodyConstraints2D.FreezeRotation;

            _meshTrail.StartDash(_sprite.flipX);

            if (_upgradeManager[UpgradeType.SleightOfPaws] > 0) _weapon.ImmediateReload();

            float gravity = _body.gravityScale;
            _body.gravityScale = 0;
            _body.velocity = _dashDirection * dashSpeed;
            yield return new WaitForSeconds(ActualDashTime);
            _body.gravityScale = gravity;

            _dashing = false;
            _meshTrail.StopDash();
        }

        public void AddPlatformOn(PlatformTileScript platform)
        {
            _platformsOn.Add(platform);
        }

        public void RemovePlatformOn(PlatformTileScript platform)
        {
            _platformsOn.Remove(platform);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (_body.velocity.y < 0)
            {
                _lateJumpTime = lateJumpLeeway;
            }
            _timeOnWall = 0;
        }
    }
}
