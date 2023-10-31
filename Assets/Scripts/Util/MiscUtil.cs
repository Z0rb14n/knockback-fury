using UnityEngine;

namespace Util
{
    public static class MiscUtil
    {
        /**
         * Enable the player to collide with all enemy layers.
         */
        public static void EnablePlayerEnemyCollision()
        {
            int playerLayerID = LayerMask.NameToLayer("Player");
            int enemyLayerID = LayerMask.NameToLayer("Enemy");
            Physics2D.IgnoreLayerCollision(playerLayerID, enemyLayerID, false);
        }
        
        /**
         * Disable player collision with all enemy layers.
         */
        public static void DisablePlayerEnemyCollision()
        {
            int playerLayerID = LayerMask.NameToLayer("Player");
            int enemyLayerID = LayerMask.NameToLayer("Enemy");
            Physics2D.IgnoreLayerCollision(playerLayerID, enemyLayerID, true);
        }
    }
}