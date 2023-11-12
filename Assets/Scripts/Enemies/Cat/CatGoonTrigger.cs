using UnityEngine;

namespace Enemies.Cat
{
    [RequireComponent(typeof(Collider2D))]
    public class CatGoonTrigger : MonoBehaviour
    {
        private CatGoon _catGoon;
        private CatBossPhaseTwo _catBoss;
        private void Awake()
        {
            _catGoon = GetComponentInParent<CatGoon>();
            _catBoss = GetComponentInParent<CatBossPhaseTwo>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (_catGoon) _catGoon.StartAttackingPlayer();
                else if (_catBoss) _catBoss.StartAttackingPlayerWithBat();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                // TODO EXTRACT
                if (_catGoon) _catGoon.StopAttackingPlayer();
                else if (_catBoss) _catBoss.StopAttackingPlayerWithBat();
            }
        }
    }
}