using UnityEngine;

namespace Enemies.Cat
{
    [RequireComponent(typeof(Collider2D))]
    public class CatGoonTrigger : MonoBehaviour
    {
        private CatGoon _catGoon;
        private void Awake()
        {
            _catGoon = GetComponentInParent<CatGoon>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                _catGoon.StartAttackingPlayer();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                _catGoon.StopAttackingPlayer();
            }
        }
    }
}