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
            StartCoroutine(DisableCollision());
            
        }

        protected override void Die()
        {
            Debug.Log("Player death");
            // TODO: player death
        }

        private IEnumerator AllowMovementAfterDelay()
        {
            yield return new WaitForSeconds(iFrameLength * 0.5f);
            _playerMovement.AllowMovement();
        }

        private IEnumerator DisableCollision()
        {
            int _playerLayerID = LayerMask.NameToLayer("Player");
            int _enemyLayerID = LayerMask.NameToLayer("Enemy");

            Physics2D.IgnoreLayerCollision(_playerLayerID, _enemyLayerID, true);
            for (float i = 0; i < iFrameLength; i += 0.2f)
            {
                _sprite.color = new Color(1, 1, 1, 0.5f);
                yield return new WaitForSeconds(0.1f);
                _sprite.color = Color.white;
                yield return new WaitForSeconds(0.1f);
            }
            Physics2D.IgnoreLayerCollision(_playerLayerID, _enemyLayerID, false);
        }
    }
}
