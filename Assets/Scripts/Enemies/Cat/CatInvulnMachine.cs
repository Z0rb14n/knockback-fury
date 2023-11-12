using System;
using UnityEngine;

namespace Enemies.Cat
{
    [RequireComponent(typeof(EntityHealth), typeof(LineRenderer))]
    public class CatInvulnMachine : MonoBehaviour
    {
        [SerializeField] private CatBossPhaseTwo catBoss;
        private EntityHealth _health;
        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _health = GetComponent<EntityHealth>();
            _lineRenderer = GetComponent<LineRenderer>();
            _health.OnDeath += OnDeath;
            _lineRenderer.SetPosition(0, transform.position);
        }

        private void Update()
        {
            _lineRenderer.SetPosition(1,catBoss.transform.position);
        }

        private void OnDeath(EntityHealth obj)
        {
            catBoss.OnInvulnDeath();
        }
    }
}