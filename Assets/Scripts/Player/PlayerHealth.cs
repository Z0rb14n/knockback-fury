using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : EntityHealth
{
    private PlayerMovementScript _playerMovement;

    protected override void Awake()
    {
        base.Awake();
        _playerMovement = GetComponent<PlayerMovementScript>();
    }

    /// <summary>
    /// Takes Damage: decreases health, sets iFrame timer, requests death if required, 
    ///               restricts player movement
    /// </summary>
    public override void TakeDamage(int dmg)
    {
        if (_iFrameTimer <= 0)
        {
            health -= dmg;
            _iFrameTimer = iFrameLength;

            if (health <= 0)
            {
                // TODO: player die
                Debug.Log("Player Death");
            }
            else
            {
                _playerMovement.StopMovement();
                StartCoroutine(AllowMovementAfterDelay());
            }
        }
    }

    private IEnumerator AllowMovementAfterDelay()
    {
        yield return new WaitForSeconds(iFrameLength * 0.75f); // disallows input upon damage
        _playerMovement.AllowMovement();
    }
}
