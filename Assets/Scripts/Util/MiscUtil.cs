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
            int enemyLayerID2 = LayerMask.NameToLayer("EnemyIgnorePlatform");
            Physics2D.IgnoreLayerCollision(playerLayerID, enemyLayerID, false);
            Physics2D.IgnoreLayerCollision(playerLayerID, enemyLayerID2, false);
        }
        
        /**
         * Disable player collision with all enemy layers.
         */
        public static void DisablePlayerEnemyCollision()
        {
            int playerLayerID = LayerMask.NameToLayer("Player");
            int enemyLayerID = LayerMask.NameToLayer("Enemy");
            int enemyLayerID2 = LayerMask.NameToLayer("EnemyIgnorePlatform");
            Physics2D.IgnoreLayerCollision(playerLayerID, enemyLayerID, true);
            Physics2D.IgnoreLayerCollision(playerLayerID, enemyLayerID2, true);
        }
    }
}