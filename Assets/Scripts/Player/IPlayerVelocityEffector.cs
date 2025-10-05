using UnityEngine;

namespace Player
{
    public interface IPlayerVelocityEffector
    {
        public Vector2 GetNewVelocity(Vector2 oldVelocity, PlayerMovementScript player);
    }
}
