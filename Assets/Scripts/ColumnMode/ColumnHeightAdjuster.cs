using UnityEngine;

namespace ColumnMode
{
    /// <summary>
    /// Thing that adjusts the column height. It's so much easier.
    /// </summary>
    public class ColumnHeightAdjuster : MonoBehaviour
    {
        [SerializeField, Min(20), Tooltip("Height of the scene")]
        private float height = 400;

        [Header("Objects to adjust don't worry about it")]
        [SerializeField]
        private BoxCollider2D leftWall;
        [SerializeField]
        private WallHeightMatcher leftWallHeightMatcher;
        [SerializeField]
        private BoxCollider2D rightWall;
        [SerializeField]
        private WallHeightMatcher rightWallHeightMatcher;
        [SerializeField]
        private Transform ceiling;

        [SerializeField] private Transform upperPlatform;
        [SerializeField] private RisingLava risingLava;
        [SerializeField] private ColumnModeGenerator columnModeGenerator;
        [SerializeField] private Transform bossLocation;

        private void Awake()
        {
            Set();
        }

        public void Set()
        {
            leftWall.size = new Vector2(3, height * 2 + 44);
            leftWallHeightMatcher.max = height - 3.5f;
            rightWall.size = new Vector2(3, height * 2 + 20);
            rightWallHeightMatcher.max = height - 15;
            ceiling.position = new Vector3(0, height + 20, 0);
            upperPlatform.position = new Vector3(0, height + 9.75f, 0);
            risingLava.max = height - 0.5f;
            columnModeGenerator.maxGeneration = height;
            bossLocation.position = new Vector3(bossLocation.position.x, height + 20.5f, 0);
        }
    }
}
