using UnityEngine;

namespace FloorGen
{
    public class FloorGenerator : MonoBehaviour
    {
        public Pair[] pairs;
    }

    [System.Serializable]
    public struct Pair
    {
        public RoomType type;
        public GameObject[] go;
    }
}
