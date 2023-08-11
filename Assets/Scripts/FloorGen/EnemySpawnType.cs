using System;

namespace FloorGen
{
    [Flags]
    public enum EnemySpawnType
    {
        Jumper = 1,
        Heavy = 2,
        Ranged = 4,
        Chaser = 8
    }
}