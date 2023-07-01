using System.Collections;
using UnityEngine;

namespace Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(PlayerMovementScript), typeof(Rigidbody2D))]
    public class PlayerWeaponControl : MonoBehaviour
    {
        public static PlayerWeaponControl Instance
        {
            get
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                if (_instance == null) _instance = FindObjectOfType<PlayerWeaponControl>();
                return _instance;
            }
        }
        private static PlayerWeaponControl _instance;

        public float AdrenalineDamageBoost => adrenalineBoost * Mathf.Min(_currAdrenalineStacks, maxAdrenalineStacks);
        public float StabilizedAimDamageBoost => stabilizedAimBoost * Mathf.Min(_stabilizedAimStacks, maxStabilizedAimStacks);
        public float FirstStrikeDamageBoost => _isFirstStrikeActive ? 0 : firstStrikeBoost;
        
        [SerializeField] private int maxAdrenalineStacks = 4;
        [SerializeField] private float adrenalineLength = 4;
        [SerializeField, Min(0)] private float adrenalineBoost = 0.15f;
        [SerializeField] private int maxStabilizedAimStacks = 4;
        [SerializeField] private float stabilizedAimLength = 5;
        [SerializeField, Min(0)] private float stabilizedAimBoost = 0.15f;
        [SerializeField, Min(0)] private float firstStrikeBoost = 0.5f;
        [SerializeField, Range(0,100)] public float lastStrikeBoost = 50;

        private PlayerMovementScript _playerMovement;
        private Weapons.Weapon _weapon;
        private Camera _cam;
        private Rigidbody2D _body;
        private int _currAdrenalineStacks;
        private int _stabilizedAimStacks;
        private IEnumerator _stabilizedAimCoroutine;
        private bool _isFirstStrikeActive;

        private void Awake()
        {
            _instance = this;
            _weapon = GetComponentInChildren<Weapons.Weapon>();
            _cam = Camera.main;
            _body = GetComponent<Rigidbody2D>();
            _playerMovement = GetComponent<PlayerMovementScript>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R)) _weapon.Reload();

            if (Input.GetKeyDown(KeyCode.LeftControl)) _weapon.SwitchWeapon(true);
            
            if (Input.mouseScrollDelta.y != 0) _weapon.SwitchWeapon(Input.mouseScrollDelta.y > 0);

            if (Input.GetMouseButton(0))
            {
                Vector3 worldMousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
                bool fireResult = _weapon.Fire(Input.GetMouseButtonDown(0));
                if (fireResult)
                {
                    _playerMovement.RequestKnockback(((Vector2)(transform.position - worldMousePos)).normalized,
                        _weapon.WeaponData.knockbackStrength);
                    _isFirstStrikeActive = false;
                }
            }

            if (Input.GetMouseButtonDown(1)) _weapon.UseRightClick(_body.velocity);
        }

        public void OnWallLaunch()
        {
            if (PlayerUpgradeManager.Instance[PlayerUpgradeType.Adrenaline] > 0)
            {
                StartCoroutine(AdrenalineCoroutine());
            }
        }

        public void OnStartWallSlide()
        {
            if (PlayerUpgradeManager.Instance[PlayerUpgradeType.StabilizedAim] > 0)
            {
                _stabilizedAimCoroutine = StabilizedAimStartCoroutine();
                StartCoroutine(_stabilizedAimCoroutine);   
            }
        }

        public void OnStopWallSlide()
        {
            if (PlayerUpgradeManager.Instance[PlayerUpgradeType.StabilizedAim] > 0)
            {
                StopCoroutine(_stabilizedAimCoroutine);
            }
        }

        public void OnFinishReload()
        {
            if (PlayerUpgradeManager.Instance[PlayerUpgradeType.FirstStrike] > 0)
            {
                _isFirstStrikeActive = true;
            }
        }

        private IEnumerator StabilizedAimStartCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                StartCoroutine(StabilizedAimBoostCoroutine());
            }
        }

        private IEnumerator StabilizedAimBoostCoroutine()
        {
            _stabilizedAimStacks++;
            yield return new WaitForSeconds(stabilizedAimLength);
            _stabilizedAimStacks--;
        }

        private IEnumerator AdrenalineCoroutine()
        {
            _currAdrenalineStacks++;
            yield return new WaitForSeconds(adrenalineLength);
            _currAdrenalineStacks--;
        }
    }
}