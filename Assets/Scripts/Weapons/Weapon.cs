using System;
using UnityEngine;
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

        public WeaponData weaponData;
        private Camera _mainCam;
        private Vector2 _spriteStartPosition;
        private Vector2 _recoilAnimDisplacement;

        // VARYING
        private float _recoilAnimTimer;
        private float _reloadTimer;
        private float _weaponDelayTimer;
        private float _weaponBurstTimer;
        private int _weaponBurstCount;

        public float ReloadTime => _reloadTimer;

        public Vector2 LookDirection => spritePivot.right;


        private void Awake()
        {
            _mainCam = Camera.main;
            _spriteStartPosition = sprite.transform.localPosition;
            _recoilAnimDisplacement = new Vector2(-0.02f, 0);
            weaponData.Reload();
            UpdateFromWeaponData();
        }

        private void Update()
        {
            PointAtCursor();

            float dt = Time.deltaTime;

            if (_recoilAnimTimer > 0) FireAnimation(dt);

            if (_reloadTimer > 0)
            {
                _reloadTimer -= dt;
                if (_reloadTimer <= 0) weaponData.Reload();
            }

            if (_weaponDelayTimer > 0) _weaponDelayTimer -= dt;
            if (_weaponBurstCount > 0 && _weaponBurstTimer > 0)
            {
                _weaponBurstTimer -= dt;
                if (_weaponBurstTimer <= 0) FireWeaponUnchecked();
            }
        }

        private void UpdateFromWeaponData()
        {
            if (weaponData != null)
                sprite.sprite = weaponData.sprite;
        }

        private void HitscanLogic(bool isMelee, Vector2 vel)
        {
            Vector2 origin = sprite.transform.TransformPoint(_spriteStartPosition);
            Vector2 normalizedLookDirection = LookDirection.normalized;
            float range = isMelee ? weaponData.meleeInfo.meleeRange : weaponData.range;
            // literally hitscan
            // TODO LAYERMASK FOR ENEMIES/PLAYERS
            RaycastHit2D hit = Physics2D.Raycast(origin, normalizedLookDirection, range);
            Vector2 finalPos = ReferenceEquals(hit.collider, null) ? origin + (normalizedLookDirection * range) : hit.point;
            int damage = isMelee
                ? Mathf.RoundToInt(Mathf.Max(0, Vector2.Dot(vel, normalizedLookDirection)) * weaponData.meleeInfo.velMultiplier +
                  weaponData.meleeInfo.baseDamage)
                : weaponData.projectileDamage;
            // TODO DEAL DAMAGE
            Debug.Log(damage);
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
            if (weaponData.isHitscan)
                HitscanLogic(false, Vector2.zero);
            else
            {
                for (int i = 0; i < weaponData.numProjectiles; i++)
                {
                    GameObject go = ReferenceEquals(projectileParent, null)
                        ? Instantiate(projectilePrefab, origin, Quaternion.identity)
                        : Instantiate(projectilePrefab, origin, Quaternion.identity, projectileParent);
                    float angle = Mathf.Atan2(normalizedLookDirection.y, normalizedLookDirection.x);
                    angle += Random.Range(-weaponData.spread/2, weaponData.spread/2) * Mathf.Deg2Rad;
                    Vector2 finalDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    WeaponProjectile proj = go.GetComponent<WeaponProjectile>();
                    proj.Initialize(weaponData.projectileDamage, weaponData.range, weaponData.projectileSpeed, finalDir);
                }
            }

            weaponData.DecrementClip();
            if (weaponData.IsClipEmpty) _reloadTimer = weaponData.reloadTime;
            _weaponDelayTimer = 1/weaponData.roundsPerSecond;
            if (weaponData.fireMode == FireMode.Burst)
            {
                _weaponBurstCount--;
                _weaponBurstTimer = 1/weaponData.burstInfo.withinBurstFirerate;
            }
        }

        /// <summary>
        /// Fire weapon
        /// </summary>
        /// <param name="isFirstDown">Whether the mouse was pressed this frame</param>
        /// <returns>Whether or not a shot was actually fired</returns>
        public bool Fire(bool isFirstDown)
        {
            if (_reloadTimer > 0) return false;
            if (_weaponDelayTimer > 0) return false;
            if (!isFirstDown && weaponData.fireMode != FireMode.Auto) return false;
            if (weaponData.IsClipEmpty)
            {
                _reloadTimer = weaponData.reloadTime;
                return false;
            }
            if (weaponData.fireMode == FireMode.Burst) _weaponBurstCount = weaponData.burstInfo.burstAmount;
            FireWeaponUnchecked();
            return true;
        }

        public void UseMelee(Vector2 vel)
        {
            if (!weaponData.hasMelee) return;
            HitscanLogic(true, vel);
        }

        public void Reload()
        {
            if (_reloadTimer > 0) return;
            _reloadTimer = weaponData.reloadTime;
        }

        private void OnValidate()
        {
            UpdateFromWeaponData();
        }

        /// <summary>
        /// Initialize values and start firing animation
        /// </summary>
        private void StartFireAnimation()
        {
            _recoilAnimTimer = weaponData.recoilAnimationDuration;
        }

        /// <summary>
        /// Update firing animation and values
        /// </summary>
        /// <param name="dt">Typically <code>Time.deltaTime</code></param>
        private void FireAnimation(float dt)
        {
            // can be null but cba
            float weight = _recoilAnimTimer / weaponData.recoilAnimationDuration;
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
    }
}
