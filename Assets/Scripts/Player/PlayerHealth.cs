using System.Collections;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(PlayerMovementScript))]
    public class PlayerHealth : EntityHealth
    {
        private PlayerMovementScript _playerMovement;
        private SpriteRenderer _playerSprite;

        protected override void Awake()
        {
            base.Awake();
            _playerMovement = GetComponent<PlayerMovementScript>();
            _playerSprite = GetComponent<SpriteRenderer>();
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
            Physics2D.IgnoreLayerCollision(6, 7, true);
            for (float i = 0; i < iFrameLength; i += 0.2f)
            {
                _playerSprite.color = new Color(1, 1, 1, 0.5f);
                yield return new WaitForSeconds(0.1f);
                _playerSprite.color = Color.white;
                yield return new WaitForSeconds(0.1f);
            }
            Physics2D.IgnoreLayerCollision(6, 7, false);
        }
    }
}
