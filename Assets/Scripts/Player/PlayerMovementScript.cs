using System.Collections;
using System.Collections.Generic;
using CustomTiles;
using DashVFX;
using FileSave;
using GameEnd;
using Grapple;
using PermUpgrade;
using UnityEngine;
using Upgrades;
using Weapons;

namespace Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D),
         typeof(AbstractDashTrail),
         typeof(PlayerUpgradeManager))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerMovementScript : MonoBehaviour
    {
        [Header("Grounded Speed/accel")]
        [Min(0), Tooltip("Affects the speed of the player")]
        public float maxSpeed = 69;
        [Min(0), Tooltip("Acceleration (scaled by time)")]
        public float accel = 50;
        [Min(0), Tooltip("Deceleration (scaled by time)")]
        public float decel = 20;
        [Min(0), Tooltip("Acceleration multiplier when turning around")]
        public float turnAroundMultiplier = 2f;
        [Min(0), Tooltip("Deceleration (scaled by time) when above max speed")]
        public float decelWhenAbove = 5;
        [Min(0), Tooltip("Max Y speed")]
        public float maxYSpeed = 30;
        [Min(0), Tooltip("Deceleration (m/s2) when above max Y speed")]
        public float decelYWhenAbove = 5;
        public Rigidbody2D.SlideMovement slideMovement;
        [Header("KB Multiplier")]
        [Min(0), Tooltip("Grounded KB Multiplier for weapons")]
        public float groundedKBMultiplier = 2;
        [Header("Jump")]
        [Min(0), Tooltip("Jump Impulse")]
        public float jumpForce = 10;
        [Min(0), Tooltip("The height of the short jump compared to the high jump")]
        public float shortJumpPercentage = 0.75f;
        [Min(0), Tooltip("How much earlier jump can be pressed")]
        public float earlyJumpLeeway = 10;
        [Min(0), Tooltip("How much later jump can be pressed")]
        public float lateJumpLeeway = 3;
        [Min(0), Tooltip("Minimum amount of time spent jumping")]
        public float minJumpTime = 0.02f;
        [Min(0), Tooltip("How long jump needs to be held to jump higher")]
        public float maxJumpTime = 0.08f;
        [Min(0), Tooltip("Time on a wall before wall jump is enabled")]
        public float minTimeBeforeWallJump = 0.15f;
        [Tooltip("Wall Jump Impulse")]
        public Vector2 wallJumpForce = new(10, 5);
        [Header("Dash")]
        [Min(0), Tooltip("Dash movement per physics update")]
        public float dashSpeed = 1;
        [Min(0), Tooltip("Time in Air Dash")]
        public float dashTime = 1;
        [Min(0), Tooltip("Seconds you freeze in the air when initiating a dash")]
        public float dashStartDelay = 1;
        [Min(0), Tooltip("Max number of dashes upon landing")]
        public int maxDashes = 1;


        [Header("Grapple Hook")]
        [Min(0), Tooltip("Grapple Hook Velocity")]
        public float grappleVelocity = 10;
        [Min(0), Tooltip("Grapple Hook Cooldown")]
        public float grappleCooldown = 3;
        [Tooltip("Grappling hook prefab")]
        public GameObject grapplePrefab;

        private float ActualDashTime => dashTime * (1 + _upgradeManager[UpgradeType.FarStride]);

        public int EffectiveDashes => _dashesRemaining + (_hasMomentumDash ? 1 : 0) + (_hasKeepingInStrideDash ? 1 : 0);

        public float GrappleHookCooldown
        {
            get => Mathf.Max(0, grappleCooldown - (Time.time - _timeOfGrapple));
            set => _timeOfGrapple = Time.time - grappleCooldown + value;
        }

        private float _timeOfGrapple = float.MinValue;

        public static PlayerMovementScript Instance
        {
            get
            {
                if (instance == null) instance = FindAnyObjectByType<PlayerMovementScript>(FindObjectsInactive.Include);
                return instance;
            }
        }
        private static PlayerMovementScript instance;
        private static readonly int IsJumpingAnimatorHash = Animator.StringToHash("isJumping");
        private static readonly int XVelocityAnimatorHash = Animator.StringToHash("xVelocity");

        public bool CanMove { get; set; } = true;

        public bool CanGrapple { get; set; } = true;

        public Vector2 Velocity => _velocity;

        public Vector2 Pos
        {
            get => _body.position;
            set => _body.position = value;
        }

        private PlayerUpgradeManager _upgradeManager;
        private Weapon _weapon;
        private int _dashesRemaining = 1;
        private AbstractDashTrail _dashVfx;
        private ContactFilter2D _groundFilter;
        private ContactFilter2D _leftWallFilter;
        private ContactFilter2D _rightWallFilter;
        private ContactFilter2D _ceilingFilter;
        private Rigidbody2D _body;
        private Vector2 _velocity;
        private bool _dashing;
        private int _physicsCheckMask;
        private bool _hasKeepingInStrideDash;
        private bool _hasMomentumDash;
        private SpriteRenderer _sprite;
        private readonly List<IPlayerVelocityEffector> _playerVelocityEffectors = new();
        private readonly List<PlatformTileScript> _platformsOn = new();
        /// <summary>
        /// Maximum time just before hitting the ground that we're allowed to jump.
        /// </summary>
        /// <remarks>
        /// We can jump a bit early and it's apparently fine.
        /// </remarks>
        private float _earlyJumpTime;
        /// <summary>
        /// Maximum time since falling downwards from a collision that we're allowed to jump.
        /// </summary>
        /// <remarks>
        /// We can jump a bit late and it's apparently fine.
        /// </remarks>
        private float _lateJumpTime;
        private float _jumpTime;
        private bool _isJumping;
        private float _timeOnWall;
        private Camera _mainCam;
        private GrappleHook _activeGrappleHook;

        private bool Grounded => _body.IsTouching(_groundFilter);
        private bool HitCeiling => _body.IsTouching(_ceilingFilter);
        private bool IsOnLeftWall => _body.IsTouching(_leftWallFilter);
        private bool IsOnRightWall => _body.IsTouching(_rightWallFilter);

        private bool HasDash => _dashesRemaining > 0 || _hasMomentumDash || _hasKeepingInStrideDash;

        //animation stuff
        private Animator _animator;

        private void Awake()
        {
            instance = this;
            _body = GetComponent<Rigidbody2D>();
            _dashVfx = GetComponent<AbstractDashTrail>();
            _weapon = GetComponentInChildren<Weapon>();
            _upgradeManager = GetComponent<PlayerUpgradeManager>();
            _sprite = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>(); // <-- animation
            _mainCam = Camera.main;
            _velocity = Vector2.zero;
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
                minNormalAngle = -30,
                maxNormalAngle = 30
            };
            _rightWallFilter = new ContactFilter2D
            {
                layerMask = _physicsCheckMask,
                useLayerMask = true,
                useNormalAngle = true,
                minNormalAngle = 150,
                maxNormalAngle = 210
            };
            _ceilingFilter = new ContactFilter2D
            {
                layerMask = _physicsCheckMask,
                useLayerMask = true,
                useNormalAngle = true,
                minNormalAngle = -120,
                maxNormalAngle = -60
            };
        }

        /// <summary>
        /// Runs horizontal movement logic.
        ///
        /// Flips the sprite, and applies the following movement:
        /// <list type="bullet">
        /// <item><description>Moves in the direction as specified by the player axis</description></item>
        /// <item><description>If moving in the opposite direction from current motion,
        /// apply additional <see cref="turnAroundMultiplier"/> multiplier to the acceleration</description></item>
        /// <item><description>If above <see cref="maxSpeed"/>, decelerates at a rate of <see cref="decelWhenAbove"/></description></item>
        /// <item><description>If not moving, decelerates at a rate of <see cref="decel"/></description></item>
        /// </list>
        /// </summary>
        /// <param name="xInput">Horizontal input axis</param>
        private void HorizontalMovementLogic(float xInput)
        {
            float originalX = _velocity.x;
            float newX = originalX;

            if (xInput != 0)
            {
                float normalAccel = accel * Time.deltaTime;
                if (originalX > 0 && xInput < 0)
                {
                    newX -= Mathf.Min(Mathf.Max(originalX, normalAccel), normalAccel * turnAroundMultiplier);
                }
                else if (originalX < 0 && xInput > 0)
                {
                    newX += Mathf.Min(Mathf.Max(-originalX, normalAccel), normalAccel * turnAroundMultiplier);
                }
                else if (Mathf.Abs(originalX) < maxSpeed)
                {
                    newX += xInput * Mathf.Min(maxSpeed - Mathf.Abs(originalX), normalAccel);
                }
            }
            else
            {
                newX = Mathf.MoveTowards(newX, 0, decel * Time.deltaTime);
            }

            float speedAboveMax = Mathf.Abs(originalX) - maxSpeed;
            if (speedAboveMax > 0)
            {
                float normalDecel = decelWhenAbove * Time.deltaTime;
                newX = Mathf.MoveTowards(newX, 0, Mathf.Min(normalDecel, speedAboveMax));
            }

            _velocity.x = newX;
        }
        
        /// <summary>
        /// Runs vertical movement logic.
        /// </summary>
        /// <param name="yInput">Vertical input axis</param>
        private void VerticalMovementLogic(float yInput)
        {
            float originalY = _velocity.y;
            float newY = originalY;

            if (yInput != 0)
            {
                float normalAccel = accel * Time.deltaTime;
                if (originalY > 0 && yInput < 0)
                {
                    newY -= Mathf.Min(Mathf.Max(originalY, normalAccel), normalAccel * turnAroundMultiplier);
                }
                else if (originalY < 0 && yInput > 0)
                {
                    newY += Mathf.Min(Mathf.Max(-originalY, normalAccel), normalAccel * turnAroundMultiplier);
                }
                else if (Mathf.Abs(originalY) < maxSpeed)
                {
                    newY += yInput * Mathf.Min(maxSpeed - Mathf.Abs(originalY), normalAccel);
                }
            }
            else
            {
                // newY = Mathf.MoveTowards(newY, 0, decel * Time.deltaTime);
                newY += Physics2D.gravity.y * Time.deltaTime;
            }

            float speedAboveMax = Mathf.Abs(originalY) - maxSpeed;
            if (speedAboveMax > 0)
            {
                float normalDecel = decelWhenAbove * Time.deltaTime;
                newY = Mathf.MoveTowards(newY, 0, Mathf.Min(normalDecel, speedAboveMax));
            }

            _velocity.y = newY;
        }

        /// <summary>
        /// Runs jumping logic.
        ///
        /// The player's allowed to jump if:
        /// <list type="bullet">
        /// <item><description>Player is grounded or has recently left a surface moving downward
        /// (as specified by <see cref="_lateJumpTime"/> being greater than 0)</description></item>
        /// <item><description>Player has pressed the jump button OR has pressed a little bit early
        /// (as specified by <see cref="_earlyJumpTime"/> being greater than 0)</description></item>
        /// </list>
        /// </summary>
        /// <param name="jumpButtonDown"></param>
        /// <param name="jumpButton"></param>
        private void JumpLogic(bool jumpButtonDown, bool jumpButton)
        {
            //jump
            if (Grounded || _lateJumpTime > 0)
            {
                if (jumpButtonDown || _earlyJumpTime > 0)
                {
                    _velocity.y = jumpForce * shortJumpPercentage;
                    _isJumping = true;
                    _earlyJumpTime = 0;
                    _lateJumpTime = 0;
                }
            }
            //pressing jump early
            else if (jumpButtonDown)
            {
                _earlyJumpTime = earlyJumpLeeway;
            }
            // if player exceeded max jump time or is no longer holding jump stop jumping
            if (_isJumping && (_jumpTime > maxJumpTime || (!jumpButton && _jumpTime > minJumpTime) || HitCeiling))
            {
                _isJumping = false;
                _jumpTime = 0;
            }
            if (!jumpButtonDown)
            {
                _earlyJumpTime -= Time.deltaTime;
                _lateJumpTime -= Time.deltaTime;
            }
            //holding jump
            if (_isJumping)
            {
                _velocity.y = jumpForce * shortJumpPercentage;
                if (_jumpTime > maxJumpTime)
                {
                    _isJumping = false;
                }
                _jumpTime += Time.deltaTime;
            }
        }

        private void GrappleHookLogic()
        {
            if (!Input.GetKeyDown(KeyCode.E) || !CanGrapple || !CrossRunInfo.HasUpgrade(PermUpgradeType.GrapplingHook)) return;
            if (_activeGrappleHook) Destroy(_activeGrappleHook.gameObject);
            if (GrappleHookCooldown <= 0)
            {
                _timeOfGrapple = Time.time;
                GameObject go = Instantiate(grapplePrefab, _body.position, Quaternion.identity);
                Vector2 worldMousePos = _mainCam.ScreenToWorldPoint(Input.mousePosition);
                go.GetComponent<Rigidbody2D>().linearVelocity =
                    ((Vector2)transform.InverseTransformPoint(worldMousePos)).normalized * grappleVelocity +
                    _velocity;
                _activeGrappleHook = go.GetComponent<GrappleHook>();
            }
        }

        public void OnGrappleOOB()
        {
            _activeGrappleHook = null;
        }

        public void OnGrappleExpire()
        {
            _activeGrappleHook = null;
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
            float yInput = Input.GetAxisRaw("Vertical");
            if (!Grounded && !IsOnLeftWall && !IsOnRightWall)
            {
                _velocity += Physics2D.gravity * Time.deltaTime;
            }
            else
            {
                VerticalMovementLogic(yInput);
            }
            float xInput = Input.GetAxisRaw("Horizontal");
            bool jumpButtonDown = Input.GetButtonDown("Jump");
            bool jumpButton = Input.GetButton("Jump");
            bool holdingDown = Input.GetButton("Down");
            switch (xInput)
            {
                case > 0 when IsOnRightWall:
                case < 0 when IsOnLeftWall:
                    xInput = 0;
                    break;
            }

            Vector2 screenPoint = _mainCam.ScreenToWorldPoint(Input.mousePosition);
            float xDiff = screenPoint.x - _body.position.x;
            if (!Mathf.Approximately(xDiff, 0))
            {
                _sprite.flipX = xDiff < 0;
            }
            HorizontalMovementLogic(xInput);
            GrappleHookLogic();

            WallJumpLogic(jumpButtonDown);

            float speedAboveMax = _velocity.y - maxYSpeed;
            if (speedAboveMax > 0)
            {
                float normalDecel = decelYWhenAbove * Time.deltaTime;
                _velocity.y = Mathf.MoveTowards(_velocity.y, 0, Mathf.Min(normalDecel, speedAboveMax));
            }
            
            JumpLogic(jumpButtonDown, jumpButton);
            DashLogic(xInput);

            if (holdingDown)
            {
                foreach (PlatformTileScript platform in _platformsOn) platform.TemporarilyIgnore();
                _platformsOn.Clear();
            }

            foreach (IPlayerVelocityEffector effector in _playerVelocityEffectors)
            {
                _velocity = effector.GetNewVelocity(_velocity, this);
            }

            Rigidbody2D.SlideResults results = _body.Slide(_velocity, Time.deltaTime, slideMovement);
            // Debug.Log(results.remainingVelocity + "," + results.slideHit.collider.gameObject.name + "," + results.surfaceHit.collider.gameObject.name);
            //_body.Slide(_velocity, Time.deltaTime, slideMovement);

            // Update animator
            _animator.SetBool(IsJumpingAnimatorHash, !Grounded);
            _animator.SetFloat(XVelocityAnimatorHash, Mathf.Abs(_velocity.x));
        }

        public void RequestKnockback(Vector2 dir, float str, bool isWeapon = false) => RequestKnockback(dir * str, isWeapon);

        public void RequestKnockback(Vector2 vec, bool isWeapon = false)
        {
            if (isWeapon)
            {
                if (Grounded) vec *= groundedKBMultiplier;
            }
            // honestly shouldn't really matter if it's here or just an addForce call
            // but this *feels* slower/unclean but idk
            _velocity += vec;
        }

        public void OnEnemyHook(EntityHealth health)
        {
            if (CrossRunInfo.HasUpgrade(PermUpgradeType.TargetedMomentum))
            {
                _dashesRemaining = maxDashes;
            }
        }

        public void OnEnemyKill()
        {
            if (GameEndCanvas.Instance)
            {
                GameEndCanvas.Instance.endData.enemiesKilled++;
            }

            if (!Grounded && _upgradeManager[UpgradeType.KeepingInStride] > 0)
            {
                _hasKeepingInStrideDash = true;
            }

            if (_activeGrappleHook && _upgradeManager[UpgradeType.RenewedVigor] > 0)
            {
                _dashesRemaining = maxDashes;
            }
        }

        private void DashLogic(float xInput)
        {
            if (!Input.GetButtonDown("Dash")) return;
            if (!_dashing && HasDash && !Grounded && !Mathf.Approximately(xInput, 0))
            {
                StartCoroutine(DashCoroutine(new Vector2(Mathf.Sign(xInput), 0)));
                if (_dashesRemaining > 0) _dashesRemaining--;
                else if (_hasKeepingInStrideDash) _hasKeepingInStrideDash = false;
                else if (_hasMomentumDash) _hasMomentumDash = false;
            }
        }

        private void WallJumpLogic(bool jumpButtonDown)
        {
            if (!jumpButtonDown) return;
            if (!Grounded && _timeOnWall > minTimeBeforeWallJump)
            {
                if (IsOnLeftWall)
                {
                    _velocity += wallJumpForce;
                    OnWallLaunch();
                }
                else if (IsOnRightWall)
                {
                    _velocity += new Vector2(-wallJumpForce.x, wallJumpForce.y);
                    OnWallLaunch();
                }
            }

            if (_activeGrappleHook)
            {
                Destroy(_activeGrappleHook.gameObject);
            }
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
                _dashesRemaining = maxDashes;
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!_dashing || _upgradeManager[UpgradeType.CloakAndDagger] <= 0) return;
            EntityHealth health = other.collider.GetComponent<EntityHealth>();
            if (health == null) return;
            health.TakeDamage(_upgradeManager.GetData(UpgradeType.CloakAndDagger));
        }

        private IEnumerator DashCoroutine(Vector2 dashDirection)
        {
            _dashing = true;
            if (dashStartDelay > 0)
            {
                _body.constraints = RigidbodyConstraints2D.FreezeAll;
                yield return new WaitForSeconds(dashStartDelay);
                _body.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            _dashVfx.StartDash(_sprite.flipX);

            if (_upgradeManager[UpgradeType.SleightOfPaws] > 0) _weapon.ImmediateReload();
            Vector2 dashVel = dashDirection * dashSpeed;
            for (float time = 0; time < ActualDashTime; time += Time.deltaTime)
            {
                
                _velocity = dashVel;
                yield return null;
            }
            _dashing = false;
            _dashVfx.StopDash();
        }

        public void AddPlatformOn(PlatformTileScript platform)
        {
            _platformsOn.Add(platform);
        }

        public void RemovePlatformOn(PlatformTileScript platform)
        {
            _platformsOn.Remove(platform);
        }

        public void ClearPlatformsOn()
        {
            _platformsOn.Clear();
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (_velocity.y < 0)
            {
                _lateJumpTime = lateJumpLeeway;
            }
            _timeOnWall = 0;
        }
    }
}
