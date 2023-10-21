namespace Weapons
{
    public enum PierceMode
    {
        /// <summary>
        /// Don't pierce
        /// </summary>
        None,
        /// <summary>
        /// Terminate on first wall interaction
        /// </summary>
        FirstWall,
        /// <summary>
        /// Ignore wall interactions
        /// </summary>
        IgnoreWall,
        /// <summary>
        /// Wall counts as 1
        /// </summary>
        WallAsEnemy
    }
}