using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace Enemies
{
    public class BossEnemy : MonoBehaviour
    {
        private bool _active;
        public bool IsActive
        {
            get => _active;
            set
            {
                if (value)
                {
                    OnActivate();
                }

                _active = value;
            }
        }

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
        
        private PlayerMovementScript _playerMovement;
        private PlayerHealth _playerHealth;

        private void Awake()
        {
            _playerMovement = PlayerMovementScript.Instance;
            _playerHealth = PlayerHealth.Instance;
        }

        private void FixedUpdate()
        {
            if (IsActive)
                transform.position = Vector3.MoveTowards(transform.position, _playerMovement.transform.position,
                    speed * Time.fixedDeltaTime);
        }

        private void OnActivate()
        {
            StartCoroutine(ZonesCoroutine());
            StartCoroutine(SummonCoroutine());
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
                for (int i = 0; i < numberOfSummons; i++)
                {
                    Instantiate(minionPrefab, summonLocations[randVals[i]], Quaternion.identity);
                }
            }
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
                int index = Random.Range(0, randIndices.Count);
                retVal.Add(randIndices[index]);
                randIndices.SwapRemove(index);
            }

            return retVal;
        }
    }
}