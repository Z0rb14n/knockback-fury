using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Upgrades;
using Weapons;

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
                if (!_instance._initialized) _instance.Initialize();
                return _instance;
            }
        }
        private static PlayerWeaponControl _instance;

        private float AdrenalineDamageBoost => adrenalineBoost * Mathf.Min(_currAdrenalineStacks, maxAdrenalineStacks);
        private float StabilizedAimDamageBoost => stabilizedAimBoost * Mathf.Min(_stabilizedAimStacks, maxStabilizedAimStacks);
        private float FirstStrikeDamageBoost => _isFirstStrikeActive ? firstStrikeBoost : 0;

        public float NonMeleeDamageBoost => AdrenalineDamageBoost + StabilizedAimDamageBoost;
        
        public float TotalDamageMult
        {
            get
            {
                float ret = 1;
                ret += PlayerUpgradeManager.Instance[UpgradeType.Adrenaline] * AdrenalineDamageBoost;
                ret += PlayerUpgradeManager.Instance[UpgradeType.StabilizedAim] * StabilizedAimDamageBoost;
                ret += PlayerUpgradeManager.Instance[UpgradeType.FirstStrike] * FirstStrikeDamageBoost;
                return ret;
            }
        }
        
        [SerializeField] private int maxAdrenalineStacks = 4;
        [SerializeField] private float adrenalineLength = 4;
        [SerializeField, Min(0)] private float adrenalineBoost = 0.15f;
        [SerializeField] private int maxStabilizedAimStacks = 4;
        [SerializeField] private float stabilizedAimLength = 5;
        [SerializeField, Min(0)] private float stabilizedAimBoost = 0.15f;
        [SerializeField, Min(0)] private float firstStrikeBoost = 0.5f;
        [SerializeField, Range(0,100)] public float lastStrikeBoost = 50;
        [SerializeField] private GameObject weaponItemPrefab;

        private PlayerMovementScript _playerMovement;
        private Weapon _weapon;
        private Camera _cam;
        private Rigidbody2D _body;
        private int _currAdrenalineStacks;
        private int _stabilizedAimStacks;
        private IEnumerator _stabilizedAimCoroutine;
        private bool _isFirstStrikeActive;

        public readonly List<WeaponPickup> weaponsOn = new();

        public bool HasWeaponSpace => _weapon.FirstAvailableInventorySpace != -1;

        public int NumWeapons => _weapon.NumWeapons;
        public WeaponData[] GetInventory => _weapon.weaponInventory;

        private WeaponPickup FirstAvailableItem => weaponsOn.FirstOrDefault(t => t.delay <= 0);

        public bool HasNoUpgradedWeapons => _weapon.weaponInventory.All(data => !data || data.numUpgrades == 0);

        private bool _initialized;

        private void Initialize()
        {
            _instance = this;
            _weapon = GetComponentInChildren<Weapon>();
            _cam = Camera.main;
            _body = GetComponent<Rigidbody2D>();
            _playerMovement = GetComponent<PlayerMovementScript>();
            _initialized = true;
        }

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q)) DropWeapon();
            
            if (Input.GetKeyDown(KeyCode.R)) _weapon.Reload();

            if (Input.GetKeyDown(KeyCode.LeftControl)) _weapon.SwitchWeapon(true);
            
            if (Input.mouseScrollDelta.y != 0) _weapon.SwitchWeapon(Input.mouseScrollDelta.y > 0);

            if (Input.GetMouseButton(0))
            {
                Vector3 worldMousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
                bool fireResult = _weapon.Fire(Input.GetMouseButtonDown(0), TotalDamageMult);
                if (fireResult)
                {
                    _playerMovement.RequestKnockback(((Vector2)(transform.position - worldMousePos)).normalized,
                        _weapon.WeaponData.actualKnockbackStrength);
                    _isFirstStrikeActive = false;
                }
            }

            if (Input.GetMouseButtonDown(1)) _weapon.UseRightClick(_body.velocity);
        }

        public void OnWallLaunch()
        {
            if (PlayerUpgradeManager.Instance[UpgradeType.Adrenaline] > 0)
            {
                StartCoroutine(AdrenalineCoroutine());
            }
        }

        public void OnStartWallSlide()
        {
            if (PlayerUpgradeManager.Instance[UpgradeType.StabilizedAim] > 0)
            {
                _stabilizedAimCoroutine = StabilizedAimStartCoroutine();
                StartCoroutine(_stabilizedAimCoroutine);   
            }
        }

        public void OnStopWallSlide()
        {
            if (PlayerUpgradeManager.Instance[UpgradeType.StabilizedAim] > 0)
            {
                StopCoroutine(_stabilizedAimCoroutine);
            }
        }

        public void OnFinishReload()
        {
            if (PlayerUpgradeManager.Instance[UpgradeType.FirstStrike] > 0)
            {
                _isFirstStrikeActive = true;
            }
        }

        public void PickupWeapon(WeaponPickup pickup)
        {
            if (pickup.delay > 0) return;
            if (_weapon.Pickup(pickup.weaponData)) Destroy(pickup.gameObject);
            else weaponsOn.Add(pickup);
        }

        private void DropWeapon()
        {
            if (weaponsOn.Count > 0)
            {
                WeaponPickup pickupSwap = FirstAvailableItem;
                if (pickupSwap != null)
                {
                    WeaponData toDrop = _weapon.SwapWeapon(pickupSwap.weaponData);
                    Destroy(pickupSwap.gameObject);
                    weaponsOn.Remove(pickupSwap);
                    GameObject item = Instantiate(weaponItemPrefab, transform.position, Quaternion.identity);
                    WeaponPickup pickup = item.GetComponent<WeaponPickup>();
                    pickup.UpdateSprite(toDrop);
                    return;
                }
            }
            WeaponData shouldDrop = _weapon.DropWeapon();
            if (ReferenceEquals(shouldDrop, null)) return;
                
            GameObject itemObject = Instantiate(weaponItemPrefab, transform.position, Quaternion.identity);
            WeaponPickup pickupObject = itemObject.GetComponent<WeaponPickup>();
            pickupObject.UpdateSprite(shouldDrop);
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