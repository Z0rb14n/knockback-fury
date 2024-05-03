using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.Events;

namespace Enemies
{
    [RequireComponent(typeof(EntityHealth))]
    public class McCoffeeBossEnemy : AbstractBossEnemy
    {
        [SerializeField] private Transform damageCone;
        [SerializeField] private GameObject projectile;
        [SerializeField, Min(1)] private int numProjectiles = 1;
        [SerializeField, Min(0)] private float speed = 2;
        [SerializeField, Min(0)] private float spread = 15;
        public UnityEvent eventOnDeath;
        
        private EntityHealth _entityHealth;
        private PlayerMovementScript _playerMovement;

        private bool _active;

        private void Awake()
        {
            _entityHealth = GetComponent<EntityHealth>();
            _entityHealth.OnDeath += OnDeath;
            _playerMovement = PlayerMovementScript.Instance;
        }

        private void OnDeath(EntityHealth health)
        {
            eventOnDeath?.Invoke();
        }

        private void FixedUpdate()
        {
            if (_active)
                transform.position = Vector3.MoveTowards(transform.position, _playerMovement.transform.position,
                    speed * Time.fixedDeltaTime);
        }

        public override void StartBoss()
        {
            _active = true;
            StartCoroutine(OccasionallyFireCoroutine());
        }

        private float SetDamageConeAngle()
        {
            Vector2 diff = _playerMovement.Pos - (Vector2)transform.position;
            float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            damageCone.localEulerAngles = new Vector3(0, 0, angle);
            return angle;
        }

        private void GenerateProjectiles(float startAngle)
        {
            Vector2 pos = transform.position;
            for (int i = 0; i < numProjectiles; i++)
            {
                float actualAngle = (startAngle + Random.Range(-spread / 2, spread / 2)) * Mathf.Deg2Rad;
                GameObject go = Instantiate(projectile, pos, Quaternion.identity);
                go.GetComponent<McCoffeeProjectile>().Initialize(actualAngle);
            }
        }

        private IEnumerator OccasionallyFireCoroutine()
        {
            while (_entityHealth.health > 0 && _active)
            {
                yield return new WaitForSeconds(2);
                float prevSpeed = speed;
                damageCone.gameObject.SetActive(true);
                speed = 0;
                float angle = SetDamageConeAngle();
                yield return new WaitForSeconds(1);
                damageCone.gameObject.SetActive(false);
                speed = prevSpeed;
                GenerateProjectiles(angle);
            }
        }
    }
}