using UnityEngine;

namespace Enemies.Cat
{
    [RequireComponent(typeof(EntityHealth), typeof(LineRenderer))]
    public class CatInvulnMachine : MonoBehaviour
    {
        [SerializeField] private CatBossPhaseTwo catBoss;
        private EntityHealth _health;
        private LineRenderer _lineRenderer;
        private HarmlessCat[] _harmlessCats;

        private void Awake()
        {
            _health = GetComponent<EntityHealth>();
            _lineRenderer = GetComponent<LineRenderer>();
            _harmlessCats = GetComponentsInChildren<HarmlessCat>();
            _health.OnDeath += OnDeath;
            _lineRenderer.SetPosition(0, transform.position);
            foreach (HarmlessCat cat in _harmlessCats)
            {
                if (cat) cat.gameObject.transform.parent = transform.parent;
            }
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