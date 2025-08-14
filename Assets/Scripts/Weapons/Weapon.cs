using System;
using System.Linq;
using Enemies.Ranged;
using FileSave;
using GameEnd;
using PermUpgrade;
using Player;
using UnityEngine;
using Upgrades;
using Random = UnityEngine.Random;

namespace Weapons
{
    [RequireComponent(typeof(AudioSource))]
    public class Weapon : MonoBehaviour
    {
        // CONSTANT
        [SerializeField] private Transform spritePivot;
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private Transform projectileParent;
        [SerializeField] private GameObject linePrefab;
        [SerializeField] private GameObject projectilePrefab;
        public WeaponData[] weaponInventory;
        [SerializeField] private int weaponIndex;
        [SerializeField] private LayerMask raycastMask;
        
        public bool IsOneYearOfReloadPossible {
            get
            {
                float max = WeaponData.reloadTime * (1-PlayerUpgradeManager.Instance.oneYearOfReloadPercent);
                float min = max - PlayerUpgradeManager.Instance.oneYearOfReloadTiming;
                return ReloadTime > 0 && PlayerUpgradeManager.Instance[UpgradeType.OneYearOfReload] > 0 && ReloadTime >= min &&
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

        public int NumWeapons => weaponInventory.Count(t => t);

        private Vector2 RandomizedLookDirection
        {
            get
            {
                Vector2 normalizedLookDirection = LookDirection.normalized;
                float angle = Mathf.Atan2(normalizedLookDirection.y, normalizedLookDirection.x);
                angle += Random.Range(-WeaponData.actualSpread/2, WeaponData.actualSpread/2) * Mathf.Deg2Rad;
                return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
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
        private AudioSource _audioSource;

        public float ReloadTime { get; private set; }

        private Vector2 LookDirection => spritePivot.right;
        
        private void Awake()
        {
            _mainCam = Camera.main;
            _spriteStartPosition = sprite.transform.localPosition;
            _recoilAnimDisplacement = new Vector2(-0.02f, 0);
            _audioSource = GetComponent<AudioSource>();
            foreach (WeaponData data in weaponInventory)
            {
                if (data) data.OnAfterDeserialize();
            }
            UpdateFromWeaponData();
            EnsureInventoryHasSpace();
        }

        private void EnsureInventoryHasSpace()
        {
            if (weaponInventory != null && weaponInventory.Length != 0 &&
                (weaponInventory.Length == 3 || !CrossRunInfo.HasUpgrade(PermUpgradeType.ExtraHolster)))
            {
                return;
            }

            WeaponData[] newData = new WeaponData[CrossRunInfo.HasUpgrade(PermUpgradeType.ExtraHolster) ? 3 : 2];
            if (weaponInventory != null)
                Array.Copy(weaponInventory, newData, weaponInventory.Length);
            weaponInventory = newData;
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
                    else
                    {
                        Vector3 mousePos = GetMousePos();
                        PlayerMovementScript.Instance.RequestKnockback((transform.position - mousePos).normalized,
                            WeaponData.actualKnockbackStrength, true);
                        FireWeaponUnchecked(PlayerWeaponControl.Instance.TotalDamageMult);
                    }
                }
            }
        }

        public void UpdateFromWeaponData()
        {
            if (WeaponData == null) return;
            sprite.sprite = WeaponData.sprite;
            if (_audioSource) _audioSource.clip = WeaponData.fireEffect;
        }

        private void HitscanHitLogic(RaycastHit2D hit, bool isMelee, Vector2 vel, Vector2 dir)
        {
            int damage = isMelee
                ? Mathf.RoundToInt(Mathf.Max(0, Vector2.Dot(vel, dir)) * WeaponData.meleeInfo.velMultiplier +
                                   WeaponData.meleeInfo.baseDamage)
                : WeaponData.actualDamage;
            damage += isMelee
                ? Mathf.RoundToInt(damage * PlayerWeaponControl.Instance.NonMeleeDamageBoost)
                : Mathf.RoundToInt(damage * (PlayerWeaponControl.Instance.TotalDamageMult - 1));
            if (!ReferenceEquals(hit.collider, null))
            {
                HitEntity(hit.collider, damage);
            }
        }

        private void HitscanLogic(bool isMelee, Vector2 vel)
        {
            Vector2 origin = sprite.transform.TransformPoint(_spriteStartPosition);
            float range = isMelee ? WeaponData.meleeInfo.meleeRange : WeaponData.actualRange;
            // literally hitscan
            int count = isMelee ? 1 : WeaponData.numProjectiles;
            Physics2D.queriesHitTriggers = false;
            for (int i = 0; i < count; i++)
            {
                Vector2 dir = RandomizedLookDirection;
                Vector2 finalPos = origin + (dir * range);
                if (WeaponData.pierceMode == PierceMode.None)
                {
                    RaycastHit2D hit = Physics2D.Raycast(origin, dir, range, raycastMask);
                    if (!ReferenceEquals(hit.collider, null)) finalPos = hit.point;
                    HitscanHitLogic(hit, isMelee, vel, dir);
                }
                else
                {
                    // yo bro if you hate it so much why don't you fix it while somehow supporting infinite hits
                    // ReSharper disable once Unity.PreferNonAllocApi
                    RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir, range, raycastMask);
                    int pierces = WeaponData.pierceInfo.maxPierces;
                    foreach (RaycastHit2D hit in hits)
                    {
                        // ReSharper disable once Unity.NoNullPropagation
                        EntityHealth health = hit.collider?.GetComponent<EntityHealth>();
                        HitscanHitLogic(hit, isMelee, vel, dir);
                        if (CheckAndUpdatePiercing(WeaponData, health, ref pierces))
                        {
                            finalPos = hit.point;
                            break;
                        }
                    }
                }
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
                    Debug.DrawLine(origin,origin + (dir*range),Color.red,1,false);
                }
            }
            
            Physics2D.queriesHitTriggers = true;

        }

        /// <summary>
        /// Fire weapon: create projectiles and reset timers.
        /// </summary>
        private void FireWeaponUnchecked(float mult)
        {
            if (GameEndCanvas.Instance)
            {
                GameEndCanvas.Instance.endData.shotsFired++;
            }
            StartFireAnimation();
            // instantiate & shoot bullets etc
            Vector2 origin = sprite.transform.TransformPoint(_spriteStartPosition);
            _audioSource.Play();
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
                    WeaponProjectile proj = go.GetComponent<WeaponProjectile>();
                    proj.Initialize(WeaponData, RandomizedLookDirection);
                    proj.ModifyDamage(mult);
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
        /// <param name="mult">Additional damage multiplier, 1 if none</param>
        /// <returns>Whether or not a shot was actually fired</returns>
        public bool Fire(bool isFirstDown, float mult)
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
            FireWeaponUnchecked(mult);
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
            if (WeaponData.Clip == WeaponData.actualClipSize) return;
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
            _weaponDelayTimer = 0;
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
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data">WeaponData of fired weaopn</param>
        /// <param name="other">Hit EntityHealth, if present</param>
        /// <param name="remainingPierces">Number of remaining pierces; modified when method ends</param>
        /// <returns>Whether the GameObject should be destroyed</returns>
        public static bool CheckAndUpdatePiercing(WeaponData data, EntityHealth other, ref int remainingPierces)
        {
            if (data.pierceMode == PierceMode.None) return true;
            bool hasEntity = other;
            if (data.pierceMode == PierceMode.FirstWall && !hasEntity) return true;
            if (remainingPierces <= 0 && !data.pierceInfo.isInfinitePierce) return true;
            if (data.pierceMode == PierceMode.WallAsEnemy && !hasEntity) remainingPierces--;
            if (hasEntity) remainingPierces--;

            return false;
        }

        public static bool HitEntity(Collider2D collider, int damage, int selfDamage = 0)
        {
            EnemyBombScript enemyBomb = collider.GetComponent<EnemyBombScript>();
            if (enemyBomb) enemyBomb.OnHitByPlayer();
            return HitEntityHealth(collider.GetComponent<EntityHealth>(), damage, selfDamage);
        }

        private static bool HitEntityHealth(EntityHealth health, int damage, int selfDamage = 0)
        {
            int finalDamage = damage;
            if (health is PlayerHealth)
            {
                Debug.Log($"[Weapon::HitEntityHealth] Tried to hit player for {damage}");
                finalDamage = selfDamage;
                if (CrossRunInfo.HasUpgrade(PermUpgradeType.GarbageRat)) return false;
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
            }
            // ReSharper disable once UseNullPropagation
            if (ReferenceEquals(health, null)) return false;
            health.TakeDamage(finalDamage);
            return true;

        }
    }
}
