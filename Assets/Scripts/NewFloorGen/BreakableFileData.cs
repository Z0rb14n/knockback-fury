using UnityEngine;

namespace NewFloorGen
{
    [CreateAssetMenu(fileName = "NewBreakableFile")]
    public class BreakableFileData : ScriptableObject
    {
        public int health;
        public bool hasInfiniteHealth;
        public string title;
        public Sprite image;
    }
}