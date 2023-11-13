using System.Collections;
using System.Collections.Generic;
using GameEnd;
using Player;
using UnityEngine;
using Util;

namespace Enemies
{
    public class BossEnemy : MonoBehaviour
    {

        [SerializeField, Min(0)] private float speed;
        [SerializeField] private DelayedDeathZone[] deathZones;
        [SerializeField, Min(0)] private float delayBetweenZones = 15f;
        [SerializeField, Min(0)] private int numberOfZones = 2;
        [SerializeField, Min(0)] private float warningDuration = 3f;
        [SerializeField, Min(0)] private float deathDuration = 1f;
        [SerializeField, Min(0)] private int zoneDamage = 5;
        [SerializeField] private GameObject minionPrefab;
        [SerializeField, Min(0)] private float delayBetweenSummons = 5f;
        [SerializeField, Min(0)] private int numberOfSummons = 3;
        [SerializeField] private Vector3[] summonLocations;
        [SerializeField] private bool summonsUseWorld = true;

        private readonly List<GameObject> _summons = new();
        private PlayerMovementScript _playerMovement;
        private PlayerHealth _playerHealth;
        private bool _active;

        private IEnumerator _zonesCoroutine;
        private IEnumerator _summonCoroutine;

        private void Awake()
        {
            _playerMovement = PlayerMovementScript.Instance;
            _playerHealth = PlayerHealth.Instance;
            GetComponent<EntityHealth>().OnDeath += _ =>
            {
                if (GameEndCanvas.Instance)
                {
                    GameEndCanvas.Instance.DisplayAfterDelay(1, true);
                }
            };
        }

        private void FixedUpdate()
        {
            if (_active)
                transform.position = Vector3.MoveTowards(transform.position, _playerMovement.transform.position,
                    speed * Time.fixedDeltaTime);
        }

        public void StartBoss()
        {
            _active = true;
            _zonesCoroutine = ZonesCoroutine();
            _summonCoroutine = SummonCoroutine();
            StartCoroutine(_zonesCoroutine);
            StartCoroutine(_summonCoroutine);
        }

        public void StopBeingActive()
        {
            _active = false;
            StopCoroutine(_summonCoroutine);
            StopCoroutine(_zonesCoroutine);
            foreach (GameObject go in _summons)
            {
                Destroy(go);
            }
            _summons.Clear();
        }
        private IEnumerator ZonesCoroutine()
        {
            while (_playerHealth.health > 0)
            {
                yield return new WaitForSeconds(delayBetweenZones);
                
                List<int> randVals = RandValues(numberOfZones, deathZones.Length);
                for (int i = 0; i < numberOfZones; i++)
                {
                    deathZones[randVals[i]].StartZone(warningDuration, deathDuration, zoneDamage);
                }
            }
        }
        
        private IEnumerator SummonCoroutine()
        {
            while (_playerHealth.health > 0)
            {
                yield return new WaitForSeconds(delayBetweenSummons);
                List<int> randVals = RandValues(numberOfSummons, summonLocations.Length);
                Transform parent = transform.parent;
                for (int i = 0; i < numberOfSummons; i++)
                {
                    Vector3 position = summonLocations[randVals[i]];
                    if (!summonsUseWorld) position = parent.TransformPoint(position);
                    GameObject newSummon = Instantiate(minionPrefab, position, Quaternion.identity, parent);
                    newSummon.GetComponent<EntityHealth>().OnDeath += OnSummonDeath;
                    _summons.Add(newSummon);
                }
            }
        }

        private void OnSummonDeath(EntityHealth health)
        {
            _summons.Remove(health.gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            foreach (Vector3 vec in summonLocations)
            {
                Gizmos.DrawWireSphere(vec, 1);
            }
        }

        private static List<int> RandValues(int number, int max)
        {
            List<int> retVal = new();
            List<int> randIndices = new();
            for (int i = 0; i < max; i++) randIndices.Add(i);
            for (int i = 0; i < number; i++)
            {
                retVal.Add(randIndices.RemoveRandom());
            }

            return retVal;
        }
    }
}