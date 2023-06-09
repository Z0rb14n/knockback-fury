using System.Collections;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(PlayerMovementScript))]
    public class PlayerHealth : EntityHealth
    {
        private PlayerMovementScript _playerMovement;

        protected override void Awake()
        {
            base.Awake();
            _playerMovement = GetComponent<PlayerMovementScript>();
        }

        /// <summary>
        /// Takes Damage: decreases health, sets iFrame timer, restricts player movement
        /// </summary>
        protected override void DoTakeDamage(int dmg)
        {
            base.DoTakeDamage(dmg);
            _playerMovement.StopMovement();
            StartCoroutine(AllowMovementAfterDelay());
        }

        protected override void Die()
        {
            Debug.Log("Player death");
            // TODO: player death
        }

        private IEnumerator AllowMovementAfterDelay()
        {
            yield return new WaitForSeconds(iFrameLength * 0.75f);
            _playerMovement.AllowMovement();
        }
    }
}
