using Player;
using UnityEngine;
using Upgrades;
using Random = UnityEngine.Random;

namespace Weapons
{
    public class Weapon : MonoBehaviour
    {
        // CONSTANT
        [SerializeField] private Transform spritePivot;
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private Transform projectileParent;
        [SerializeField] private GameObject linePrefab;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private WeaponData[] weaponInventory;
        [SerializeField] private int weaponIndex = 0;
        
        public bool IsOneYearOfReloadPossible {
            get
            {
                float max = WeaponData.reloadTime * (1-PlayerUpgradeManager.Instance.oneYearOfReloadPercent);
                float min = max - PlayerUpgradeManager.Instance.oneYearOfReloadTiming;
                return PlayerUpgradeManager.Instance[UpgradeType.OneYearOfReload] > 0 && ReloadTime >= min &&
                       ReloadTime < max;
            }
        }

        public int FirstAvailableInventorySpace
        {
            get
            {
                for (int i = 0; i < weaponInventory.Length; i++)
                {
                    if (weaponInventory[i] == null) return i;
                }

                return -1;
            }
        }

        private const int BogglingEyesMaxDistance = 30;
        private const int BogglingEyesMinDistance = 10;

        public WeaponData WeaponData => weaponInventory[weaponIndex];
        private Camera _mainCam;
        private Vector2 _spriteStartPosition;
        private Vector2 _recoilAnimDisplacement;

        // VARYING
        private float _recoilAnimTimer;
        private float _weaponDelayTimer;
        private float _weaponBurstTimer;
        private int _weaponBurstCount;

        public float ReloadTime { get; private set; }

        private Vector2 LookDirection => spritePivot.right;
        
        private void Awake()
        {
            _mainCam = Camera.main;
            _spriteStartPosition = sprite.transform.localPosition;
            _recoilAnimDisplacement = new Vector2(-0.02f, 0);
            WeaponData.Reload();
            UpdateFromWeaponData();
            EnsureInventoryHasSpace();
        }

        private void EnsureInventoryHasSpace()
        {
            if (weaponInventory != null && weaponInventory.Length != 0) return;
            weaponInventory = new WeaponData[2];
            weaponIndex = 0;
        }

        private void Update()
        {
            PointAtCursor();

            float dt = Time.deltaTime;

            if (_recoilAnimTimer > 0) FireAnimation(dt);

            if (ReloadTime > 0)
            {
                ReloadTime -= dt;
                if (ReloadTime <= 0) ImmediateReload();
            }

            if (_weaponDelayTimer > 0) _weaponDelayTimer -= dt;
            if (_weaponBurstCount > 0 && _weaponBurstTimer > 0)
            {
                _weaponBurstTimer -= dt;
                if (_weaponBurstTimer <= 0)
                {
                    if (WeaponData.IsClipEmpty) ReloadTime = WeaponData.reloadTime;
                    else FireWeaponUnchecked();
                }
            }
        }

        private void UpdateFromWeaponData()
        {
            if (WeaponData != null)
                sprite.sprite = WeaponData.sprite;
        }

        private void HitscanLogic(bool isMelee, Vector2 vel)
        {
            Vector2 origin = sprite.transform.TransformPoint(_spriteStartPosition);
            Vector2 normalizedLookDirection = LookDirection.normalized;
            float range = isMelee ? WeaponData.meleeInfo.meleeRange : WeaponData.range;
            // literally hitscan
            // TODO LAYERMASK FOR ENEMIES/PLAYERS
            RaycastHit2D hit = Physics2D.Raycast(origin, normalizedLookDirection, range);
            Vector2 finalPos = ReferenceEquals(hit.collider, null) ? origin + (normalizedLookDirection * range) : hit.point;
            int damage = isMelee
                ? Mathf.RoundToInt(Mathf.Max(0, Vector2.Dot(vel, normalizedLookDirection)) * WeaponData.meleeInfo.velMultiplier +
                  WeaponData.meleeInfo.baseDamage)
                : WeaponData.projectileDamage;
            if (!ReferenceEquals(hit.collider,null)) HitEntityHealth(hit.collider.GetComponent<EntityHealth>(), damage);
            if (!isMelee)
            {
                GameObject go = ReferenceEquals(projectileParent, null)
                    ? Instantiate(linePrefab)
                    : Instantiate(linePrefab, projectileParent);
                // expensive but required; Fire isn't called every frame(?)
                WeaponRaycastLine line = go.GetComponent<WeaponRaycastLine>();
                line.Initialize(origin, finalPos, 1);
            }
            else
            {
                Debug.DrawLine(origin,origin + (normalizedLookDirection*range),Color.red,1,false);
            }

        }

        /// <summary>
        /// Fire weapon: create projectiles and reset timers.
        /// </summary>
        private void FireWeaponUnchecked()
        {
            StartFireAnimation();
            // instantiate & shoot bullets etc
            Vector2 origin = sprite.transform.TransformPoint(_spriteStartPosition);
            Vector2 normalizedLookDirection = LookDirection.normalized;
            if (WeaponData.isHitscan)
                HitscanLogic(false, Vector2.zero);
            else
            {
                // ReSharper disable once MergeConditionalExpression
                GameObject projectile = WeaponData.customProjectile == null ? projectilePrefab : WeaponData.customProjectile;
                for (int i = 0; i < WeaponData.numProjectiles; i++)
                {
                    GameObject go = ReferenceEquals(projectileParent, null)
                        ? Instantiate(projectile, origin, Quaternion.identity)
                        : Instantiate(projectile, origin, Quaternion.identity, projectileParent);
                    float angle = Mathf.Atan2(normalizedLookDirection.y, normalizedLookDirection.x);
                    angle += Random.Range(-WeaponData.spread/2, WeaponData.spread/2) * Mathf.Deg2Rad;
                    Vector2 finalDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    WeaponProjectile proj = go.GetComponent<WeaponProjectile>();
                    proj.Initialize(WeaponData, finalDir);
                }
            }

            WeaponData.DecrementClip();
            if (WeaponData.IsClipEmpty) ReloadTime = WeaponData.reloadTime;
            _weaponDelayTimer = 1/WeaponData.roundsPerSecond;
            if (WeaponData.fireMode == FireMode.Burst)
            {
                _weaponBurstCount--;
                _weaponBurstTimer = 1/WeaponData.burstInfo.withinBurstFirerate;
            }
        }

        /// <summary>
        /// Fire weapon
        /// </summary>
        /// <param name="isFirstDown">Whether the mouse was pressed this frame</param>
        /// <returns>Whether or not a shot was actually fired</returns>
        public bool Fire(bool isFirstDown)
        {
            if (ReloadTime > 0) return false;
            if (_weaponDelayTimer > 0) return false;
            if (!isFirstDown && WeaponData.fireMode != FireMode.Auto) return false;
            if (WeaponData.IsClipEmpty)
            {
                ReloadTime = WeaponData.reloadTime;
                return false;
            }
            if (WeaponData.fireMode == FireMode.Burst) _weaponBurstCount = WeaponData.burstInfo.burstAmount;
            FireWeaponUnchecked();
            return true;
        }

        public void UseRightClick(Vector2 vel)
        {
            switch (WeaponData.rightClickAction)
            {
                case WeaponRightClickAction.FireModeToggle:
                    (WeaponData.fireMode, WeaponData.altFireMode) = (WeaponData.altFireMode, WeaponData.fireMode);
                    break;
                case WeaponRightClickAction.Melee:
                    HitscanLogic(true, vel);
                    break;
                case WeaponRightClickAction.None:
                default:
                    break;
            }
        }

        public void Reload()
        {
            if (IsOneYearOfReloadPossible) ImmediateReload();
            if (ReloadTime > 0) return;
            if (WeaponData.Clip == WeaponData.clipSize) return;
            ReloadTime = WeaponData.reloadTime;
            if (WeaponData.Clip == 1 && PlayerUpgradeManager.Instance[UpgradeType.LastStrike] > 0)
            {
                ReloadTime *= 1-PlayerWeaponControl.Instance.lastStrikeBoost/100f;
            }
        }

        public void ImmediateReload()
        {
            WeaponData.Reload();
            ReloadTime = 0;
            PlayerWeaponControl.Instance.OnFinishReload();
        }

        public void SwitchWeapon(bool up)
        {
            ReloadTime = 0;
            int newIndex = weaponIndex + (up ? 1 : -1);
            bool shouldDecrement = up || newIndex < 0;
            if (newIndex < 0) newIndex = weaponInventory.Length - 1;
            if (newIndex >= weaponInventory.Length) newIndex = 0;
            if (shouldDecrement)
            {
                for (; newIndex > 0; newIndex--)
                {
                    if (!ReferenceEquals(null, weaponInventory[newIndex])) break;
                }
            }

            weaponIndex = newIndex;
            _weaponDelayTimer = Mathf.Min(_weaponDelayTimer, 1 / WeaponData.roundsPerSecond);
            UpdateFromWeaponData();
        }

        public bool Pickup(WeaponData data)
        {
            int firstAvailableSpace = FirstAvailableInventorySpace;
            if (firstAvailableSpace == -1) return false;
            weaponInventory[firstAvailableSpace] = data;
            return true;
        }

        public WeaponData DropWeapon()
        {
            if (FirstAvailableInventorySpace == 1) return null; // only one weapon; don't drop
            WeaponData toDrop = weaponInventory[weaponIndex];
            for (int i = weaponIndex; i < weaponInventory.Length; i++)
            {
                weaponInventory[i] = i == weaponInventory.Length - 1 ? null : weaponInventory[i + 1];
            }
            SwitchWeapon(false);
            return toDrop;
        }

        public WeaponData SwapWeapon(WeaponData to)
        {
            WeaponData toDrop = weaponInventory[weaponIndex];
            weaponInventory[weaponIndex] = to;
            _weaponDelayTimer = Mathf.Min(_weaponDelayTimer, 1 / WeaponData.roundsPerSecond);
            UpdateFromWeaponData();
            return toDrop;
        }

        private void OnValidate()
        {
            EnsureInventoryHasSpace();
            UpdateFromWeaponData();
        }

        /// <summary>
        /// Initialize values and start firing animation
        /// </summary>
        private void StartFireAnimation()
        {
            _recoilAnimTimer = WeaponData.recoilAnimationDuration;
        }

        /// <summary>
        /// Update firing animation and values
        /// </summary>
        /// <param name="dt">Typically <code>Time.deltaTime</code></param>
        private void FireAnimation(float dt)
        {
            // can be null but cba
            float weight = _recoilAnimTimer / WeaponData.recoilAnimationDuration;
            if (weight <= 0.5)
            {
                weight = 1 - weight;
            }

            weight = (weight - 0.5f) * 2;

            sprite.transform.localPosition = Vector2.Lerp(_recoilAnimDisplacement, _spriteStartPosition, weight);

            _recoilAnimTimer -= dt;
            if (_recoilAnimTimer <= 0)
            {
                sprite.transform.localPosition = _spriteStartPosition;
            }
        }

        /// <summary>
        /// Return world mouse coordinates
        /// </summary>
        private Vector2 GetMousePos()
        {
            return _mainCam.ScreenToWorldPoint(Input.mousePosition);
        }

        /// <summary>
        /// Make the sprite pivot point towards to cursor
        /// </summary>
        private void PointAtCursor()
        {
            Vector2 pivotPoint = spritePivot.position;
            Vector2 mousePos = GetMousePos();
            spritePivot.right = mousePos - pivotPoint;
            sprite.flipY = mousePos.x < pivotPoint.x;
        }

        public static void HitEntityHealth(EntityHealth health, int damage)
        {
            int finalDamage = damage;
            if (health is PlayerHealth)
            {
                Debug.Log($"[Raycast] Hit player for {damage}");
            }
            else if (!ReferenceEquals(health,null))
            {
                if (PlayerUpgradeManager.Instance[UpgradeType.BogglingEyes] > 0)
                {
                    float dist = ((Vector2)(PlayerUpgradeManager.Instance.transform.position - health.transform.position)).magnitude;
                    if (dist >= BogglingEyesMaxDistance)
                        finalDamage += damage;
                    else if (dist >= BogglingEyesMinDistance)
                    {
                        float boost = (dist - BogglingEyesMinDistance) /
                                      (BogglingEyesMaxDistance - BogglingEyesMinDistance);
                        finalDamage += Mathf.RoundToInt(damage * boost);
                    }
                }
                finalDamage += Mathf.RoundToInt(damage * PlayerWeaponControl.Instance.AdrenalineDamageBoost);
                finalDamage += Mathf.RoundToInt(damage * PlayerWeaponControl.Instance.StabilizedAimDamageBoost);
                finalDamage += Mathf.RoundToInt(damage * PlayerWeaponControl.Instance.FirstStrikeDamageBoost);
            }
            // ReSharper disable once UseNullPropagation
            if (!ReferenceEquals(health,null))
                health.TakeDamage(finalDamage);
        }
    }
}
