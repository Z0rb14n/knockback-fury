﻿using System.Collections;
using Enemies.Ranged;
using FMODUnity;
using Player;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace Enemies.Cat
{
    [RequireComponent(typeof(LineRenderer), typeof(SpriteRenderer))]
    public class CatTurret : MonoBehaviour
    {
        [SerializeField, Min(0)] private float delayBeforeFiring;
        [SerializeField, Min(0)] private int numProjectiles = 2;
        [SerializeField] private float animationMoveOffset = 0.1f;
        [SerializeField] private int numAnimationTicks = 10;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform projectileParent;
        [SerializeField] private EventReference sound;
        private SpriteRenderer _spriteRenderer;
        private PlayerMovementScript _player;
        private LineRenderer _line;
        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _line.enabled = false;
            _player = PlayerMovementScript.Instance;
            _spriteRenderer.enabled = false;
        }

        public void Activate()
        {
            _spriteRenderer.enabled = true;
            StartCoroutine(AnimateInchForward());
        }

        public void Fire()
        {
            StartCoroutine(FireCoroutine());
        }

        private IEnumerator AnimateInchForward()
        {
            for (int ticks = 0; ticks < numAnimationTicks; ticks++)
            {
                transform.localPosition += new Vector3(animationMoveOffset / numAnimationTicks, 0, 0);
                yield return new WaitForFixedUpdate();
            }
        }

        private IEnumerator FireCoroutine()
        {
            for (int i = 0; i < numProjectiles; i++)
            {
                Vector3 playerPos = _player.transform.position;
                Vector3 pos = transform.position;
                Vector3 dir = (playerPos - pos).normalized;
                if (Random.Range(0.0f, 1.0f) < 0.3f)
                {
                    dir = (playerPos + new Vector3(0, 3, 0) - pos).normalized;
                }
                else if (Random.Range(0.0f, 1.0f) < 0.3f)
                {
                    dir = VectorUtil.Rotate(dir, 0.3f);
                }

                _line.enabled = true;
                _line.SetPosition(0, pos);
                _line.SetPosition(1, pos + dir * 100);
                transform.localEulerAngles = new Vector3(0, 0, Vector2.Angle(new Vector2(1, 0), dir));
                yield return new WaitForSeconds(delayBeforeFiring);
                _line.enabled = false;
                GameObject go = Instantiate(projectilePrefab, pos, Quaternion.identity, projectileParent);
                EnemyBulletScript bullet = go.GetComponent<EnemyBulletScript>();
                bullet.Initialize(1, dir);
                RuntimeManager.PlayOneShot(sound, pos);
                yield return new WaitForSeconds(delayBeforeFiring);
            }
        }
    }
}